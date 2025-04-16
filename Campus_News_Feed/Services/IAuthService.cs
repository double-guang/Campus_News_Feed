using Campus_News_Feed.Models.Domain;

namespace Campus_News_Feed.Services
{
    public interface IAuthService
    {
        Task<(bool success, string message)> RequestRegistrationAsync(string email);
        Task<(bool success, string message, User? user)> VerifyRegistrationAsync(string token);
        Task<(bool success, string message)> RequestLoginAsync(string email);
        Task<(bool success, string message, User? user)> VerifyLoginAsync(string token);
        Task<bool> IsEmailInSchoolDomainAsync(string email);
    }
} 