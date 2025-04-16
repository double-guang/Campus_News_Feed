namespace Campus_News_Feed.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendRegistrationTokenAsync(string email, string token);
        Task SendLoginTokenAsync(string email, string token);
    }
} 