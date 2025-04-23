using System.Net;
using System.Net.Mail;

namespace Campus_News_Feed.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly IWebHostEnvironment _environment;

        public EmailService(
            IConfiguration configuration, 
            ILogger<EmailService> logger,
            IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _logger = logger;
            _environment = environment;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                // 读取SMTP配置
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var host = smtpSettings["Host"];
                var port = int.Parse(smtpSettings["Port"] ?? "25");
                var username = smtpSettings["Username"];
                var password = smtpSettings["Password"];
                var fromEmail = smtpSettings["FromEmail"] ?? username;
                var fromName = smtpSettings["FromName"] ?? "校园新闻推送系统";
                var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(fromEmail))
                {
                    _logger.LogError("SMTP配置缺失，无法发送邮件");
                    throw new InvalidOperationException("SMTP配置不完整");
                }

                // 开发环境下显示邮件信息但不实际发送
                if (_environment.IsDevelopment() && username.Contains("您的QQ邮箱"))
                {
                    _logger.LogWarning("================================================");
                    _logger.LogWarning("SMTP未配置，邮件将只在控制台显示，不会真实发送");
                    _logger.LogWarning("请在appsettings.json中配置有效的SMTP设置");
                    _logger.LogWarning("================================================");
                    _logger.LogInformation($"[DEV模式] 邮件内容 - 收件人: {to}, 主题: {subject}");
                    _logger.LogInformation($"[DEV模式] 邮件正文: {body}");
                    return;
                }

                // 配置SmtpClient
                var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = enableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 20000 // 20秒超时时间
                };

                // 禁用证书验证以避免SSL/TLS证书问题
                ServicePointManager.ServerCertificateValidationCallback = 
                    (sender, certificate, chain, sslPolicyErrors) => true;

                // 创建邮件
                var from = new MailAddress(fromEmail, fromName);
                var message = new MailMessage
                {
                    From = from,
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(to);

                // 发送邮件
                await client.SendMailAsync(message);
                _logger.LogInformation($"邮件已成功发送到: {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"发送邮件失败: {ex.Message}");
                throw;
            }
        }

        public async Task SendRegistrationTokenAsync(string email, string token)
        {
            var baseUrl = _configuration["AppSettings:BaseUrl"];
            var registrationUrl = $"{baseUrl}/auth/verify-registration?token={token}";

            var subject = "校园新闻推送系统 - 完成注册";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 5px;'>
                    <div style='text-align: center; padding: 10px; background-color: #f8f9fa; margin-bottom: 20px;'>
                        <h2 style='color: #0056b3;'>校园新闻推送系统</h2>
                    </div>
                    <h3>欢迎注册校园新闻推送系统</h3>
                    <p>您好！感谢您注册我们的校园新闻推送系统。请点击下面的按钮完成注册：</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{registrationUrl}' style='background-color: #0056b3; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px; font-weight: bold;'>点击这里完成注册</a>
                    </div>
                    <p>如果按钮无法点击，您也可以复制以下链接到浏览器地址栏访问：</p>
                    <p style='word-break: break-all; background-color: #f8f9fa; padding: 10px; border-radius: 4px;'>{registrationUrl}</p>
                    <p>如果您没有请求注册，请忽略此邮件。</p>
                    <p>链接将在30分钟后失效。</p>
                    <div style='margin-top: 40px; padding-top: 20px; border-top: 1px solid #eee; color: #6c757d; font-size: 0.9em;'>
                        <p>此邮件由系统自动发送，请勿回复。</p>
                    </div>
                </body>
                </html>
            ";

            // 开发环境下在控制台输出验证链接
            if (_environment.IsDevelopment())
            {
                _logger.LogWarning("================================================");
                _logger.LogWarning("开发环境: 使用以下链接完成注册");
                _logger.LogWarning($"注册验证链接: {registrationUrl}");
                _logger.LogWarning("================================================");
                
                // 也将链接写入控制台，使其更容易复制
                Console.WriteLine($"\n⭐ 注册验证链接: {registrationUrl} ⭐\n");
            }

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendLoginTokenAsync(string email, string token)
        {
            var baseUrl = _configuration["AppSettings:BaseUrl"];
            var loginUrl = $"{baseUrl}/auth/verify-login?token={token}";

            var subject = "校园新闻推送系统 - 登录链接";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 5px;'>
                    <div style='text-align: center; padding: 10px; background-color: #f8f9fa; margin-bottom: 20px;'>
                        <h2 style='color: #0056b3;'>校园新闻推送系统</h2>
                    </div>
                    <h3>登录校园新闻推送系统</h3>
                    <p>您好！您请求了登录校园新闻推送系统。请点击下面的按钮完成登录：</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{loginUrl}' style='background-color: #0056b3; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px; font-weight: bold;'>点击这里登录</a>
                    </div>
                    <p>如果按钮无法点击，您也可以复制以下链接到浏览器地址栏访问：</p>
                    <p style='word-break: break-all; background-color: #f8f9fa; padding: 10px; border-radius: 4px;'>{loginUrl}</p>
                    <p>如果您没有请求登录，请忽略此邮件。</p>
                    <p>链接将在30分钟后失效。</p>
                    <div style='margin-top: 40px; padding-top: 20px; border-top: 1px solid #eee; color: #6c757d; font-size: 0.9em;'>
                        <p>此邮件由系统自动发送，请勿回复。</p>
                    </div>
                </body>
                </html>
            ";

            // 开发环境下在控制台输出验证链接
            if (_environment.IsDevelopment())
            {
                _logger.LogWarning("================================================");
                _logger.LogWarning("开发环境: 使用以下链接完成登录");
                _logger.LogWarning($"登录验证链接: {loginUrl}");
                _logger.LogWarning("================================================");
                
                // 也将链接写入控制台，使其更容易复制
                Console.WriteLine($"\n⭐ 登录验证链接: {loginUrl} ⭐\n");
            }

            await SendEmailAsync(email, subject, body);
        }
    }
} 