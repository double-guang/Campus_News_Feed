using Campus_News_Feed.Models.ViewModels;
using Campus_News_Feed.Services;
using Microsoft.AspNetCore.Mvc;

namespace Campus_News_Feed.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly INewsService _newsService;
        private readonly ISessionService _sessionService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userService,
            INewsService newsService,
            ISessionService sessionService,
            ILogger<UserController> logger)
        {
            _userService = userService;
            _newsService = newsService;
            _sessionService = sessionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var currentUser = await _sessionService.GetCurrentUserAsync(HttpContext);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userPreferences = await _userService.GetUserPreferencesAsync(currentUser.Id);
            var allCategories = await _newsService.GetAllCategoriesAsync();

            var viewModel = new UserProfileViewModel
            {
                Email = currentUser.Email,
                RegisteredAt = currentUser.RegisteredAt,
                LastLoginAt = currentUser.LastLoginAt,
                IsAdmin = currentUser.IsAdmin,
                SelectedCategories = userPreferences.Select(p => p.CategoryId).ToList(),
                AllCategories = allCategories.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePreferences(UserProfileViewModel model)
        {
            var currentUser = await _sessionService.GetCurrentUserAsync(HttpContext);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            await _userService.UpdateUserPreferencesAsync(currentUser.Id, model.SelectedCategories);
            
            TempData["SuccessMessage"] = "偏好设置已更新";
            return RedirectToAction("Profile");
        }
    }
} 