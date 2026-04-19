namespace ChatService.ChatAPI.DTOs
{
    public class SendMessageRequest
    {
        public Guid MaPhien { get; set; }
        public Guid SenderID { get; set; }
        public string SenderType { get; set; } = "user";
        public string NoiDung { get; set; } = string.Empty;
        public Guid? ClientID { get; set; }
    }
}