using Microsoft.AspNetCore.SignalR;
using ChatService.ChatAPI.Models;
using ChatService.ChatAPI.Services;

namespace ChatService.ChatAPI
{
    public class ChatHub : Hub
    {
        private readonly ChatMongoService _chatService;
        private readonly ChatRedisService _redisService;

        public ChatHub(ChatMongoService chatService, ChatRedisService redisService)
        {
            _chatService = chatService;
            _redisService = redisService;
        }

        private string GetDefaultAvatar(string name)
        {
            return $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(name)}&background=random&color=fff";
        }

        // --- CÁC HÀM XỬ LÝ NHÂN VIÊN ---

        // Nhân viên đăng ký vào hàng đợi tư vấn
        public async Task RegisterStaff(string staffId, string staffName, string staffAvatar)
        {
            await _redisService.RegisterStaffOnlineAsync(staffId);
            await _redisService.SetStaffNameAsync(staffId, staffName);
            if (!string.IsNullOrEmpty(staffAvatar))
            {
                await _redisService.SetStaffAvatarAsync(staffId, staffAvatar);
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, "AdminGroup");
        }

        // --- CÁC HÀM XỬ LÝ KHÁCH HÀNG / NGƯỜI DÙNG ---

        // Khách hoặc User bấm tạo chat mới
        public async Task<string> CreateNewChatSession(string userId, string clientType)
        {
            // Nếu là USER đã đăng nhập, kiểm tra xem có phiên ACTIVE nào chưa
            if (clientType == "USER")
            {
                var existingSessions = await _chatService.GetDanhSachPhienAsync();
                var activeSession = existingSessions.FirstOrDefault(p => 
                    p.UserID == Guid.Parse(userId) && 
                    p.TrangThai == "ACTIVE");
                
                if (activeSession != null)
                {
                    return activeSession.Id.ToString();
                }
            }

            var assignedStaffId = await _redisService.AssignLeastBusyStaffAsync();
            var status = assignedStaffId != null ? "ACTIVE" : "QUEUE";
            string? staffName = null;
            string? staffAvatar = null;
            if (assignedStaffId != null)
            {
                staffName = await _redisService.GetStaffNameAsync(assignedStaffId);
                staffAvatar = await _redisService.GetStaffAvatarAsync(assignedStaffId);
            }

            if (staffName == "Administrator") staffName = "Nhân viên";
            var phienMoi = new PhienTroChuyen 
            {
                Id = Guid.NewGuid(),
                UserID = clientType == "GUEST" ? CreateGuidFromString(userId) : Guid.Parse(userId),
                ClientType = clientType,
                StaffID = assignedStaffId ?? "BOT",
                StaffHoTen = staffName ?? (assignedStaffId != null ? "Tư vấn viên" : null),
                StaffAvatar = staffAvatar ?? (assignedStaffId != null ? GetDefaultAvatar(staffName ?? "Staff") : ""),
                TrangThai = status,
                LastMessage = "Bắt đầu cuộc hội thoại",
                LastTime = DateTime.UtcNow,
                UnreadCount = 0
            };

            await _chatService.TaoPhienAsync(phienMoi);

            if (status == "QUEUE")
            {
                await _redisService.AddToWaitingQueueAsync(phienMoi.Id.ToString());
                await Clients.Caller.SendAsync("SessionQueued");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, phienMoi.Id.ToString());
            // Gửi thông báo cho nhân viên nếu có online
            if(assignedStaffId != null) 
            {
                await Clients.Group("AdminGroup").SendAsync("NewChatAssigned", phienMoi);
            }

            return phienMoi.Id.ToString(); 
        }

