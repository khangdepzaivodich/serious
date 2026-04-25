using Microsoft.AspNetCore.Mvc;
using ChatService.ChatAPI.Models;
using ChatService.ChatAPI.Services;
using ChatService.ChatAPI.DTOs;

namespace ChatService.ChatAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    // Đây là 1 API Test nhanh gọn
    public class ChatController : ControllerBase
    {
        private readonly ChatMongoService _chatService;
        private readonly ChatRedisService _redisService;

        public ChatController(
            ChatMongoService chatService, 
            ChatRedisService redisService)
        {
            _chatService = chatService;
            _redisService = redisService;
        }

        // Lấy danh sách các phiên trò chuyện
        [HttpGet("chat-sessions")]
        public async Task<ActionResult<List<PhienTroChuyen>>> GetDanhSachPhien()
        {
            var phienList = await _chatService.GetDanhSachPhienAsync();
            return Ok(phienList);
        }

        // Lấy chi tiết 1 phiên trò chuyện
        [HttpGet("chat-sessions/{idPhien}")]
        public async Task<ActionResult<PhienTroChuyen>> GetPhien(Guid idPhien)
        {
            var phien = await _chatService.GetPhienByIdAsync(idPhien);
            if (phien == null) return NotFound();

            // FALLBACK: Nếu phiên đã gán Staff nhưng chưa có tên hiển thị trong DB
            if (string.IsNullOrEmpty(phien.StaffHoTen) && !string.IsNullOrEmpty(phien.StaffID) && phien.StaffID != "BOT")
            {
                var nameFromRedis = await _redisService.GetStaffNameAsync(phien.StaffID);
                if (!string.IsNullOrEmpty(nameFromRedis))
                {
                    phien.StaffHoTen = nameFromRedis;
                    // Tiện tay cập nhật ngược lại vào MongoDB để lần sau không phải tra cứu nữa
                    await _chatService.CapNhatThongTinStaffPhienAsync(phien.Id, phien.StaffID, nameFromRedis);
                }
            }

            return Ok(phien);
        }

        // Lấy lịch sử hội thoại của 1 phiên
        [HttpGet("messages/{idPhien}")]
        public async Task<ActionResult<List<HoiThoai>>> GetHoiThoai(Guid idPhien)
        {
            var msgs = await _chatService.GetTinNhanTheoPhienAsync(idPhien);
            return Ok(msgs);
        }

        // Lấy phiên chat mới nhất đang hoạt động của User
        [HttpGet("latest-session/{userId}")]
        public async Task<ActionResult<PhienTroChuyen?>> GetLatestActiveSession(string userId)
        {
            if (!Guid.TryParse(userId, out var userGuid)) return BadRequest("Invalid UserId");

            var sessions = await _chatService.GetDanhSachPhienByUserIdAsync(userGuid);
            var activeSession = sessions
                .Where(p => p.TrangThai == "WAITING" || p.TrangThai == "ASSIGNED")
                .OrderByDescending(p => p.ClientType == "USER") 
                .ThenByDescending(p => p.LastTime)
                .FirstOrDefault();
            
            return Ok(activeSession);
        }

        // [API CHÍNH THỨC] - Tạo 1 Phiên Chat Mới
        [HttpPost("create-session")]
        public async Task<IActionResult> TaoPhien([FromBody] CreateSessionRequest request)
        {
            var phienMoi = new PhienTroChuyen
            {
                UserID = request.UserId,
                ThoiGianTao = DateTime.UtcNow,
                TrangThai = "ACTIVE",
                LastMessage = "Bắt đầu cuộc trò chuyện",
                UnreadCount = 0
            };

            await _chatService.TaoPhienAsync(phienMoi);

            return Ok(new { Note = "Đã tạo phiên chat thành công!", Phien = phienMoi });
        }

        // [API CHÍNH THỨC] - Gửi 1 Tin Nhắn Mới vào Phiên
        [HttpPost("send-message")]
        public async Task<IActionResult> GuiTinNhan([FromBody] SendMessageRequest request)
        {
            var tinNhanMoi = new HoiThoai
            {
                MaPhien = request.MaPhien,
                SenderID = request.SenderID,
                SenderType = request.SenderType,
                NoiDung = request.NoiDung,
                ThoiGianGui = DateTime.UtcNow,
                TrangThai = "sent",
                ClientID = request.ClientID ?? Guid.NewGuid()
            };

            await _chatService.GuiTinNhanAsync(tinNhanMoi);

            return Ok(new { Note = "Đã gửi tin nhắn!", TinNhan = tinNhanMoi });
        }

        // [API CHÍNH THỨC] - Online Ping
        [HttpPost("ping-online")]
        public async Task<IActionResult> PingOnline([FromBody] OnlinePingRequest request)
        {
            await _redisService.SetUserOnlineAsync(request.UserId.ToString());
            return Ok(new { Note = "Đã cập nhật trạng thái Online (Chấm xanh) cho người dùng." });
        }

        // [API CHÍNH THỨC] - Lấy trạng thái của 1 User xem có Online không
        [HttpGet("kiem-tra-online/{userId}")]
        public async Task<IActionResult> KiemTraOnline(Guid userId)
        {
            var isOnline = await _redisService.IsUserOnlineAsync(userId.ToString());
            return Ok(new { UserId = userId, IsOnline = isOnline });
        }

        // =======================================================
        // CÁC API TEST NHANH (Dùng trên trình duyệt)
        // =======================================================

        /*
        // 1. Tạo Test 1 phiên + Gửi 1 tin nhắn (Gõ thẳng lên Browser)
        [HttpGet("tao-phien-test")]
        public async Task<IActionResult> TestTaoPhien()
        {
            var phienMoi = new PhienTroChuyen { UserID = Guid.NewGuid() };
            await _chatService.TaoPhienAsync(phienMoi);

            var tinNhanTest = new HoiThoai
            {
                MaPhien = phienMoi.Id,
                SenderID = phienMoi.UserID,
                NoiDung = "Shop ơi, tôi muốn mua áo khoác bò size XL!"
            };
            await _chatService.GuiTinNhanAsync(tinNhanTest);

            return Ok(new { Note = "Đã tạo!", phienMoi, tinNhanTest });
        }

        // --- TEST REDIS ---
        // 2. Test tạo trạng thái 1 User đang Online
        [HttpGet("SetOnline/{userId}")]
        public async Task<IActionResult> SetUserOnline(string userId)
        {
            await _redisService.SetUserOnlineAsync(userId);
            return Ok(new { 
                Note = "Thành công!", 
                Message = $"Đã đánh dấu user [{userId}] là đang Online. (Sẽ tự Offline sau 5 phút nếu không kích hoạt lại)." 
            });
        }

        // 3. Test kiểm tra xem User đó có đang Online không? (để show chấm xanh/chấm xám trong Chat)
        [HttpGet("CheckOnline/{userId}")]
        public async Task<IActionResult> CheckUserOnline(string userId)
        {
            var isOnline = await _redisService.IsUserOnlineAsync(userId);
            return Ok(new { 
                UserId = userId, 
                TrangThai = isOnline ? "Đang Online (Chấm xanh) 🟢" : "Đang Offline (Chấm xám) ⚪" 
            });
        }
        */
    }
}