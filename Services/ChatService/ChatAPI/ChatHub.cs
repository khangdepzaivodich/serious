using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "STAFF")]
        public async Task RegisterStaff(string staffId, string staffName, string staffAvatar)
        {
            await _redisService.RegisterStaffOnlineAsync(staffId);
            await _redisService.SetStaffNameAsync(staffId, staffName);
            if (!string.IsNullOrEmpty(staffAvatar))
            {
                await _redisService.SetStaffAvatarAsync(staffId, staffAvatar);
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, "AdminGroup");

            await ReassignWaitingSessionsAsync();
        }

        // --- CÁC HÀM XỬ LÝ KHÁCH HÀNG / NGƯỜI DÙNG ---

        public async Task<string> CreateNewChatSession(string userId, string clientType)
        {
            if (clientType == "USER")
            {
                if (!Guid.TryParse(userId, out var userGuid))
                {
                    throw new HubException("Invalid User ID format.");
                }

                var existingSessions = await _chatService.GetDanhSachPhienByUserIdAsync(userGuid);
                // Lấy phiên mới nhất (bất kể trạng thái)
                var latestSession = existingSessions.OrderByDescending(p => p.ThoiGianTao).FirstOrDefault();
                
                if (latestSession != null)
                {
                    // Nếu phiên cũ đã đóng, mình mở lại nó luôn để giữ mạch hội thoại
                    if (latestSession.TrangThai == "CLOSED")
                    {
                        await _chatService.CapNhatTrangThaiPhienAsync(latestSession.Id, "WAITING");
                        // Reset cả staff để có thể phân phối lại nếu cần
                        await _chatService.CapNhatThongTinStaffPhienAsync(latestSession.Id, "", "");
                    }
                    return latestSession.Id.ToString();
                }
            }

            var phienMoi = new PhienTroChuyen 
            {
                Id = Guid.NewGuid(),
                UserID = clientType == "USER" ? Guid.Parse(userId) : Guid.Empty,
                ThoiGianTao = DateTime.UtcNow,
                TrangThai = "WAITING",
                LastTime = DateTime.UtcNow,
                ClientType = clientType,
                UnreadCount = 0
            };

            if (clientType == "GUEST")
            {
                // Reset số thứ tự theo ngày: chat:guest_counter:20240425
                var dateKey = DateTime.UtcNow.ToString("yyyyMMdd");
                var guestNumber = await _redisService.GetNextGuestNumberWithDateAsync(dateKey);
                phienMoi.HoTen = $"Khách #{guestNumber}";
            }
            else
            {
                phienMoi.HoTen = "User"; // Sẽ được cập nhật sau
                phienMoi.Avatar = "";
            }

            await _chatService.TaoPhienAsync(phienMoi);
            await Groups.AddToGroupAsync(Context.ConnectionId, phienMoi.Id.ToString());

            // Gửi thông báo cho AdminGroup để nhân viên biết có chat mới cần Pick
            await Clients.Group("AdminGroup").SendAsync("NewChatWaiting", phienMoi);

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

            // Nếu phiên này đang đóng mà User lại Join vào (mở khung chat), 
            // có thể họ muốn chat tiếp -> Tự động mở lại phiên.
            var phienGuid = Guid.Parse(maPhien);
            var phien = await _chatService.GetPhienByIdAsync(phienGuid);
            if (phien != null && phien.TrangThai == "CLOSED")
            {
                await _chatService.CapNhatTrangThaiPhienAsync(phienGuid, "WAITING");
                await _chatService.CapNhatThongTinStaffPhienAsync(phienGuid, "", "");
                
                // Cập nhật lại thông tin mới nhất để gửi cho Admin
                phien.TrangThai = "WAITING";
                phien.StaffID = "";
                phien.StaffHoTen = "";
                await Clients.Group("AdminGroup").SendAsync("NewChatWaiting", phien);
            }
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

            var phien = await _chatService.GetPhienByIdAsync(tinNhan.MaPhien);
            if (phien == null) return;

            if (tinNhan.SenderType != "STAFF")
            {
                // Mở lại session nếu bị đóng
                if (phien.TrangThai == "CLOSED")
                {
                    // Nếu đã từng có staff gán thì quay lại ASSIGNED, nếu chưa thì quay về WAITING
                    var nextStatus = string.IsNullOrEmpty(phien.StaffID) ? "WAITING" : "ASSIGNED";
                    var isSuccess = await _chatService.CapNhatTrangThaiPhienAsync(tinNhan.MaPhien, nextStatus, "CLOSED");
                    if (isSuccess)
                    {
                        await Clients.Group("AdminGroup").SendAsync("SessionReopened", tinNhan.MaPhien.ToString(), nextStatus);
                    }
                }

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
                
                if (!string.IsNullOrEmpty(staffName) && phien.StaffID != staffIdStr && !tinNhan.IsInternalNote)
                {
                    // Tự động gán/Takeover nếu nhân viên nhắn trực tiếp (không phải internal note)
                    await _chatService.CapNhatThongTinStaffPhienAsync(tinNhan.MaPhien, staffIdStr, staffName, staffAvatar);
                    await _chatService.CapNhatTrangThaiPhienAsync(tinNhan.MaPhien, "ASSIGNED");
                    await Clients.Group(tinNhan.MaPhien.ToString()).SendAsync("StaffNameUpdated", staffName);
                    await Clients.Group("AdminGroup").SendAsync("SessionAssigned", tinNhan.MaPhien.ToString(), staffIdStr, staffName);
                }
            }

            // Lưu tin nhắn xuống DB (MongoDB)
            await _chatService.GuiTinNhanAsync(tinNhan);

            // Broadcast: Khách không nhận được Internal Note
            if (!tinNhan.IsInternalNote)
            {
                await Clients.Group(tinNhan.MaPhien.ToString()).SendAsync("ReceiveNewMessage", tinNhan);
            }
            
            // Nhân viên lúc nào cũng nhận được
            await Clients.Group("AdminGroup").SendAsync("ReceiveNewMessage", tinNhan);
        }

        // Staff Take Over hoặc Assign
        [Authorize(Roles = "STAFF")]
        public async Task AssignSession(string maPhien, string staffId)
        {
            var staffName = await _redisService.GetStaffNameAsync(staffId);
            var staffAvatar = await _redisService.GetStaffAvatarAsync(staffId);
            
            var phienGuid = Guid.Parse(maPhien);
            await _chatService.CapNhatThongTinStaffPhienAsync(phienGuid, staffId, staffName ?? "Tư vấn viên", staffAvatar);
            await _chatService.CapNhatTrangThaiPhienAsync(phienGuid, "ASSIGNED");

            await Clients.Group(maPhien).SendAsync("StaffNameUpdated", staffName);
            await Clients.Group("AdminGroup").SendAsync("SessionAssigned", maPhien, staffId, staffName);
        }

        // Staff bấm nút "Kết thúc phiên"
        [Authorize(Roles = "STAFF")]
        public async Task CloseSession(string maPhien, string staffId)
        {
            var phienGuid = Guid.Parse(maPhien);

            // Không cần biết trước đó là ASSIGNED hay WAITING, cứ set sang CLOSED
            var isSuccess = await _chatService.CapNhatTrangThaiPhienAsync(phienGuid, "CLOSED");
            if (isSuccess)
            {
                await Clients.Group("AdminGroup").SendAsync("SessionClosed", maPhien);
                await Clients.Group(maPhien).SendAsync("SessionClosed", maPhien);
            }
        }

        // Staff bấm nút "Mở lại phiên" thủ công
        [Authorize(Roles = "STAFF")]
        public async Task ReopenSession(string maPhien, string staffId)
        {
            var phienGuid = Guid.Parse(maPhien);
            var isSuccess = await _chatService.CapNhatTrangThaiPhienAsync(phienGuid, "ASSIGNED", "CLOSED");
            if (isSuccess)
            {
                await Clients.Group("AdminGroup").SendAsync("SessionReopened", maPhien, "ASSIGNED");
                await Clients.Group(maPhien).SendAsync("SessionReopened", maPhien);
            }
        }

        private async Task ReassignWaitingSessionsAsync()
        {
            // Do logic mới dùng WAITING và Assign thủ công nên không cần tự động Reassign nữa
            await Task.CompletedTask;
        }
    }
}