        private Guid CreateGuidFromString(string input)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
                return new Guid(hash);
            }
        }

        // Mở Chat
        public async Task JoinChatSession(string maPhien)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, maPhien);
        }

        // Staff đánh dấu đã đọc
        public async Task MarkAsRead(string maPhien)
        {
            await _chatService.ResetUnreadAsync(Guid.Parse(maPhien));
        }

        // Guest login → nâng cấp phiên thành USER với tên thật dùng mapping
        public async Task UpgradeSession(string maPhien, string userId, string hoTen, string? avatar = null)
        {
            var phienGuid = Guid.Parse(maPhien);
            var userGuid = Guid.Parse(userId);

            await _redisService.MapSessionToUserAsync(maPhien, userId, hoTen);

            await _chatService.UpgradePhienAsync(phienGuid, userGuid, hoTen, avatar);
            await Clients.Group("AdminGroup").SendAsync("SessionUpgraded", maPhien, hoTen);
        }

        // Khi người dùng tắt khung Chat
        public async Task LeaveChatSession(string maPhien)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, maPhien);
        }

        public async Task SendMessage(HoiThoai tinNhan)
        {
            tinNhan.ThoiGianGui = DateTime.UtcNow;
            tinNhan.TrangThai = "sent";
            if (tinNhan.ClientID == Guid.Empty) 
                tinNhan.ClientID = Guid.NewGuid();

            if (string.IsNullOrEmpty(tinNhan.SenderAvatar))
            {
                tinNhan.SenderAvatar = GetDefaultAvatar(!string.IsNullOrEmpty(tinNhan.SenderName) ? tinNhan.SenderName : "User");
            }

            if (tinNhan.SenderType != "STAFF")
            {
                var phien = await _chatService.GetPhienByIdAsync(tinNhan.MaPhien);
                if (phien == null) return;

                // --- LOGIC REASSIGN NẾU STAFF OFFLINE > 1 NGÀY ---
                if (!string.IsNullOrEmpty(phien.StaffID) && phien.StaffID != "BOT")
                {
                    var isOnline = await _redisService.IsUserOnlineAsync(phien.StaffID);
                    if (!isOnline)
                    {
                        var lastSeen = await _redisService.GetStaffLastSeenAsync(phien.StaffID);
                        // Nếu offline > 1 ngày (hoặc chưa bao giờ seen) thì tìm người mới
                        if (lastSeen == null || DateTime.UtcNow - lastSeen > TimeSpan.FromDays(1))
                        {
                            var newStaffId = await _redisService.AssignLeastBusyStaffAsync();
                            if (newStaffId != null)
                            {
                                var newStaffName = await _redisService.GetStaffNameAsync(newStaffId);
                                var newStaffAvatar = await _redisService.GetStaffAvatarAsync(newStaffId);
                                await _chatService.CapNhatThongTinStaffPhienAsync(phien.Id, newStaffId, newStaffName ?? "Tư vấn viên", newStaffAvatar);
                                    
                                // Trừ workload cho staff cũ
                                await _redisService.DecreaseStaffWorkloadAsync(phien.StaffID);

                                phien.StaffID = newStaffId;
                                await Clients.Group("AdminGroup").SendAsync("SessionReassigned", phien.Id.ToString(), newStaffId);
                            }
                        }
                    }
                }

                if (phien.TrangThai == "CLOSED")
                {
                    var isSuccess = await _chatService.CapNhatTrangThaiPhienAsync(tinNhan.MaPhien, "ACTIVE", "CLOSED");
                    if (isSuccess && !string.IsNullOrEmpty(phien.StaffID) && phien.StaffID != "BOT")
                    {
                        await _redisService.IncreaseStaffWorkloadAsync(phien.StaffID);
                        await Clients.Group("AdminGroup").SendAsync("SessionReopened", tinNhan.MaPhien.ToString());
                    }
                }
            }

            if (tinNhan.SenderType != "STAFF")
            {
                // KIỂM TRA MAPPING
                var (mappedUserId, mappedHoTen) = await _redisService.GetSessionMappingAsync(tinNhan.MaPhien.ToString());
                if (mappedUserId != null)
                {
                    // Nếu đã có mapping -> Ép kiểu tin nhắn thành USER
                    tinNhan.SenderType = "USER";
                    tinNhan.SenderID = Guid.Parse(mappedUserId);
                }
            }

            if (tinNhan.SenderType == "STAFF")
            {
                var staffIdStr = tinNhan.SenderID.ToString();
                var staffName = await _redisService.GetStaffNameAsync(staffIdStr);
                var staffAvatar = await _redisService.GetStaffAvatarAsync(staffIdStr);
                
                if (!string.IsNullOrEmpty(staffName))
                {
                    await _chatService.CapNhatThongTinStaffPhienAsync(tinNhan.MaPhien, staffIdStr, staffName, staffAvatar);
                    // Thông báo cho Khách biết tên nhân viên vừa nhắn (để cập nhật Header real-time)
                    await Clients.Group(tinNhan.MaPhien.ToString()).SendAsync("StaffNameUpdated", staffName);
                }
            }

            // Lưu tin nhắn xuống DB (MongoDB)
            await _chatService.GuiTinNhanAsync(tinNhan);
            await Clients.Group(tinNhan.MaPhien.ToString()).SendAsync("ReceiveNewMessage", tinNhan);
            await Clients.Group("AdminGroup").SendAsync("ReceiveNewMessage", tinNhan);
        }

        // Staff bấm nút "Kết thúc phiên"
        public async Task CloseSession(string maPhien, string staffId)
        {
            var phienGuid = Guid.Parse(maPhien);

            var isSuccess = await _chatService.CapNhatTrangThaiPhienAsync(phienGuid, "CLOSED", "ACTIVE");
            if (isSuccess)
            {
                await _redisService.DecreaseStaffWorkloadAsync(staffId);
                await Clients.Group("AdminGroup").SendAsync("SessionClosed", maPhien);
                await Clients.Group(maPhien).SendAsync("SessionClosed", maPhien);

                // --- HÀNG ĐỢI: Bốc người tiếp theo trong hàng đợi nếu có ---
                var nextSessionId = await _redisService.GetNextInWaitingQueueAsync();
                if (nextSessionId != null)
                {
                    var staffName = await _redisService.GetStaffNameAsync(staffId);

                    // Gán ngay cho Staff vừa rảnh tay này
                    var staffAvatar = await _redisService.GetStaffAvatarAsync(staffId);
                    await _chatService.CapNhatThongTinStaffPhienAsync(Guid.Parse(nextSessionId), staffId, staffName ?? "Tư vấn viên", staffAvatar);
                    await _chatService.CapNhatTrangThaiPhienAsync(Guid.Parse(nextSessionId), "ACTIVE"); // Chuyển từ QUEUE sang ACTIVE
                    await _redisService.IncreaseStaffWorkloadAsync(staffId);

                    // Thông báo cho cả 2 bên
                    await Clients.Group(nextSessionId).SendAsync("SessionAssigned", staffId, staffName ?? "Tư vấn viên");
                    await Clients.Group("AdminGroup").SendAsync("NewChatAssigned", nextSessionId);
                }
            }
        }

        // Staff bấm nút "Mở lại phiên" thủ công
        public async Task ReopenSession(string maPhien, string staffId)
        {
            var phienGuid = Guid.Parse(maPhien);
            var isSuccess = await _chatService.CapNhatTrangThaiPhienAsync(phienGuid, "ACTIVE", "CLOSED");
            if (isSuccess)
            {
                await _redisService.IncreaseStaffWorkloadAsync(staffId);
                await Clients.Group("AdminGroup").SendAsync("SessionReopened", maPhien);
                await Clients.Group(maPhien).SendAsync("SessionReopened", maPhien);
            }
        }
    }
}