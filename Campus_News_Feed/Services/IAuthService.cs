using Campus_News_Feed.Models.Domain;

namespace Campus_News_Feed.Services
{
    public interface IAuthService
    {
        // 邮箱验证
        Task<bool> ValidateEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        
        // 注册相关
        Task<string> CreateRegistrationTokenAsync(string email);
        Task<bool> VerifyRegistrationTokenAsync(string token);
        
        // 登录相关
        Task<string> CreateLoginTokenAsync(string email);
        Task<int> VerifyLoginTokenAsync(string token);
        
        // 管理员密码登录
        Task<bool> SetAdminPasswordAsync(int userId, string password);
        Task<User?> ValidateAdminCredentialsAsync(string username, string password);
        
        // 用户管理
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> UpdateUserAsync(User user);
    }
} 