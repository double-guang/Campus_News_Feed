using Campus_News_Feed.Data;
using Campus_News_Feed.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Campus_News_Feed.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(AppDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<Category>> GetUserPreferencesAsync(int userId)
        {
            return await _context.UserPreferences
                .Where(up => up.UserId == userId)
                .Include(up => up.Category)
                .Select(up => up.Category!)
                .ToListAsync();
        }

        public async Task<bool> UpdateUserPreferencesAsync(int userId, IEnumerable<int> categoryIds)
        {
            try
            {
                // 获取用户
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return false;
                }

                // 删除现有偏好
                var existingPreferences = await _context.UserPreferences
                    .Where(up => up.UserId == userId)
                    .ToListAsync();

                _context.UserPreferences.RemoveRange(existingPreferences);

                // 添加新偏好
                foreach (var categoryId in categoryIds)
                {
                    var category = await _context.Categories.FindAsync(categoryId);
                    if (category != null)
                    {
                        _context.UserPreferences.Add(new UserPreference
                        {
                            UserId = userId,
                            CategoryId = categoryId
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"更新用户偏好时出错: {ex.Message}");
                return false;
            }
        }
    }
} 