using StackExchange.Redis;

namespace ChatService.ChatAPI.Services
{
    public class ChatRedisService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public ChatRedisService(IConfiguration config)
        {
            // 1. Lấy chuỗi kết nối: Ưu tiên Redis:ConnectionString từ Docker
            var connectionString = config["Redis:ConnectionString"] 
                                 ?? config.GetConnectionString("Redis") 
                                 ?? "redis:6379,abortConnect=false";

            // 2. Kết nối vào Redis
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _db = _redis.GetDatabase();
        }

        // Đánh dấu 1 người dùng đang Online (giữ trạng thái sống trong 5 phút)
        public async Task SetUserOnlineAsync(string userId)
        {
            var key = $"user:online:{userId}";
            await _db.StringSetAsync(key, "true", TimeSpan.FromMinutes(5));
        }

        // Đánh dấu STAFF đang online và cập nhật/khởi tạo số phiên chat đang xử lý
        public async Task RegisterStaffOnlineAsync(string staffId)
        {
            var staffKey = "staff:online_queue";
            await _db.SortedSetAddAsync(staffKey, staffId, 0, CommandFlags.None);
            
            var key = $"user:online:{staffId}";
            await _db.StringSetAsync(key, "true", TimeSpan.FromMinutes(5));

            // Lưu thời điểm cuối cùng staff xuất hiện (Dùng để check reassign)
            var lastSeenKey = $"staff:last_seen:{staffId}";
            await _db.StringSetAsync(lastSeenKey, DateTime.UtcNow.ToString("O"));
        }

        public async Task<DateTime?> GetStaffLastSeenAsync(string staffId)
        {
            var lastSeenKey = $"staff:last_seen:{staffId}";
            var val = await _db.StringGetAsync(lastSeenKey);
            if (val.HasValue && DateTime.TryParse(val, out var lastSeen))
            {
                return lastSeen;
            }
            return null;
        }

        // Rút (Lấy) 1 Staff đang rảnh việc nhất ra khỏi danh sách, sau đó tăng điểm (Cộng thêm 1 phiên)
        // Dùng Lua Script để đảm bảo tính Atomic (Nguyên tử), tránh Race Condition khi nhiều khách vào cùng lúc.
        // HÀNG ĐỢI: Chỉ bốc staff nếu workload < MAX_CAPACITY (5)
        public async Task<string?> AssignLeastBusyStaffAsync()
        {
            var staffKey = "staff:online_queue";
            int maxCapacity = 5;

            // Script Lua: Lấy danh sách staff, duyệt từ người rảnh nhất
            var script = @"
                local all_staff = redis.call('ZRANGE', KEYS[1], 0, -1, 'WITHSCORES')
                for i = 1, #all_staff, 2 do
                    local staffId = all_staff[i]
                    local score = tonumber(all_staff[i+1])
                    
                    -- Kiểm tra xem staff này có thực sự đang Online không
                    if redis.call('EXISTS', 'user:online:' .. staffId) == 1 then
                        if score < tonumber(ARGV[1]) then
                            redis.call('ZINCRBY', KEYS[1], 1, staffId)
                            return staffId
                        end
                    end
                end
                return nil";

            var result = await _db.ScriptEvaluateAsync(script, new RedisKey[] { staffKey }, new RedisValue[] { maxCapacity });
            
            return result.IsNull ? null : result.ToString();
        }

        // --- HÀNG ĐỢI CHỜ (WAITING QUEUE) ---
        public async Task AddToWaitingQueueAsync(string sessionId)
        {
            var queueKey = "chat:waiting_queue";
            await _db.ListLeftPushAsync(queueKey, sessionId);
        }

        public async Task<string?> GetNextInWaitingQueueAsync()
        {
            var queueKey = "chat:waiting_queue";
            var sessionId = await _db.ListRightPopAsync(queueKey);
            return sessionId.IsNull ? null : sessionId.ToString();
        }

        // Khi Staff chat xong hoặc kết thúc 1 phiên, trừ score đi (Atomic)
        public async Task DecreaseStaffWorkloadAsync(string staffId)
        {
             var staffKey = "staff:online_queue";
             
             var script = @"
                local score = redis.call('ZSCORE', KEYS[1], ARGV[1])
                if score and tonumber(score) > 0 then
                    return redis.call('ZINCRBY', KEYS[1], -1, ARGV[1])
                end
                return score";

             await _db.ScriptEvaluateAsync(script, new RedisKey[] { staffKey }, new RedisValue[] { staffId });
        }

        // Tăng workload (Atomic)
        public async Task IncreaseStaffWorkloadAsync(string staffId)
        {
             var staffKey = "staff:online_queue";
             await _db.SortedSetAddAsync(staffKey, staffId, 0, When.NotExists);
             await _db.SortedSetIncrementAsync(staffKey, staffId, 1);
        }

        // Kiểm tra xem User đó có đang Online không
        public async Task<bool> IsUserOnlineAsync(string userId)
        {
            var key = $"user:online:{userId}";
            return await _db.KeyExistsAsync(key);
        }

        // --- MAPPING GUEST -> USER
        public async Task MapSessionToUserAsync(string sessionId, string userId, string hoTen)
        {
            var key = $"chat:map:{sessionId}";
            var entries = new HashEntry[]
            {
                new HashEntry("userId", userId),
                new HashEntry("hoTen", hoTen)
            };
            await _db.HashSetAsync(key, entries);
            await _db.KeyExpireAsync(key, TimeSpan.FromDays(1)); // Mapping sống 1 ngày
        }

        public async Task<(string? userId, string? hoTen)> GetSessionMappingAsync(string sessionId)
        {
            var key = $"chat:map:{sessionId}";
            var userId = await _db.HashGetAsync(key, "userId");
            var hoTen = await _db.HashGetAsync(key, "hoTen");
            return (userId.HasValue ? userId.ToString() : null, hoTen.HasValue ? hoTen.ToString() : null);
        }

        // --- TÊN NHÂN VIÊN ---
        public async Task SetStaffNameAsync(string staffId, string staffName)
        {
            var key = $"staff:name:{staffId}";
            await _db.StringSetAsync(key, staffName, TimeSpan.FromHours(12));
        }

        public async Task<string?> GetStaffNameAsync(string staffId)
        {
            var key = $"staff:name:{staffId}";
            var name = await _db.StringGetAsync(key);
            return name.HasValue ? name.ToString() : null;
        }

        public async Task SetStaffAvatarAsync(string staffId, string avatar)
        {
            var key = $"staff:avatar:{staffId}";
            await _db.StringSetAsync(key, avatar, TimeSpan.FromHours(12));
        }

        public async Task<string?> GetStaffAvatarAsync(string staffId)
        {
            var key = $"staff:avatar:{staffId}";
            var avatar = await _db.StringGetAsync(key);
            return avatar.HasValue ? avatar.ToString() : null;
        }

        public async Task<long> GetNextGuestNumberWithDateAsync(string dateKey)
        {
            var key = $"chat:guest_counter:{dateKey}";
            var count = await _db.StringIncrementAsync(key);
            if (count == 1)
            {
                await _db.KeyExpireAsync(key, TimeSpan.FromDays(2)); // Tự xóa sau 2 ngày
            }
            return count;
        }
    }
}