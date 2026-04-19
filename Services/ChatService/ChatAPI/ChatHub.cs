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
            // Có thể cho staff join vào 1 group riêng để nhận thông báo toàn hệ thống, vd như "AdminGroup"
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
                StaffID = assignedStaffId ?? "BOT",  // Nếu không có ai online, cho 1 con BOT tiếp
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

            return phienMoi.Id.ToString(); // Trả mã phòng cho Frontend Blazor
        }

        private Guid CreateGuidFromString(string input)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
                return new Guid(hash);
            }
        }

        // User bấm mở khung Chat lên (Tham gia vào Phòng/Phiên)
        public async Task JoinChatSession(string maPhien)
        {
            // Nhét id kết nối (Context.ConnectionId) của user này vào 1 cái Group tên là {maPhien}
            await Groups.AddToGroupAsync(Context.ConnectionId, maPhien);
        }

        // Khi người dùng tắt khung Chat
        public async Task LeaveChatSession(string maPhien)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, maPhien);
        }

        // Hàm Gửi Tin Nhắn xịn xò (Frontend gọi hàm này thay vì gọi cái HTTP POST trong ChatController)
        public async Task SendMessage(HoiThoai tinNhan)
        {
            // A. Chuẩn bị dữ liệu hệ thống
            tinNhan.ThoiGianGui = DateTime.UtcNow;
            tinNhan.TrangThai = "sent";
            if (tinNhan.ClientID == Guid.Empty) 
                tinNhan.ClientID = Guid.NewGuid();

            // B. Lưu tin nhắn xuống DB (MongoDB)
            await _chatService.GuiTinNhanAsync(tinNhan);

            // C. QUAN TRỌNG NHẤT: Trực tiếp bốc tin nhắn này Broadcast ném về cho 
            // TẤT CẢ mọi thiết bị đang ở trong cái phòng chat (Mã Phiên) này.
            // Nhờ có thư viện Redis ở Program.cs, thao tác này sẽ kích hoạt chéo trên tất cả các server!!!
            await Clients.Group(tinNhan.MaPhien.ToString())
                         .SendAsync("ReceiveNewMessage", tinNhan);

            // Đồng thời bắn sang màn hình chờ của Nhân Viên để báo có tin nhắn mới (chớp đỏ unread)
            // Ngay cả khi nhân viên chưa bấm join vào room đó.
            await Clients.Group("AdminGroup").SendAsync("ReceiveNewMessage", tinNhan);
        }
    }
}