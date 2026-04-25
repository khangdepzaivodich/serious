using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.ChatAPI.Models
{
    public class PhienTroChuyen
    {
        // UUID cho _id (maPhien) làm Primary Key
        [BsonId]
        [BsonRepresentation(BsonType.String)] // Lưu dưới dạng String trong Mongo để dễ đọc/truy vấn hơn, thao tác code C# vẫn là Guid
        public Guid Id { get; set; } = Guid.NewGuid();

        // Soft FK (Gọi từ Identity)
        [BsonRepresentation(BsonType.String)]
        public Guid UserID { get; set; }

        public DateTime ThoiGianTao { get; set; } = DateTime.UtcNow;

        // WAITING || ASSIGNED || CLOSED
        public string TrangThai { get; set; } = "WAITING";

        public string StaffID { get; set; } = string.Empty; // Staff hỗ trợ phiên chat này

        public string ClientType { get; set; } = "GUEST"; // GUEST hoặc USER

        public DateTime LastTime { get; set; } = DateTime.UtcNow;

        public string LastMessage { get; set; } = string.Empty;

        public string HoTen { get; set; } = string.Empty; // Tên khách hàng (rỗng nếu GUEST)
        public string Avatar { get; set; } = string.Empty; // Avatar khách hàng

        public string StaffHoTen { get; set; } = string.Empty; // Tên nhân viên hỗ trợ
        public string StaffAvatar { get; set; } = string.Empty; // Avatar nhân viên

        public int UnreadCount { get; set; } = 0;
    }
}
