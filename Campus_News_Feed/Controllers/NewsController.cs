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
        public async Task<IActionResult> Index()
        {
            var newsList = await _newsService.GetAllNewsAsync();
            var viewModel = new NewsListViewModel
            {
                News = newsList.Select(NewsViewModel.FromNews).ToList(),
                IsRecommended = false
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Category(int id)
        {
            var category = (await _newsService.GetAllCategoriesAsync()).FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            var newsList = await _newsService.GetNewsByCategoryIdAsync(id);
            var viewModel = new NewsListViewModel
            {
                News = newsList.Select(NewsViewModel.FromNews).ToList(),
                CategoryName = category.Name,
                IsRecommended = false
            };

            return View("Index", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Recommended()
        {
            var currentUser = await _sessionService.GetCurrentUserAsync(HttpContext);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var recommendedNews = await _newsService.GetRecommendedNewsForUserAsync(currentUser.Id);
            var viewModel = new NewsListViewModel
            {
                News = recommendedNews.Select(NewsViewModel.FromNews).ToList(),
                IsRecommended = true
            };

            return View("Index", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var news = await _newsService.GetNewsByIdAsync(id);
            if (news == null)
            {
                return NotFound();
            }

            // 增加点击计数
            await _newsService.IncrementClickCountAsync(id);

            var viewModel = NewsViewModel.FromNews(news);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // 验证是否为管理员
            if (!await _sessionService.IsAdminAsync(HttpContext))
            {
                return Forbid();
            }

            var categories = await _newsService.GetAllCategoriesAsync();
            var viewModel = new NewsCreateViewModel
            {
                Categories = categories.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(NewsCreateViewModel model)
        {
            // 验证是否为管理员
            if (!await _sessionService.IsAdminAsync(HttpContext))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                model.Categories = (await _newsService.GetAllCategoriesAsync()).ToList();
                return View(model);
            }

            var news = new News
            {
                Title = model.Title,
                Content = model.Content,
                CategoryId = model.CategoryId
            };

            await _newsService.CreateNewsAsync(news);
            TempData["SuccessMessage"] = "新闻发布成功";
            return RedirectToAction("Index", "Admin");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // 验证是否为管理员
            if (!await _sessionService.IsAdminAsync(HttpContext))
            {
                return Forbid();
            }

            var news = await _newsService.GetNewsByIdAsync(id);
            if (news == null)
            {
                return NotFound();
            }

            var categories = await _newsService.GetAllCategoriesAsync();
            var viewModel = new NewsEditViewModel
            {
                Id = news.Id,
                Title = news.Title,
                Content = news.Content,
                CategoryId = news.CategoryId,
                Categories = categories.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(NewsEditViewModel model)
        {
            // 验证是否为管理员
            if (!await _sessionService.IsAdminAsync(HttpContext))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                model.Categories = (await _newsService.GetAllCategoriesAsync()).ToList();
                return View(model);
            }

            var news = new News
            {
                Id = model.Id,
                Title = model.Title,
                Content = model.Content,
                CategoryId = model.CategoryId
            };

            var result = await _newsService.UpdateNewsAsync(model.Id, news);
            if (result == null)
            {
                TempData["ErrorMessage"] = "更新新闻失败";
                return RedirectToAction("Index", "Admin");
            }

            TempData["SuccessMessage"] = "新闻更新成功";
            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            // 验证是否为管理员
            if (!await _sessionService.IsAdminAsync(HttpContext))
            {
                return Forbid();
            }

            var result = await _newsService.DeleteNewsAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "新闻删除成功";
            }
            else
            {
                TempData["ErrorMessage"] = "删除新闻失败";
            }

            return RedirectToAction("Index", "Admin");
        }
    }
} 