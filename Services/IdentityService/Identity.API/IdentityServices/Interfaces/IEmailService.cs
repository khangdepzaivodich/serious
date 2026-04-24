namespace IdentityService.Identity.API.IdentityServices.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
    }
}
