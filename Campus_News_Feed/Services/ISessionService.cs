using Campus_News_Feed.Models.Domain;

namespace Campus_News_Feed.Services
{
    public interface ISessionService
    {
        Task CreateUserSessionAsync(HttpContext httpContext, User user);
        Task<User?> GetCurrentUserAsync(HttpContext httpContext);
        Task<bool> IsAdminAsync(HttpContext httpContext);
        Task ClearUserSessionAsync(HttpContext httpContext);
    }
} 