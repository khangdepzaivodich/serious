using System;
using System.Collections.Generic;
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

        [BsonElement("donHangToiThieu")]
        [BsonRepresentation(BsonType.Decimal128)]
        [BsonIgnoreIfNull]
        public decimal? DonHangToiThieu { get; set; }

        [BsonElement("giaTriGiamToiDa")]
        [BsonRepresentation(BsonType.Decimal128)]
        [BsonIgnoreIfNull]
        public decimal? GiaTriGiamToiDa { get; set; } // null if not % or no max

        [BsonElement("soLuong")]
        public int SoLuong { get; set; }

        [BsonElement("hanSuDung")]
        public DateTime HanSuDung { get; set; }

        [BsonElement("apDungCho")]
        public string ApDungCho { get; set; } = "TatCa";

        [BsonElement("maLDM")]
        [BsonIgnoreIfNull]
        [BsonRepresentation(BsonType.String)]
        public Guid? MaLDM { get; set; }

        [BsonElement("maDM")]
        [BsonIgnoreIfNull]
        [BsonRepresentation(BsonType.String)]
        public Guid? MaDM { get; set; }

        [BsonElement("maSP")]
        [BsonIgnoreIfNull]
        [BsonRepresentation(BsonType.String)]
        public Guid? MaSP { get; set; }

        [BsonElement("maSPs")]
        [BsonIgnoreIfNull]
        public List<Guid> MaSPs { get; set; } = new();
    }
}
