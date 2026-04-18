using Microsoft.AspNetCore.SignalR;
using ChatService.ChatAPI.Models;

namespace ChatService.ChatAPI
{
    // Đây là "Ngã tư đường" để những ai xài WebSockets kết nối vào
    public class ChatHub : Hub
    {
        private readonly ChatMongoService _chatService;

        public ChatHub(ChatMongoService chatService)
        {
            _chatService = chatService;
        }

        // 1. Khi 1 user bấm mở khung Chat lên (Tham gia vào Phòng/Phiên)
        public async Task JoinChatSession(string maPhien)
        {
            // Nhét id kết nối (Context.ConnectionId) của user này vào 1 cái Group tên là {maPhien}
            await Groups.AddToGroupAsync(Context.ConnectionId, maPhien);
        }

        // 2. Khi người dùng tắt khung Chat
        public async Task LeaveChatSession(string maPhien)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, maPhien);
        }

        // 3. Hàm Gửi Tin Nhắn xịn xò (Frontend gọi hàm này thay vì gọi cái HTTP POST trong ChatController)
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
        }
    }
}