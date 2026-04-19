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

        // Kiểm tra xem User đó có đang Online không
        public async Task<bool> IsUserOnlineAsync(string userId)
        {
            var key = $"user:online:{userId}";
            return await _db.KeyExistsAsync(key);
        }
    }
}