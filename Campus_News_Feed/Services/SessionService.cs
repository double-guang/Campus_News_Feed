using System.Text.Json;
using Campus_News_Feed.Data;
using Campus_News_Feed.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Campus_News_Feed.Services
{
    public class SessionService : ISessionService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<SessionService> _logger;

        public SessionService(
            AppDbContext dbContext,
            ILogger<SessionService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        // 会话管理
        public Task CreateSessionAsync(HttpContext httpContext, User user)
        {
            try
            {
                httpContext.Session.SetString("UserId", user.Id.ToString());
                httpContext.Session.SetString("Email", user.Email);
                
                if (user.IsAdmin)
                {
                    httpContext.Session.SetString("IsAdmin", "true");
                }
                
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建会话时出错: {UserId}", user.Id);
                throw;
            }
        }

        public Task DestroySessionAsync(HttpContext httpContext)
        {
            try
            {
                httpContext.Session.Clear();
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "销毁会话时出错");
                throw;
            }
        }

        // 获取当前用户信息
        public async Task<User?> GetCurrentUserAsync(HttpContext httpContext)
        {
            try
            {
                var userIdStr = httpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return null;
                }

                var user = await _dbContext.Users.FindAsync(userId);
                
                // 如果用户被注销，则清除会话并返回null
                if (user != null && !user.IsActive)
                {
                    await DestroySessionAsync(httpContext);
                    return null;
                }
                
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取当前用户时出错");
                return null;
            }
        }

        public Task<int> GetCurrentUserIdAsync(HttpContext httpContext)
        {
            try
            {
                var userIdStr = httpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return Task.FromResult(0);
                }

                return Task.FromResult(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取当前用户ID时出错");
                return Task.FromResult(0);
            }
        }

        public Task<bool> IsAuthenticatedAsync(HttpContext httpContext)
        {
            try
            {
                var userIdStr = httpContext.Session.GetString("UserId");
                return Task.FromResult(!string.IsNullOrEmpty(userIdStr));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查用户是否已认证时出错");
                return Task.FromResult(false);
            }
        }

        public Task<bool> IsAdminAsync(HttpContext httpContext)
        {
            try
            {
                var isAdmin = httpContext.Session.GetString("IsAdmin");
                return Task.FromResult(isAdmin == "true");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查用户是否为管理员时出错");
                return Task.FromResult(false);
            }
        }
    }
} 