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

        // --- CÁC HÀM XỬ LÝ NHÂN VIÊN ---

        // Nhân viên đăng nhập vào hệ thống chat
        public async Task RegisterStaff(string staffId)
        {
            await _redisService.RegisterStaffOnlineAsync(staffId);
            await Groups.AddToGroupAsync(Context.ConnectionId, "AdminGroup");
        }

        // --- CÁC HÀM XỬ LÝ KHÁCH HÀNG / NGƯỜI DÙNG ---

        // Khách hoặc User bấm tạo chat mới
        public async Task<string> CreateNewChatSession(string userId, string clientType)
        {
            // Tìm nhân viên rảnh rỗi nhất
            var assignedStaffId = await _redisService.AssignLeastBusyStaffAsync();

            var phienMoi = new PhienTroChuyen 
            {
                Id = Guid.NewGuid(),
                // Tạm thời nếu clientType = GUEST, ta có thể lưu cái userId (Guest_XXX) vào Trường UserID dưới dạng Guid qua convert,
                UserID = clientType == "GUEST" ? CreateGuidFromString(userId) : Guid.Parse(userId),
                ClientType = clientType,
                StaffID = assignedStaffId ?? "BOT",
                TrangThai = "ACTIVE",
                LastMessage = "Bắt đầu cuộc trò chuyện...",
                ThoiGianTao = DateTime.UtcNow,
                LastTime = DateTime.UtcNow
            };

            await _chatService.TaoPhienAsync(phienMoi);

            // Tự động join room cho Khách
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

        // Guest login → nâng cấp phiên thành USER với tên thật
        public async Task UpgradeSession(string maPhien, string userId, string hoTen)
        {
            var phienGuid = Guid.Parse(maPhien);
            var userGuid = Guid.Parse(userId);

            await _chatService.UpgradePhienAsync(phienGuid, userGuid, hoTen);

            // Báo cho Staff đổi tên hiển thị
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

            // AUTO-REOPEN: Nếu phiên đang CLOSED mà user nhắn lại → tự mở lại
            if (tinNhan.SenderType != "STAFF")
            {
                var phien = await _chatService.GetPhienByIdAsync(tinNhan.MaPhien);
                if (phien != null && phien.TrangThai == "CLOSED")
                {
                    await _chatService.CapNhatTrangThaiPhienAsync(tinNhan.MaPhien, "ACTIVE");
                    
                    // Tăng workload Staff cũ lên lại vì phiên mở lại
                    if (!string.IsNullOrEmpty(phien.StaffID) && phien.StaffID != "BOT")
                    {
                        await _redisService.AssignLeastBusyStaffAsync(); // Placeholder: ideally increment specific staff
                    }

                    // Báo cho Staff biết phiên đã mở lại
                    await Clients.Group("AdminGroup").SendAsync("SessionReopened", tinNhan.MaPhien.ToString());
                }
            }

            // Lưu tin nhắn xuống DB (MongoDB)
            await _chatService.GuiTinNhanAsync(tinNhan);

            // Bốc tin nhắn này Broadcast ném về cho 
            await Clients.Group(tinNhan.MaPhien.ToString())
                         .SendAsync("ReceiveNewMessage", tinNhan);

            await Clients.Group("AdminGroup").SendAsync("ReceiveNewMessage", tinNhan);
        }

        // Staff bấm nút "Kết thúc phiên"
        public async Task CloseSession(string maPhien, string staffId)
        {
            var phienGuid = Guid.Parse(maPhien);

            await _chatService.CapNhatTrangThaiPhienAsync(phienGuid, "CLOSED");

            await _redisService.DecreaseStaffWorkloadAsync(staffId);

            await Clients.Group(maPhien).SendAsync("SessionClosed", maPhien);

            await Clients.Group("AdminGroup").SendAsync("SessionClosed", maPhien);
        }

        // Staff bấm nút "Mở lại phiên"
        public async Task ReopenSession(string maPhien, string staffId)
        {
            var phienGuid = Guid.Parse(maPhien);

            // 1. Cập nhật trạng thái trong MongoDB
            await _chatService.CapNhatTrangThaiPhienAsync(phienGuid, "ACTIVE");

            // 2. Broadcast cho AdminGroup
            await Clients.Group("AdminGroup").SendAsync("SessionReopened", maPhien);
            await Clients.Group(maPhien).SendAsync("SessionReopened", maPhien);
        }
    }
}