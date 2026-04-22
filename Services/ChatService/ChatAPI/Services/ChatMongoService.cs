using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ChatService.ChatAPI.Models;

namespace ChatService.ChatAPI.Services
{
    public class ChatMongoService
    {
        private readonly IMongoCollection<PhienTroChuyen> _phienCollection;
        private readonly IMongoCollection<HoiThoai> _hoiThoaiCollection;

        public ChatMongoService(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("MongoDb");
            var mongoClient = new MongoClient(connectionString);
            var mongoDatabase = mongoClient.GetDatabase("ChatStoreDB");

            // Ánh xạ 2 bảng theo models
            _phienCollection = mongoDatabase.GetCollection<PhienTroChuyen>("PhienTroChuyen");
            _hoiThoaiCollection = mongoDatabase.GetCollection<HoiThoai>("HoiThoai");
        }

        // --- PHIÊN TRÒ CHUYỆN ---

        // Lấy danh sách các phiên chat
        public async Task<List<PhienTroChuyen>> GetDanhSachPhienAsync() =>
            await _phienCollection.Find(_ => true).ToListAsync();

        // Tạo 1 phiên chat mới
        public async Task TaoPhienAsync(PhienTroChuyen phien) =>
            await _phienCollection.InsertOneAsync(phien);

        // Cập nhật thông tin LastMessage, LastTime, UnreadCount... cho Phiên
        public async Task CapNhatThongTinPhienAsync(Guid idPhien, string lastMessage)
        {
            var update = Builders<PhienTroChuyen>.Update
                .Set(p => p.LastMessage, lastMessage)
                .Set(p => p.LastTime, DateTime.UtcNow)
                .Inc(p => p.UnreadCount, 1); // Tăng đếm chưa đọc +1

            await _phienCollection.UpdateOneAsync(p => p.Id == idPhien, update);
        }

        // Reset unread count khi Staff đã đọc
        public async Task ResetUnreadAsync(Guid idPhien)
        {
            var update = Builders<PhienTroChuyen>.Update.Set(p => p.UnreadCount, 0);
            await _phienCollection.UpdateOneAsync(p => p.Id == idPhien, update);
        }

        // Cập nhật trạng thái phiên (ACTIVE / CLOSED)
        public async Task CapNhatTrangThaiPhienAsync(Guid idPhien, string trangThai)
        {
            var update = Builders<PhienTroChuyen>.Update
                .Set(p => p.TrangThai, trangThai)
                .Set(p => p.LastTime, DateTime.UtcNow);

            await _phienCollection.UpdateOneAsync(p => p.Id == idPhien, update);
        }

        // Nâng cấp phiên từ GUEST → USER (khi khách login)
        public async Task UpgradePhienAsync(Guid idPhien, Guid userId, string hoTen)
        {
            var update = Builders<PhienTroChuyen>.Update
                .Set(p => p.ClientType, "USER")
                .Set(p => p.UserID, userId)
                .Set(p => p.HoTen, hoTen)
                .Set(p => p.LastTime, DateTime.UtcNow);

            await _phienCollection.UpdateOneAsync(p => p.Id == idPhien, update);
        }

        // Lấy 1 phiên theo ID
        public async Task<PhienTroChuyen?> GetPhienByIdAsync(Guid idPhien)
        {
            return await _phienCollection.Find(p => p.Id == idPhien).FirstOrDefaultAsync();
        }


        // --- HỘI THOẠI (TIN NHẮN) ---

        // Lấy danh sách tin nhắn của 1 phiên cụ thể
        public async Task<List<HoiThoai>> GetTinNhanTheoPhienAsync(Guid idPhien) =>
            await _hoiThoaiCollection.Find(h => h.MaPhien == idPhien).ToListAsync();

        // Gửi 1 tin nhắn mới
        public async Task GuiTinNhanAsync(HoiThoai tinNhan)
        {
            await _hoiThoaiCollection.InsertOneAsync(tinNhan);

            // Tự động đẩy update phiên chat
            await CapNhatThongTinPhienAsync(tinNhan.MaPhien, tinNhan.NoiDung);
        }
    }
}