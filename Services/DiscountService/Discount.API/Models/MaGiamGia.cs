using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DiscountService.Discount.API.Models
{
    public class MaGiamGia
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid MaGG { get; set; } = Guid.NewGuid();

        [BsonElement("maCode")]
        public string MaCode { get; set; } = string.Empty;

        [BsonElement("loai")]
        public string Loai { get; set; } = string.Empty; // e.g. "PhanTram", "Tien"

        [BsonElement("soTien")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal SoTien { get; set; } // is % or money

        [BsonElement("giaTriGiamToiDa")]
        [BsonRepresentation(BsonType.Decimal128)]
        [BsonIgnoreIfNull]
        public decimal? GiaTriGiamToiDa { get; set; } // null if not % or no max

        [BsonElement("soLuong")]
        public int SoLuong { get; set; }

        [BsonElement("hanSuDung")]
        public DateTime HanSuDung { get; set; }
    }
}
