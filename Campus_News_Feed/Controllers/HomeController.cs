using Campus_News_Feed.Models.ViewModels;
using Campus_News_Feed.Services;
using Microsoft.AspNetCore.Mvc;

namespace Campus_News_Feed.Controllers
{
    public class HomeController : Controller
    {
        private readonly INewsService _newsService;
        private readonly ISessionService _sessionService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            INewsService newsService,
            ISessionService sessionService,
            ILogger<HomeController> logger)
        {
            _newsService = newsService;
            _sessionService = sessionService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _sessionService.GetCurrentUserAsync(HttpContext);
            
            // 如果用户已登录，默认展示推荐新闻
            if (currentUser != null)
            {
                return RedirectToAction("Recommended", "News");
            }

            // 未登录用户展示所有新闻
            var newsList = await _newsService.GetAllNewsAsync();
            var viewModel = new NewsListViewModel
            {
                News = newsList.Select(NewsViewModel.FromNews).ToList(),
                IsRecommended = false
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Categories()
        {
            var categories = await _newsService.GetAllCategoriesAsync();
            return View(categories);
        }

        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Error()
        {
            return View();
        }
    }
} 