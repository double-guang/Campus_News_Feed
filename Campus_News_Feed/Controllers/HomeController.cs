using Campus_News_Feed.Data;
using Campus_News_Feed.Models.ViewModels;
using Campus_News_Feed.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Campus_News_Feed.Controllers
{
    public class HomeController : Controller
    {
        private readonly INewsService _newsService;
        private readonly ISessionService _sessionService;
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _dbContext;
        private const int PageSize = 30;
        private const int HomePageNewsCount = 10;

        public HomeController(
            INewsService newsService,
            ISessionService sessionService,
            ILogger<HomeController> logger,
            AppDbContext dbContext)
        {
            _newsService = newsService;
            _sessionService = sessionService;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _sessionService.GetCurrentUserAsync(HttpContext);
            
            IEnumerable<Models.Domain.News> recommendedNews;
            
            if (currentUser != null)
            {
                // 已登录用户：基于用户偏好、点击量和时间推荐
                recommendedNews = await _newsService.GetRecommendedNewsAsync(currentUser.Id);
            }
            else
            {
                // 未登录用户：基于点击量和时间推荐热门新闻
                recommendedNews = await _newsService.GetHotNewsAsync(HomePageNewsCount);
            }
            
            var viewModel = new NewsListViewModel
            {
                News = recommendedNews.Take(HomePageNewsCount).Select(NewsViewModel.FromNews).ToList(),
                CurrentPage = 1,
                TotalPages = 1, // 首页只显示一页
                HasNextPage = false,
                HasPreviousPage = false
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

        [HttpGet]
        public async Task<IActionResult> Debug()
        {
            var users = await _dbContext.Users.ToListAsync();
            var tokens = await _dbContext.Tokens.ToListAsync();
            
            ViewBag.Users = users;
            ViewBag.Tokens = tokens;
            
            return View();
        }
    }
} 