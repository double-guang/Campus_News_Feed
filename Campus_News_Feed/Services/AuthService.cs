using Campus_News_Feed.Data;
using Campus_News_Feed.Models.Domain;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Cryptography;

namespace Campus_News_Feed.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            AppDbContext dbContext,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;
        }

        // 邮箱验证
        public async Task<bool> ValidateEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || !email.Contains('@'))
                {
                    _logger.LogWarning($"邮箱格式无效: {email}");
                    return false;
                }

                // 获取允许的邮箱域名列表
                var allowedDomains = _configuration.GetSection("AppSettings:AllowedEmailDomains").Get<string[]>();
                if (allowedDomains == null || allowedDomains.Length == 0)
                {
                    // 如果未配置，默认仅允许吉林大学邮箱
                    allowedDomains = new[] { "mails.jlu.edu.cn", "jlu.edu.cn" };
                    _logger.LogInformation($"未配置邮箱域名限制，使用默认限制（仅吉林大学邮箱）: {email}");
                }

                // 检查邮箱域名是否在允许列表中
                var domain = email.Split('@')[1].ToLower();
                var isAllowed = allowedDomains.Any(d => domain.Equals(d.ToLower()));
                
                if (!isAllowed)
                {
                    _logger.LogWarning($"邮箱域名不在允许列表中: {email}, 域名: {domain}，仅允许吉林大学邮箱");
                    return false;
                }
                
                _logger.LogInformation($"邮箱验证通过: {email}，符合吉林大学邮箱要求");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证邮箱时出错: {Email}", email);
                return false;
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            try
            {
                return await _dbContext.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查邮箱是否存在时出错: {Email}", email);
                throw;
            }
        }

        // 注册相关
        public async Task<string> CreateRegistrationTokenAsync(string email)
        {
            try
            {
                // 生成随机令牌
                var token = GenerateRandomToken();
                
                // 保存到数据库
                var tokenEntity = new Token
                {
                    Value = token,
                    Type = TokenType.Registration,
                    UserId = null, // 注册时还没有用户ID
                    Email = email, // 使用Email字段存储邮箱
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30), // 30分钟有效期
                    IsUsed = false
                };
                
                _dbContext.Tokens.Add(tokenEntity);
                await _dbContext.SaveChangesAsync();
                
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建注册令牌时出错: {Email}", email);
                throw;
            }
        }

        public async Task<bool> VerifyRegistrationTokenAsync(string token)
        {
            try
            {
                _logger.LogInformation($"开始查找注册令牌: {token}");
                
                // 查找令牌
                var tokenEntity = await _dbContext.Tokens.FirstOrDefaultAsync(t => 
                    t.Value == token && 
                    t.Type == TokenType.Registration && 
                    !t.IsUsed && 
                    t.ExpiresAt > DateTime.UtcNow);
                
                if (tokenEntity == null)
                {
                    _logger.LogWarning($"未找到有效的注册令牌: {token}");
                    return false;
                }
                
                _logger.LogInformation($"找到有效的注册令牌: {token}, 邮箱: {tokenEntity.Email}");
                
                // 标记令牌为已使用
                tokenEntity.IsUsed = true;
                
                // 创建用户
                var user = new User
                {
                    Email = tokenEntity.Email,
                    RegisteredAt = DateTime.UtcNow,
                    IsAdmin = false
                };
                
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
                
                _logger.LogInformation($"成功创建用户: {tokenEntity.Email}, 用户ID: {user.Id}");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证注册令牌时出错: {Token}", token);
                throw;
            }
        }

        // 登录相关
        public async Task<string> CreateLoginTokenAsync(string email)
        {
            try
            {
                // 查找用户
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                if (user == null)
                {
                    throw new Exception($"用户不存在: {email}");
                }
                
                // 生成随机令牌
                var token = GenerateRandomToken();
                
                // 保存到数据库
                var tokenEntity = new Token
                {
                    Value = token,
                    Type = TokenType.Login,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30), // 30分钟有效期
                    IsUsed = false
                };
                
                _dbContext.Tokens.Add(tokenEntity);
                await _dbContext.SaveChangesAsync();
                
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建登录令牌时出错: {Email}", email);
                throw;
            }
        }

        public async Task<int> VerifyLoginTokenAsync(string token)
        {
            try
            {
                // 查找令牌
                var tokenEntity = await _dbContext.Tokens.FirstOrDefaultAsync(t => 
                    t.Value == token && 
                    t.Type == TokenType.Login && 
                    !t.IsUsed && 
                    t.ExpiresAt > DateTime.UtcNow);
                
                if (tokenEntity == null || tokenEntity.UserId == null)
                {
                    return 0;
                }
                
                // 检查用户是否被注销
                var user = await _dbContext.Users.FindAsync(tokenEntity.UserId);
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning($"尝试登录已注销的账号: {tokenEntity.UserId}");
                    return 0;
                }
                
                // 标记令牌为已使用
                tokenEntity.IsUsed = true;
                await _dbContext.SaveChangesAsync();
                
                return tokenEntity.UserId.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证登录令牌时出错: {Token}", token);
                throw;
            }
        }

        // 用户管理
        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                return await _dbContext.Users.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据ID获取用户时出错: {Id}", id);
                throw;
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据邮箱获取用户时出错: {Email}", email);
                throw;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户时出错: {UserId}", user.Id);
                throw;
            }
        }
        
        // 辅助方法
        private string GenerateRandomToken()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var token = new StringBuilder(32);
            
            for (int i = 0; i < 32; i++)
            {
                token.Append(chars[random.Next(chars.Length)]);
            }
            
            return token.ToString();
        }

        // 管理员密码登录
        public async Task<bool> SetAdminPasswordAsync(int userId, string password)
        {
            try
            {
                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null || !user.IsAdmin)
                {
                    _logger.LogWarning($"尝试为非管理员用户设置密码: {userId}");
                    return false;
                }

                // 生成密码哈希
                user.PasswordHash = HashPassword(password);
                user.Username = user.Email.Split('@')[0]; // 默认使用邮箱用户名部分
                user.UsePasswordLogin = true;

                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();
                
                _logger.LogInformation($"已为管理员设置密码: {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置管理员密码时出错: {UserId}", userId);
                return false;
            }
        }

        public async Task<User?> ValidateAdminCredentialsAsync(string username, string password)
        {
            try
            {
                // 先尝试通过用户名查找
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Username == username && u.IsAdmin && u.UsePasswordLogin && u.IsActive);

                // 如果找不到，尝试通过邮箱查找
                if (user == null)
                {
                    user = await _dbContext.Users
                        .FirstOrDefaultAsync(u => u.Email == username && u.IsAdmin && u.UsePasswordLogin && u.IsActive);
                }

                if (user != null && VerifyPassword(password, user.PasswordHash))
                {
                    _logger.LogInformation($"管理员登录成功: {user.Id}");
                    
                    // 更新最后登录时间
                    user.LastLoginAt = DateTime.UtcNow;
                    _dbContext.Users.Update(user);
                    await _dbContext.SaveChangesAsync();
                    
                    return user;
                }

                _logger.LogWarning($"管理员登录失败，用户名或密码错误: {username}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证管理员凭据时出错: {Username}", username);
                return null;
            }
        }

        // 密码哈希辅助方法
        private string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            return Convert.ToBase64String(hashBytes);
        }

        private bool VerifyPassword(string password, string? storedHash)
        {
            if (string.IsNullOrEmpty(storedHash))
            {
                return false;
            }

            byte[] hashBytes = Convert.FromBase64String(storedHash);
            
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);
            
            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    return false;
                }
            }
            
            return true;
        }
    }
} 