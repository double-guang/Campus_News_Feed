using Campus_News_Feed.Models.Domain;
using Campus_News_Feed.Models.ViewModels;
using Campus_News_Feed.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Campus_News_Feed.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ISessionService _sessionService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(
            IAuthService authService,
            ISessionService sessionService,
            IEmailService emailService,
            ILogger<AuthController> logger,
            IConfiguration configuration)
        {
            _authService = authService;
            _sessionService = sessionService;
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "请输入邮箱地址");
                return View();
            }

            try
            {
                // 验证是否为QQ邮箱，只有管理员可以使用QQ邮箱
                if (email.ToLower().EndsWith("@qq.com"))
                {
                    ModelState.AddModelError("", "普通用户不能使用QQ邮箱注册，请使用吉林大学邮箱");
                    return View();
                }

                // 验证邮箱格式和域名
                _logger.LogInformation($"正在验证邮箱: {email}");
                var isValid = await _authService.ValidateEmailAsync(email);
                if (!isValid)
                {
                    _logger.LogWarning($"邮箱验证失败: {email}");
                    var allowedDomains = _configuration.GetSection("AppSettings:AllowedEmailDomains").Get<string[]>();
                    string domainsText = allowedDomains != null && allowedDomains.Length > 0 
                        ? string.Join(", ", allowedDomains.Where(d => !d.EndsWith("qq.com")).Select(d => $"@{d}")) 
                        : "@mails.jlu.edu.cn, @jlu.edu.cn";
                    
                    ModelState.AddModelError("", $"请使用吉林大学邮箱注册。支持的邮箱域名: {domainsText}");
                    return View();
                }

                // 检查邮箱是否已注册
                var exists = await _authService.EmailExistsAsync(email);
                if (exists)
                {
                    _logger.LogWarning($"邮箱已被注册: {email}");
                    ModelState.AddModelError("", "该邮箱已被注册，请直接登录或使用其他邮箱");
                    return View();
                }

                // 创建注册令牌并发送邮件
                _logger.LogInformation($"创建注册令牌: {email}");
                var token = await _authService.CreateRegistrationTokenAsync(email);
                
                try
                {
                    await _emailService.SendRegistrationTokenAsync(email, token);
                    _logger.LogInformation($"注册验证邮件已发送: {email}");
                    ViewBag.SuccessMessage = "注册链接已发送到您的邮箱，请查收并点击链接完成注册";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "发送注册邮件失败: {Email}", email);
                    ViewBag.SuccessMessage = "注册成功，但发送邮件失败。请查看控制台日志获取验证链接";
                }
                
                return View("RegisterConfirmation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理注册请求时出错: {Email}", email);
                ModelState.AddModelError("", "注册过程中出现错误，请稍后再试");
                return View();
            }
        }

        [HttpGet]
        public IActionResult RegisterConfirmation()
        {
            return View();
        }

        [HttpGet]
        [Route("auth/verify-registration")]
        public async Task<IActionResult> VerifyRegistration(string token)
        {
            _logger.LogInformation($"正在验证注册令牌: {token}");
            
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("验证注册失败: 令牌为空");
                ViewBag.Success = false;
                ViewBag.ErrorMessage = "验证链接无效，请重新注册";
                return View();
            }

            try
            {
                var result = await _authService.VerifyRegistrationTokenAsync(token);
                
                if (result)
                {
                    _logger.LogInformation($"验证注册成功: {token}");
                    ViewBag.Success = true;
                    ViewBag.SuccessMessage = "注册成功！您现在可以登录系统了";
                }
                else
                {
                    _logger.LogWarning($"验证注册失败: 令牌无效或已过期: {token}");
                    ViewBag.Success = false;
                    ViewBag.ErrorMessage = "验证链接无效或已过期，请重新注册";
                }
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证注册令牌时出错: {Token}", token);
                ViewBag.Success = false;
                ViewBag.ErrorMessage = "验证过程中出现错误，请稍后再试";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "请输入邮箱地址");
                return View();
            }

            try
            {
                // 验证是否为QQ邮箱，只有管理员可以使用QQ邮箱
                if (email.ToLower().EndsWith("@qq.com"))
                {
                    // 检查是否为管理员的QQ邮箱
                    var user = await _authService.GetUserByEmailAsync(email);
                    if (user == null || !user.IsAdmin)
                    {
                        ModelState.AddModelError("", "普通用户不能使用QQ邮箱登录，请使用吉林大学邮箱");
                        return View();
                    }
                }
                else
                {
                    // 对于非QQ邮箱，验证是否为吉大邮箱
                    var isValid = await _authService.ValidateEmailAsync(email);
                    if (!isValid)
                    {
                        var allowedDomains = _configuration.GetSection("AppSettings:AllowedEmailDomains").Get<string[]>();
                        string domainsText = allowedDomains != null && allowedDomains.Length > 0 
                            ? string.Join(", ", allowedDomains.Where(d => !d.EndsWith("qq.com")).Select(d => $"@{d}")) 
                            : "@mails.jlu.edu.cn, @jlu.edu.cn";
                        
                        ModelState.AddModelError("", $"请使用吉林大学邮箱登录。支持的邮箱域名: {domainsText}");
                        return View();
                    }
                }

                // 检查邮箱是否存在
                var exists = await _authService.EmailExistsAsync(email);
                if (!exists)
                {
                    ModelState.AddModelError("", "该邮箱尚未注册");
                    return View();
                }

                // 创建登录令牌并发送邮件
                var token = await _authService.CreateLoginTokenAsync(email);
                await _emailService.SendLoginTokenAsync(email, token);

                return RedirectToAction("LoginConfirmation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理登录请求时出错");
                ModelState.AddModelError("", "登录过程中出现错误，请稍后再试");
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> LoginConfirmation()
        {
            // 检查用户是否已登录，如果已登录则重定向到首页
            var currentUser = await _sessionService.GetCurrentUserAsync(HttpContext);
            if (currentUser != null)
            {
                _logger.LogInformation("已登录用户访问登录确认页面，重定向到首页");
                return RedirectToAction("Index", "Home");
            }
            
            return View();
        }

        [HttpGet]
        [Route("auth/verify-login")]
        public async Task<IActionResult> VerifyLogin(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Success = false;
                return View();
            }

            try
            {
                var userId = await _authService.VerifyLoginTokenAsync(token);
                if (userId > 0)
                {
                    var user = await _authService.GetUserByIdAsync(userId);
                    if (user != null)
                    {
                        // 更新最后登录时间
                        user.LastLoginAt = DateTime.UtcNow;
                        await _authService.UpdateUserAsync(user);

                        // 创建会话
                        await _sessionService.CreateSessionAsync(HttpContext, user);
                        
                        ViewBag.Success = true;
                        return View();
                    }
                }

                ViewBag.Success = false;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证登录令牌时出错");
                ViewBag.Success = false;
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _sessionService.DestroySessionAsync(HttpContext);
            return RedirectToAction("Index", "Home");
        }
        
        [HttpGet]
        public async Task<IActionResult> CheckLoginStatus()
        {
            var currentUser = await _sessionService.GetCurrentUserAsync(HttpContext);
            return Json(new { isLoggedIn = currentUser != null });
        }
    }
} 