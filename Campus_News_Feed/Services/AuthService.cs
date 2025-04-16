using Campus_News_Feed.Data;
using Campus_News_Feed.Models.Domain;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Campus_News_Feed.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            AppDbContext context,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<(bool success, string message)> RequestRegistrationAsync(string email)
        {
            try
            {
                if (!await IsEmailInSchoolDomainAsync(email))
                {
                    return (false, "请使用学校邮箱注册");
                }

                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (existingUser != null)
                {
                    return (false, "该邮箱已注册");
                }

                // 创建新用户（待验证）
                var user = new User
                {
                    Email = email,
                    RegisteredAt = DateTime.UtcNow
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // 生成注册令牌
                string tokenValue = GenerateRandomToken();
                var token = new Token
                {
                    Value = tokenValue,
                    Type = TokenType.Registration,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30)
                };

                _context.Tokens.Add(token);
                await _context.SaveChangesAsync();

                // 发送注册验证邮件
                await _emailService.SendRegistrationTokenAsync(email, tokenValue);

                return (true, "注册链接已发送到您的邮箱");
            }
            catch (Exception ex)
            {
                _logger.LogError($"注册请求失败: {ex.Message}");
                return (false, "注册过程中出现错误");
            }
        }

        public async Task<(bool success, string message, User? user)> VerifyRegistrationAsync(string token)
        {
            try
            {
                var tokenEntity = await _context.Tokens
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => 
                        t.Value == token && 
                        t.Type == TokenType.Registration && 
                        !t.IsUsed && 
                        t.ExpiresAt > DateTime.UtcNow);

                if (tokenEntity == null)
                {
                    return (false, "无效或已过期的注册链接", null);
                }

                // 标记令牌为已使用
                tokenEntity.IsUsed = true;
                await _context.SaveChangesAsync();

                return (true, "注册成功", tokenEntity.User);
            }
            catch (Exception ex)
            {
                _logger.LogError($"验证注册失败: {ex.Message}");
                return (false, "验证过程中出现错误", null);
            }
        }

        public async Task<(bool success, string message)> RequestLoginAsync(string email)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    return (false, "该邮箱未注册");
                }

                // 生成登录令牌
                string tokenValue = GenerateRandomToken();
                var token = new Token
                {
                    Value = tokenValue,
                    Type = TokenType.Login,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30)
                };

                _context.Tokens.Add(token);
                await _context.SaveChangesAsync();

                // 发送登录链接邮件
                await _emailService.SendLoginTokenAsync(email, tokenValue);

                return (true, "登录链接已发送到您的邮箱");
            }
            catch (Exception ex)
            {
                _logger.LogError($"登录请求失败: {ex.Message}");
                return (false, "登录过程中出现错误");
            }
        }

        public async Task<(bool success, string message, User? user)> VerifyLoginAsync(string token)
        {
            try
            {
                var tokenEntity = await _context.Tokens
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => 
                        t.Value == token && 
                        t.Type == TokenType.Login && 
                        !t.IsUsed && 
                        t.ExpiresAt > DateTime.UtcNow);

                if (tokenEntity == null)
                {
                    return (false, "无效或已过期的登录链接", null);
                }

                // 标记令牌为已使用
                tokenEntity.IsUsed = true;
                
                // 更新最后登录时间
                var user = tokenEntity.User;
                if (user != null)
                {
                    user.LastLoginAt = DateTime.UtcNow;
                }
                
                await _context.SaveChangesAsync();

                return (true, "登录成功", user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"验证登录失败: {ex.Message}");
                return (false, "验证过程中出现错误", null);
            }
        }

        public Task<bool> IsEmailInSchoolDomainAsync(string email)
        {
            var allowedDomains = _configuration.GetSection("AppSettings:AllowedEmailDomains").Get<string[]>() ?? Array.Empty<string>();
            
            if (allowedDomains.Length == 0)
            {
                // 如果没有配置允许的域名，默认接受所有邮箱（开发模式）
                return Task.FromResult(true);
            }

            var emailDomain = email.Split('@').Last();
            return Task.FromResult(allowedDomains.Contains(emailDomain));
        }

        private string GenerateRandomToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }
    }
} 