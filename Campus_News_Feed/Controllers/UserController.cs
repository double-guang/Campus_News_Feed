using Campus_News_Feed.Models.ViewModels;
using Campus_News_Feed.Services;
using Microsoft.AspNetCore.Mvc;

namespace Campus_News_Feed.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ISessionService _sessionService;
        private readonly INewsService _newsService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userService,
            ISessionService sessionService,
            INewsService newsService,
            ILogger<UserController> logger)
        {
            _userService = userService;
            _sessionService = sessionService;
            _newsService = newsService;
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
                AllCategories = allCategories.ToList(),
                SelectedCategoryIds = userPreferences.Select(p => p.Id).ToList()
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

            var success = await _userService.UpdateUserPreferencesAsync(currentUser.Id, model.SelectedCategoryIds);
            if (success)
            {
                TempData["SuccessMessage"] = "偏好设置已更新";
            }
            else
            {
                TempData["ErrorMessage"] = "更新偏好设置失败";
            }

            return RedirectToAction("Profile");
        }
    }
} 