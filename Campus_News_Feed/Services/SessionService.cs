using System.Text.Json;
using Campus_News_Feed.Data;
using Campus_News_Feed.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Campus_News_Feed.Services
{
    public class SessionService : ISessionService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SessionService> _logger;
        private const string UserSessionKey = "CurrentUser";

        public SessionService(AppDbContext context, ILogger<SessionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CreateUserSessionAsync(HttpContext httpContext, User user)
        {
            try
            {
                // 保存用户ID到会话中
                httpContext.Session.SetInt32(UserSessionKey, user.Id);
                await httpContext.Session.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"创建用户会话失败: {ex.Message}");
                throw;
            }
        }

        public async Task<User?> GetCurrentUserAsync(HttpContext httpContext)
        {
            try
            {
                // 从会话中获取用户ID
                var userId = httpContext.Session.GetInt32(UserSessionKey);
                if (userId == null)
                {
                    return null;
                }

                // 获取用户信息
                return await _context.Users.FindAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"获取当前用户信息失败: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> IsAdminAsync(HttpContext httpContext)
        {
            var user = await GetCurrentUserAsync(httpContext);
            return user?.IsAdmin ?? false;
        }

        public async Task ClearUserSessionAsync(HttpContext httpContext)
        {
            try
            {
                httpContext.Session.Remove(UserSessionKey);
                await httpContext.Session.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"清除用户会话失败: {ex.Message}");
                throw;
            }
        }
    }
} 