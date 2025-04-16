using Campus_News_Feed.Models.ViewModels;
using Campus_News_Feed.Services;
using Microsoft.AspNetCore.Mvc;

namespace Campus_News_Feed.Controllers
{
    public class AdminController : Controller
    {
        private readonly INewsService _newsService;
        private readonly ISessionService _sessionService;
        private readonly IUserService _userService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            INewsService newsService,
            ISessionService sessionService,
            IUserService userService,
            ILogger<AdminController> logger)
        {
            _newsService = newsService;
            _sessionService = sessionService;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 验证是否为管理员
            if (!await _sessionService.IsAdminAsync(HttpContext))
            {
                return RedirectToAction("Login", "Auth");
            }

            var newsList = await _newsService.GetAllNewsAsync();
            var viewModel = new NewsListViewModel
            {
                News = newsList.Select(NewsViewModel.FromNews).ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            // 如果已经是管理员，直接进入后台
            if (await _sessionService.IsAdminAsync(HttpContext))
            {
                return RedirectToAction("Index");
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 检查是否是管理员邮箱
            var user = await _userService.GetUserByEmailAsync(model.Email);
            if (user == null || !user.IsAdmin)
            {
                ModelState.AddModelError(string.Empty, "无效的管理员账号");
                return View(model);
            }

            // 简化管理员登录流程，使用与普通用户相同的邮箱验证
            return RedirectToAction("Login", "Auth");
        }
    }
} 