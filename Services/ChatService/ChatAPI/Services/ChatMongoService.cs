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
            var connectionString = config["Mongo:ConnectionString"] 
                                 ?? config.GetConnectionString("MongoDb");
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
                .Inc(p => p.UnreadCount, 1);

            await _phienCollection.UpdateOneAsync(p => p.Id == idPhien, update);
        }

        // Reset unread count khi Staff đã đọc
        public async Task ResetUnreadAsync(Guid idPhien)
        {
            var update = Builders<PhienTroChuyen>.Update.Set(p => p.UnreadCount, 0);
            await _phienCollection.UpdateOneAsync(p => p.Id == idPhien, update);
        }

        // Cập nhật trạng thái phiên (ACTIVE / CLOSED)
        public async Task<bool> CapNhatTrangThaiPhienAsync(Guid idPhien, string trangThaiMoi, string? trangThaiCu = null)
        {
            var filter = Builders<PhienTroChuyen>.Filter.Eq(p => p.Id, idPhien);
            if (!string.IsNullOrEmpty(trangThaiCu))
            {
                filter &= Builders<PhienTroChuyen>.Filter.Eq(p => p.TrangThai, trangThaiCu);
            }

            var update = Builders<PhienTroChuyen>.Update
                .Set(p => p.TrangThai, trangThaiMoi)
                .Set(p => p.LastTime, DateTime.UtcNow);

            var result = await _phienCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        // Nâng cấp phiên từ GUEST → USER (khi khách login)
        public async Task UpgradePhienAsync(Guid idPhien, Guid userId, string hoTen, string? avatar = null)
        {
            var update = Builders<PhienTroChuyen>.Update
                .Set(p => p.ClientType, "USER")
                .Set(p => p.UserID, userId)
                .Set(p => p.HoTen, hoTen)
                .Set(p => p.LastTime, DateTime.UtcNow);

            if (!string.IsNullOrEmpty(avatar))
            {
                update = update.Set(p => p.Avatar, avatar);
            }

            await _phienCollection.UpdateOneAsync(p => p.Id == idPhien, update);
        }

        // Cập nhật thông tin nhân viên đảm nhận (ID và Tên)
        public async Task CapNhatThongTinStaffPhienAsync(Guid idPhien, string staffId, string staffName, string? staffAvatar = null)
        {
            var update = Builders<PhienTroChuyen>.Update
                .Set(p => p.StaffID, staffId)
                .Set(p => p.StaffHoTen, staffName);

            if (!string.IsNullOrEmpty(staffAvatar))
            {
                update = update.Set(p => p.StaffAvatar, staffAvatar);
            }

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
            await CapNhatThongTinPhienAsync(tinNhan.MaPhien, tinNhan.NoiDung);
        }

        // --- LẤY DANH SÁCH PHIÊN IDLE (Dùng cho Worker) ---
        public async Task<List<PhienTroChuyen>> GetIdleSessionsAsync(TimeSpan threshold)
        {
            var cutoff = DateTime.UtcNow - threshold;
            var filter = Builders<PhienTroChuyen>.Filter.And(
                Builders<PhienTroChuyen>.Filter.Eq(p => p.TrangThai, "ACTIVE"),
                Builders<PhienTroChuyen>.Filter.Lt(p => p.LastTime, cutoff)
            );
            return await _phienCollection.Find(filter).ToListAsync();
        }
    }
}