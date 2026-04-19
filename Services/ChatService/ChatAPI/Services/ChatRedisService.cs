using StackExchange.Redis;

namespace ChatService.ChatAPI.Services
{
    public class ChatRedisService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public ChatRedisService(IConfiguration config)
        {
            // 1. Lấy chuỗi kết nối
            var connectionString = config.GetConnectionString("Redis");

            // 2. Kết nối vào Redis
            _redis = ConnectionMultiplexer.Connect(connectionString!);
            _db = _redis.GetDatabase();
        }

        // Đánh dấu 1 người dùng đang Online (giữ trạng thái sống trong 5 phút)
        public async Task SetUserOnlineAsync(string userId)
        {
            var key = $"user:online:{userId}";
            // Set 1 giá trị rỗng true, tự xoá sau 5 phút nếu không gửi lại tín hiệu (ping)
            await _db.StringSetAsync(key, "true", TimeSpan.FromMinutes(5));
        }

        // Đánh dấu STAFF đang online và cập nhật/khởi tạo số phiên chat đang xử lý
        public async Task RegisterStaffOnlineAsync(string staffId)
        {
            var staffKey = "staff:online_queue";

            // Dùng SortedSet trong Redis để lưu Staff và Điểm (Score). Điểm = Số lượng cuộc trò chuyện đang xử lý
            // Chỉ thêm vào với Score = 0 nếu staff đó chưa tồn tại trong danh sách
            await _db.SortedSetAddAsync(staffKey, staffId, 0, CommandFlags.None);

            // Đồng thời giữ trạng thái online giống user thường
            var key = $"user:online:{staffId}";
            await _db.StringSetAsync(key, "true", TimeSpan.FromMinutes(5));
        }

        // Rút (Lấy) 1 Staff đang rảnh việc nhất ra khỏi danh sách, sau đó tăng điểm (Cộng thêm 1 phiên)
        public async Task<string?> AssignLeastBusyStaffAsync()
        {
            var staffKey = "staff:online_queue";

            // Lấy 1 bản ghi có Điểm thấp nhất từ Sorted Set (Bốc người ít việc nhất)
            var leastBusyStaffs = await _db.SortedSetRangeByRankAsync(staffKey, 0, 0);

            if (leastBusyStaffs != null && leastBusyStaffs.Length > 0)
            {
                var staffId = leastBusyStaffs[0].ToString();

                // Tăng score của ông này lên +1 (Cộng thêm 1 việc)
                await _db.SortedSetIncrementAsync(staffKey, staffId, 1);
                return staffId;
            }

            return null; // Không có Staff nào online
        }

        // Khi Staff chat xong hoặc kết thúc 1 phiên, trừ score đi
        public async Task DecreaseStaffWorkloadAsync(string staffId)
        {
             var staffKey = "staff:online_queue";
             var score = await _db.SortedSetScoreAsync(staffKey, staffId);

             if (score.HasValue && score.Value > 0)
             {
                 await _db.SortedSetDecrementAsync(staffKey, staffId, 1);
             }
        }

        // Kiểm tra xem User đó có đang Online không
        public async Task<bool> IsUserOnlineAsync(string userId)
        {
            var key = $"user:online:{userId}";
            return await _db.KeyExistsAsync(key);
        }
    }
}