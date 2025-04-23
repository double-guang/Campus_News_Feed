using Campus_News_Feed.Services;
using Microsoft.AspNetCore.Mvc;

namespace Campus_News_Feed.Controllers
{
    public class CategoryController : Controller
    {
        private readonly INewsService _newsService;
        private readonly ISessionService _sessionService;

        public CategoryController(
            INewsService newsService,
            ISessionService sessionService)
        {
            _newsService = newsService;
            _sessionService = sessionService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories = await _newsService.GetAllCategoriesAsync();
            return View(categories);
        }
    }
} 