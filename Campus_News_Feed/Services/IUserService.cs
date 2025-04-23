using Campus_News_Feed.Models.Domain;
using Campus_News_Feed.Models.ViewModels;

namespace Campus_News_Feed.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<UserPreference>> GetUserPreferencesAsync(int userId);
        Task<bool> UpdateUserPreferencesAsync(int userId, List<int> categoryIds);
        Task<bool> DeactivateUserAsync(int userId);
        Task<bool> SetUserPreferencesAsync(int userId, IEnumerable<int> categoryIds);
        Task<IEnumerable<Category>> GetUserCategoriesAsync(int userId);
        
        /// <summary>
        /// 获取用户总数
        /// </summary>
        Task<int> GetUserCountAsync();
        
        /// <summary>
        /// 获取分页用户列表
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页显示数量</param>
        /// <returns>分页用户列表</returns>
        Task<PaginatedList<User>> GetPaginatedUsersAsync(int pageIndex, int pageSize);
        
        /// <summary>
        /// 获取分页用户列表（带搜索和排序）
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页显示数量</param>
        /// <param name="searchTerm">搜索关键词</param>
        /// <param name="sortBy">排序字段</param>
        /// <param name="isActive">是否筛选活跃用户</param>
        /// <returns>分页用户列表</returns>
        Task<PaginatedList<User>> GetPaginatedUsersAsync(int pageIndex, int pageSize, string? searchTerm, string sortBy, bool? isActive);
    }
} 