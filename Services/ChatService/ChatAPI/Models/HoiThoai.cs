using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatService.ChatAPI.Models
{
    public class HoiThoai
    {
        // ObjectID cho _id (maTN) làm Primary Key
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)] // Để Mongo tự quản lý ObjectId dưới dạng chuỗi
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        // Foreign Key móc tới PhienTroChuyen (maPhien)
        [BsonRepresentation(BsonType.String)]
        public Guid MaPhien { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid SenderID { get; set; }

        // user || guest || staff
        public string SenderType { get; set; } = "user";

        public string SenderName { get; set; } = string.Empty;

        public string SenderAvatar { get; set; } = string.Empty;

        public string NoiDung { get; set; } = string.Empty;

        public DateTime ThoiGianGui { get; set; } = DateTime.UtcNow;

        // sent || seen
        public string TrangThai { get; set; } = "sent";

        // Chống duplicate realtime
        [BsonRepresentation(BsonType.String)]
        public Guid ClientID { get; set; } = Guid.NewGuid();

        // True: Chỉ Staff thấy với nhau (Zendesk internal note), False: Khách thấy được
        public bool IsInternalNote { get; set; } = false;
    }
}
