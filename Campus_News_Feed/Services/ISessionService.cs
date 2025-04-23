using Campus_News_Feed.Models.Domain;

namespace Campus_News_Feed.Services
{
    public interface ISessionService
    {
        // 会话管理
        Task CreateSessionAsync(HttpContext httpContext, User user);
        Task DestroySessionAsync(HttpContext httpContext);
        
        // 获取当前用户信息
        Task<User?> GetCurrentUserAsync(HttpContext httpContext);
        Task<int> GetCurrentUserIdAsync(HttpContext httpContext);
        Task<bool> IsAuthenticatedAsync(HttpContext httpContext);
        Task<bool> IsAdminAsync(HttpContext httpContext);
    }
} 