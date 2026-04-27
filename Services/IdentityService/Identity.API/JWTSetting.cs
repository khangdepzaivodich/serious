namespace IdentityService.Identity.API
{
    public class JwtSettings
    {
        public string? RsaPrivateKey { get; set; } // Khóa để ký (chỉ IdentityService giữ)
        public string? RsaPublicKey { get; set; }  // Khóa để xác thực (chia sẻ cho các service khác)
        public int ExpiryMinutes { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
    }
}