using Campus_News_Feed.Models.Domain;
using Campus_News_Feed.Models.ViewModels;
using Campus_News_Feed.Services;
using Microsoft.AspNetCore.Mvc;

namespace Campus_News_Feed.Controllers
{
    public class AdminCategoryController : Controller
    {
        private readonly INewsService _newsService;
        private readonly ISessionService _sessionService;
        private readonly ILogger<AdminCategoryController> _logger;

        public AdminCategoryController(
            INewsService newsService,
            ISessionService sessionService,
            ILogger<AdminCategoryController> logger)
        {
            _newsService = newsService;
            _sessionService = sessionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 检查是否是管理员
            var currentUser = await _sessionService.GetCurrentUserAsync(HttpContext);
            if (currentUser == null || !currentUser.IsAdmin)
            {
                _logger.LogWarning("未授权用户尝试访问分类管理");
                return RedirectToAction("Login", "Auth");
            }

            var categories = await _newsService.GetAllCategoriesAsync();
            var viewModel = new CategoryListViewModel
            {
                Categories = new List<CategoryViewModel>()
            };

            foreach (var category in categories)
            {
                int newsCount = await _newsService.GetNewsByCategoryCountAsync(category.Id);
                viewModel.Categories.Add(CategoryViewModel.FromCategory(category, newsCount));
            }

            _logger.LogInformation("管理员访问分类管理页面，显示 {CategoryCount} 个分类", categories.Count());
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // 检查是否是管理员
            var currentUser = await _sessionService.GetCurrentUserAsync(HttpContext);
            if (currentUser == null || !currentUser.IsAdmin)
            {
                _logger.LogWarning("未授权用户尝试访问创建分类页面");
                return RedirectToAction("Login", "Auth");
            }

            return View(new CategoryCreateViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryCreateViewModel model)
        {
            // 检查是否是管理员
            var currentUser = await _sessionService.GetCurrentUserAsync(HttpContext);
            if (currentUser == null || !currentUser.IsAdmin)
            {
                _logger.LogWarning("未授权用户尝试创建分类");
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 检查分类名称是否已存在
            var existingCategories = await _newsService.GetAllCategoriesAsync();
            if (existingCategories.Any(c => c.Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("Name", "该分类名称已存在");
                return View(model);
            }

            var category = new Category
            {
                Name = model.Name,
                Description = model.Description
            };

            try
            {
                await _newsService.CreateCategoryAsync(category);
                _logger.LogInformation("管理员创建了新分类: {CategoryName}", model.Name);
                TempData["SuccessMessage"] = "分类创建成功";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建分类时出错: {CategoryName}", model.Name);
                ModelState.AddModelError("", "创建分类时发生错误，请稍后再试");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // 检查是否是管理员
            var currentUser = await _sessionService.GetCurrentUserAsync(HttpContext);
            if (currentUser == null || !currentUser.IsAdmin)
            {
                _logger.LogWarning("未授权用户尝试访问编辑分类页面");
                return RedirectToAction("Login", "Auth");
            }

            var category = await _newsService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning("尝试编辑不存在的分类: {CategoryId}", id);
                return View("NotFound");
            }

            var viewModel = new CategoryEditViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CategoryEditViewModel model)
        {
            // 检查是否是管理员
            var currentUser = await _sessionService.GetCurrentUserAsync(HttpContext);
            if (currentUser == null || !currentUser.IsAdmin)
            {
                _logger.LogWarning("未授权用户尝试更新分类");
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 检查分类名称是否已存在（除了当前编辑的分类）
            var existingCategories = await _newsService.GetAllCategoriesAsync();
            if (existingCategories.Any(c => c.Id != model.Id && c.Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("Name", "该分类名称已存在");
                return View(model);
            }

            var category = new Category
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description
            };

            try
            {
                var result = await _newsService.UpdateCategoryAsync(category);
                if (result)
                {
                    _logger.LogInformation("管理员更新了分类: {CategoryId}, {CategoryName}", model.Id, model.Name);
                    TempData["SuccessMessage"] = "分类更新成功";
                    return RedirectToAction("Index");
                }
                else
                {
                    _logger.LogWarning("更新分类失败: {CategoryId}", model.Id);
                    ModelState.AddModelError("", "更新分类失败，分类可能已被删除");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新分类时出错: {CategoryId}, {CategoryName}", model.Id, model.Name);
                ModelState.AddModelError("", "更新分类时发生错误，请稍后再试");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            // 检查是否是管理员
            var currentUser = await _sessionService.GetCurrentUserAsync(HttpContext);
            if (currentUser == null || !currentUser.IsAdmin)
            {
                _logger.LogWarning("未授权用户尝试访问删除分类页面");
                return RedirectToAction("Login", "Auth");
            }

            var category = await _newsService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning("尝试删除不存在的分类: {CategoryId}", id);
                return View("NotFound");
            }

            int newsCount = await _newsService.GetNewsByCategoryCountAsync(id);
            var viewModel = CategoryViewModel.FromCategory(category, newsCount);

            return View(viewModel);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 检查是否是管理员
            var currentUser = await _sessionService.GetCurrentUserAsync(HttpContext);
            if (currentUser == null || !currentUser.IsAdmin)
            {
                _logger.LogWarning("未授权用户尝试删除分类");
                return RedirectToAction("Login", "Auth");
            }

            var category = await _newsService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning("尝试删除不存在的分类: {CategoryId}", id);
                TempData["ErrorMessage"] = "找不到要删除的分类";
                return RedirectToAction("Index");
            }

            int newsCount = await _newsService.GetNewsByCategoryCountAsync(id);
            if (newsCount > 0)
            {
                _logger.LogWarning("尝试删除包含新闻的分类: {CategoryId}, {NewsCount}条新闻", id, newsCount);
                TempData["ErrorMessage"] = $"无法删除分类。该分类包含{newsCount}条新闻，请先将新闻移动到其他分类。";
                return RedirectToAction("Index");
            }

            try
            {
                var result = await _newsService.DeleteCategoryAsync(id);
                if (result)
                {
                    _logger.LogInformation("管理员删除了分类: {CategoryId}, {CategoryName}", id, category.Name);
                    TempData["SuccessMessage"] = "分类删除成功";
                }
                else
                {
                    _logger.LogWarning("删除分类失败: {CategoryId}", id);
                    TempData["ErrorMessage"] = "无法删除分类，请稍后重试。";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除分类时出错: {CategoryId}", id);
                TempData["ErrorMessage"] = "删除分类时发生错误，请稍后再试。";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // 检查是否是管理员
            var currentUser = await _sessionService.GetCurrentUserAsync(HttpContext);
            if (currentUser == null || !currentUser.IsAdmin)
            {
                return RedirectToAction("Login", "Auth");
            }

            var category = await _newsService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return View("NotFound");
            }

            int newsCount = await _newsService.GetNewsByCategoryCountAsync(id);
            var viewModel = CategoryViewModel.FromCategory(category, newsCount);

            return View(viewModel);
        }
    }
} 