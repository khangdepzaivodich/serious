namespace OrderingService
{
    public class JwtSettings
    {
        public string? RsaPublicKey { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
    }
}
