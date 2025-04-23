using Campus_News_Feed.Data;
using Campus_News_Feed.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Campus_News_Feed.Models.ViewModels;

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

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.OrderByDescending(u => u.RegisteredAt).ToListAsync();
        }

        public async Task<IEnumerable<UserPreference>> GetUserPreferencesAsync(int userId)
        {
            return await _context.UserPreferences
                .Where(up => up.UserId == userId)
                .Include(up => up.Category)
                .ToListAsync();
        }

        public async Task<bool> UpdateUserPreferencesAsync(int userId, List<int> categoryIds)
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

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return false;
                }

                // 不允许注销管理员
                if (user.IsAdmin)
                {
                    return false;
                }

                // 设置用户为非活跃状态
                user.IsActive = false;
                
                // 清除用户偏好
                var userPreferences = await _context.UserPreferences
                    .Where(up => up.UserId == userId)
                    .ToListAsync();
                
                _context.UserPreferences.RemoveRange(userPreferences);
                
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"用户 {userId} 已被注销");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"注销用户时出错: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetUserPreferencesAsync(int userId, IEnumerable<int> categoryIds)
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
                _logger.LogError($"设置用户偏好时出错: {ex.Message}");
                return false;
            }
        }
        
        public async Task<IEnumerable<Category>> GetUserCategoriesAsync(int userId)
        {
            var userPreferences = await _context.UserPreferences
                .Where(up => up.UserId == userId)
                .Include(up => up.Category)
                .ToListAsync();
                
            // 使用LINQ过滤掉null值并强制转换为非空类型
            return userPreferences
                .Where(up => up.Category != null)
                .Select(up => up.Category!)
                .ToList();
        }

        public async Task<int> GetUserCountAsync()
        {
            return await _context.Users.Where(u => !u.IsAdmin).CountAsync();
        }
        
        public async Task<PaginatedList<User>> GetPaginatedUsersAsync(int pageIndex, int pageSize)
        {
            try
            {
                var query = _context.Users
                    .OrderByDescending(u => u.RegisteredAt)
                    .AsQueryable();
                
                return await PaginatedList<User>.CreateAsync(query, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分页用户列表时出错");
                return new PaginatedList<User>(new List<User>(), 0, pageIndex, pageSize);
            }
        }
        
        public async Task<PaginatedList<User>> GetPaginatedUsersAsync(int pageIndex, int pageSize, string? searchTerm, string sortBy, bool? isActive)
        {
            try
            {
                var query = _context.Users.AsQueryable();
                
                // 应用搜索条件
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(u => 
                        u.Email.ToLower().Contains(searchTerm) || 
                        (u.Username != null && u.Username.ToLower().Contains(searchTerm)));
                }
                
                // 应用活跃状态筛选
                if (isActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == isActive.Value);
                }
                
                // 应用排序
                switch (sortBy)
                {
                    case "email":
                        query = query.OrderBy(u => u.Email);
                        break;
                    case "emailDesc":
                        query = query.OrderByDescending(u => u.Email);
                        break;
                    case "username":
                        query = query.OrderBy(u => u.Username);
                        break;
                    case "usernameDesc":
                        query = query.OrderByDescending(u => u.Username);
                        break;
                    case "lastLogin":
                        query = query.OrderByDescending(u => u.LastLoginAt);
                        break;
                    case "lastLoginAsc":
                        query = query.OrderBy(u => u.LastLoginAt);
                        break;
                    case "registerDateAsc":
                        query = query.OrderBy(u => u.RegisteredAt);
                        break;
                    default: // registerDate (默认)
                        query = query.OrderByDescending(u => u.RegisteredAt);
                        break;
                }
                
                return await PaginatedList<User>.CreateAsync(query, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分页用户列表(含搜索排序)时出错: {SearchTerm}, {SortBy}", searchTerm, sortBy);
                return new PaginatedList<User>(new List<User>(), 0, pageIndex, pageSize);
            }
        }
    }
} 