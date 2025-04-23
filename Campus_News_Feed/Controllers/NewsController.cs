using Campus_News_Feed.Models.Domain;
using Campus_News_Feed.Models.ViewModels;
using Campus_News_Feed.Services;
using Microsoft.AspNetCore.Mvc;

namespace Campus_News_Feed.Controllers
{
    public class NewsController : Controller
    {
        private readonly INewsService _newsService;
        private readonly ISessionService _sessionService;
        private readonly ILogger<NewsController> _logger;
        private const int PageSize = 30;

        public NewsController(
            INewsService newsService,
            ISessionService sessionService,
            ILogger<NewsController> logger)
        {
            _newsService = newsService;
            _sessionService = sessionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int? page, 
            string? searchTerm = null, 
            int? categoryId = null, 
            DateTime? dateFrom = null, 
            DateTime? dateTo = null, 
            string sort = "comprehensive")
        {
            int pageNumber = page ?? 1;
            var currentUser = await _sessionService.GetCurrentUserAsync(HttpContext);
            int? userId = currentUser?.Id;

            // 将排序参数转换为枚举
            NewsSortOption sortOption = GetSortOptionFromString(sort);
            
            // 使用带筛选功能的方法获取新闻列表
            var paginatedNews = await _newsService.GetFilteredNewsAsync(
                pageNumber, 
                PageSize, 
                searchTerm, 
                categoryId, 
                dateFrom, 
                dateTo, 
                sortOption);
            
            // 获取所有分类用于分类筛选下拉列表
            var categories = await _newsService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            
            var viewModel = new NewsListViewModel
            {
                News = paginatedNews.Select(NewsViewModel.FromNews).ToList(),
                CurrentPage = pageNumber,
                TotalPages = paginatedNews.TotalPages,
                HasNextPage = paginatedNews.HasNextPage,
                HasPreviousPage = paginatedNews.HasPreviousPage,
                SortOption = sortOption,
                SearchTerm = searchTerm,
                FilterCategoryId = categoryId,
                DateFrom = dateFrom,
                DateTo = dateTo,
                TotalItems = paginatedNews.TotalItems
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var news = await _newsService.GetNewsByIdAsync(id);
            if (news == null)
            {
                return View("NotFound");
            }

            // 增加点击量
            await _newsService.IncrementClickCountAsync(id);

            var viewModel = NewsViewModel.FromNews(news);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ByCategory(
            int categoryId, 
            int? page, 
            string? searchTerm = null, 
            DateTime? dateFrom = null, 
            DateTime? dateTo = null, 
            string sort = "comprehensive")
        {
            var category = await _newsService.GetCategoryByIdAsync(categoryId);
            if (category == null)
            {
                return View("NotFound");
            }

            int pageNumber = page ?? 1;
            var currentUser = await _sessionService.GetCurrentUserAsync(HttpContext);
            int? userId = currentUser?.Id;

            // 将排序参数转换为枚举
            NewsSortOption sortOption = GetSortOptionFromString(sort);
            
            // 使用带筛选功能的方法获取新闻列表
            var paginatedNews = await _newsService.GetFilteredNewsAsync(
                pageNumber, 
                PageSize, 
                searchTerm, 
                categoryId, 
                dateFrom, 
                dateTo, 
                sortOption);
            
            // 获取所有分类用于分类筛选下拉列表
            var categories = await _newsService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            
            var viewModel = new NewsListViewModel
            {
                News = paginatedNews.Select(NewsViewModel.FromNews).ToList(),
                CategoryName = category.Name,
                CurrentPage = pageNumber,
                TotalPages = paginatedNews.TotalPages,
                HasNextPage = paginatedNews.HasNextPage,
                HasPreviousPage = paginatedNews.HasPreviousPage,
                CategoryId = categoryId,
                SortOption = sortOption,
                SearchTerm = searchTerm,
                FilterCategoryId = categoryId,
                DateFrom = dateFrom,
                DateTo = dateTo,
                TotalItems = paginatedNews.TotalItems
            };

            return View("Index", viewModel);
        }

        // 辅助方法：将字符串转换为排序选项
        private NewsSortOption GetSortOptionFromString(string sort)
            {
            return sort.ToLower() switch
            {
                "timeasc" => NewsSortOption.TimeAsc,
                "timedesc" => NewsSortOption.TimeDesc,
                _ => NewsSortOption.Comprehensive
            };
        }

        // 以下方法重定向到管理后台
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var currentUser = await _sessionService.GetCurrentUserAsync(HttpContext);
            if (currentUser == null || !currentUser.IsAdmin)
            {
                return RedirectToAction("Login", "Auth");
            }

            // 重定向到管理后台的新闻添加页面
            return RedirectToAction("AddNews", "Admin");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var currentUser = await _sessionService.GetCurrentUserAsync(HttpContext);
            if (currentUser == null || !currentUser.IsAdmin)
            {
                return RedirectToAction("Login", "Auth");
            }

            // 重定向到管理后台的新闻编辑页面
            return RedirectToAction("EditNews", "Admin", new { id = id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUser = await _sessionService.GetCurrentUserAsync(HttpContext);
            if (currentUser == null || !currentUser.IsAdmin)
            {
                return RedirectToAction("Login", "Auth");
            }

            // 重定向到管理后台
            return RedirectToAction("News", "Admin");
        }
    }
} 