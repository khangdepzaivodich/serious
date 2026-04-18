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

        // ACTIVE || CLOSED
        public string TrangThai { get; set; } = "ACTIVE";

        public DateTime LastTime { get; set; } = DateTime.UtcNow;

        public string LastMessage { get; set; } = string.Empty;

        public int UnreadCount { get; set; } = 0;
    }
}
