using Campus_News_Feed.Models.Domain;

namespace Campus_News_Feed.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<Category>> GetUserPreferencesAsync(int userId);
        Task<bool> UpdateUserPreferencesAsync(int userId, IEnumerable<int> categoryIds);
    }
} 