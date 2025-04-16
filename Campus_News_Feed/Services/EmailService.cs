using System.Net;
using System.Net.Mail;

namespace Campus_News_Feed.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var host = smtpSettings["Host"];
                var port = int.Parse(smtpSettings["Port"]);
                var username = smtpSettings["Username"];
                var password = smtpSettings["Password"];
                var from = smtpSettings["From"];

                var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true
                };

                var message = new MailMessage(from, to, subject, body)
                {
                    IsBodyHtml = true
                };

                await client.SendMailAsync(message);
                _logger.LogInformation($"邮件已发送到: {to}");
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
                <body>
                    <h2>欢迎注册校园新闻推送系统</h2>
                    <p>请点击以下链接完成注册：</p>
                    <p><a href='{registrationUrl}'>点击这里完成注册</a></p>
                    <p>如果您没有请求注册，请忽略此邮件。</p>
                    <p>链接将在30分钟后失效。</p>
                </body>
                </html>
            ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendLoginTokenAsync(string email, string token)
        {
            var baseUrl = _configuration["AppSettings:BaseUrl"];
            var loginUrl = $"{baseUrl}/auth/verify-login?token={token}";

            var subject = "校园新闻推送系统 - 登录链接";
            var body = $@"
                <html>
                <body>
                    <h2>校园新闻推送系统登录</h2>
                    <p>请点击以下链接完成登录：</p>
                    <p><a href='{loginUrl}'>点击这里登录</a></p>
                    <p>如果您没有请求登录，请忽略此邮件。</p>
                    <p>链接将在30分钟后失效。</p>
                </body>
                </html>
            ";

            await SendEmailAsync(email, subject, body);
        }
    }
} 