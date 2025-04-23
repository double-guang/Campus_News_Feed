using Campus_News_Feed.Models.Domain;
using Campus_News_Feed.Models.ViewModels;
using Campus_News_Feed.Services;
using Microsoft.AspNetCore.Mvc;
using Campus_News_Feed.Tools;

namespace Campus_News_Feed.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ISessionService _sessionService;
        private readonly INewsService _newsService;
        private readonly IUserService _userService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IAuthService authService,
            ISessionService sessionService,
            INewsService newsService,
            IUserService userService,
            ILogger<AdminController> logger)
        {
            _authService = authService;
            _sessionService = sessionService;
            _newsService = newsService;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(AdminLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _authService.ValidateAdminCredentialsAsync(model.Username, model.Password);
                if (user == null)
                {
                    _logger.LogWarning($"管理员登录失败: {model.Username}");
                    ModelState.AddModelError("", "用户名或密码错误");
                    return View(model);
                }

                await _sessionService.CreateSessionAsync(HttpContext, user);
                
                _logger.LogInformation($"管理员登录成功: {user.Id}, {user.Email}");
                return RedirectToAction("Index", "Admin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "管理员登录过程中出错");
                ModelState.AddModelError("", "登录过程中出现错误，请稍后再试");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> News(
            int page = 1, 
            string? searchTerm = null, 
            int? categoryId = null, 
            DateTime? dateFrom = null, 
            DateTime? dateTo = null, 
            NewsSortOption sortOption = NewsSortOption.Comprehensive)
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Login");
            }

            const int pageSize = 30; // 每页显示30条新闻
            
            // 使用筛选方法获取新闻列表
            var paginatedNews = await _newsService.GetFilteredNewsAsync(
                page, pageSize, searchTerm, categoryId, dateFrom, dateTo, sortOption);
            
            // 获取所有分类用于筛选器
            var categories = await _newsService.GetAllCategoriesAsync();
            
            // 创建视图模型，包含分页信息
            var viewModel = new NewsListViewModel
            {
                News = paginatedNews.Select(NewsViewModel.FromNews).ToList(),
                CurrentPage = page,
                TotalPages = paginatedNews.TotalPages,
                HasNextPage = paginatedNews.HasNextPage,
                HasPreviousPage = paginatedNews.HasPreviousPage,
                TotalItems = paginatedNews.TotalItems,
                SearchTerm = searchTerm,
                FilterCategoryId = categoryId,
                DateFrom = dateFrom,
                DateTo = dateTo,
                SortOption = sortOption
            };
            
            ViewBag.Categories = categories;
            
            return View(viewModel);
        }

        // 添加新闻
        [HttpGet]
        public async Task<IActionResult> AddNews()
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Login");
            }

            var categories = await _newsService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddNews(NewsViewModel model)
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                var categories = await _newsService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;
                return View(model);
            }

            try
            {
                var news = new News
                {
                    Title = model.Title,
                    Content = model.Content,
                    CategoryId = model.CategoryId,
                    CreatedAt = DateTime.UtcNow
                };

                await _newsService.CreateNewsAsync(news);
                return RedirectToAction("News");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加新闻时出错");
                ModelState.AddModelError("", "添加新闻时出现错误，请稍后再试");
                var categories = await _newsService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;
                return View(model);
            }
        }

        // 编辑新闻
        [HttpGet]
        public async Task<IActionResult> EditNews(int id)
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Login");
            }

            var news = await _newsService.GetNewsByIdAsync(id);
            if (news == null)
            {
                return NotFound();
            }

            var model = new NewsViewModel
            {
                Id = news.Id,
                Title = news.Title,
                Content = news.Content,
                CategoryId = news.CategoryId,
                CategoryName = news.Category?.Name ?? ""
            };

            var categories = await _newsService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditNews(NewsViewModel model)
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                var categories = await _newsService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;
                return View(model);
            }

            try
            {
                var news = await _newsService.GetNewsByIdAsync(model.Id);
                if (news == null)
                {
                    return NotFound();
                }

                news.Title = model.Title;
                news.Content = model.Content;
                news.CategoryId = model.CategoryId;
                news.UpdatedAt = DateTime.UtcNow;

                await _newsService.UpdateNewsAsync(news);
                return RedirectToAction("News");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "编辑新闻时出错: {NewsId}", model.Id);
                ModelState.AddModelError("", "编辑新闻时出现错误，请稍后再试");
                var categories = await _newsService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;
                return View(model);
            }
        }

        // 删除新闻
        [HttpPost]
        public async Task<IActionResult> DeleteNews(int id)
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Login");
            }

            try
            {
                await _newsService.DeleteNewsAsync(id);
                return RedirectToAction("News");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除新闻时出错: {NewsId}", id);
                return RedirectToAction("News");
            }
        }

        // 生成测试数据
        [HttpGet]
        public async Task<IActionResult> GenerateTestData()
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Login");
            }

            try
            {
                await DataGenerator.GenerateNewsData(_newsService.GetDbContext(), 50);
                return RedirectToAction("News", new { message = "成功生成50条测试新闻数据" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成测试数据时出错");
                return RedirectToAction("News", new { error = "生成测试数据失败：" + ex.Message });
            }
        }

        // 用户管理 - 用户列表
        [HttpGet]
        public async Task<IActionResult> Users(int page = 1, string? searchTerm = null, string sortBy = "registerDate", bool? isActive = null)
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Login");
            }

            try
            {
                const int pageSize = 30; // 每页显示30个用户
                
                // 获取分页的用户列表（使用新的带搜索和排序功能的方法）
                var users = await _userService.GetPaginatedUsersAsync(page, pageSize, searchTerm, sortBy, isActive);
                
                // 创建视图模型
                var viewModel = new UserListViewModel
                {
                    Users = users.Select(UserViewModel.FromUser).ToList(),
                    CurrentPage = page,
                    TotalPages = users.TotalPages,
                    HasPreviousPage = users.HasPreviousPage,
                    HasNextPage = users.HasNextPage,
                    SearchTerm = searchTerm,
                    SortBy = sortBy,
                    IsActive = isActive,
                    TotalItems = users.TotalItems
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户列表时发生错误");
                TempData["ErrorMessage"] = "获取用户列表失败，请稍后再试";
                return View(new UserListViewModel 
                { 
                    Users = new List<UserViewModel>(),
                    CurrentPage = 1,
                    TotalPages = 1,
                    HasPreviousPage = false,
                    HasNextPage = false
                });
            }
        }

        // 用户管理 - 用户详情
        [HttpGet]
        public async Task<IActionResult> UserDetails(int id)
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Login");
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // 获取用户偏好
            var userPreferences = await _userService.GetUserCategoriesAsync(user.Id);
            var categories = await _newsService.GetAllCategoriesAsync();

            var viewModel = new UserDetailsViewModel
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                RegisteredAt = user.RegisteredAt,
                LastLoginAt = user.LastLoginAt,
                IsAdmin = user.IsAdmin,
                IsActive = true, // 临时硬编码为活跃状态
                UserPreferences = userPreferences.ToList(),
                AllCategories = categories.ToList()
            };

            return View(viewModel);
        }

        // 用户管理 - 注销用户
        [HttpPost]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Login");
            }

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // 防止注销自己或其他管理员
            var currentUser = await _sessionService.GetCurrentUserAsync(HttpContext);
            if (currentUser?.Id == id || (user.IsAdmin && currentUser?.Id != id))
            {
                TempData["ErrorMessage"] = "不能注销管理员账号";
                return RedirectToAction("Users");
            }

            try
            {
                await _userService.DeactivateUserAsync(id);
                TempData["SuccessMessage"] = $"用户 {(string.IsNullOrEmpty(user.Username) ? user.Email : user.Username)} 的偏好设置已被清除（临时注销措施）";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "注销用户时出错: {UserId}", id);
                TempData["ErrorMessage"] = "注销用户时出错";
            }

            return RedirectToAction("Users");
        }

        // 帮助方法
        private async Task<bool> IsAdmin()
        {
            var currentUser = await _sessionService.GetCurrentUserAsync(HttpContext);
            return currentUser != null && currentUser.IsAdmin;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetSystemStats()
        {
            if (!await IsAdmin())
            {
                return Unauthorized();
            }
            
            try 
            {
                var newsCount = await _newsService.GetNewsCountAsync();
                var userCount = await _userService.GetUserCountAsync();
                
                _logger.LogInformation("系统统计数据获取成功: 新闻数={NewsCount}, 用户数={UserCount}", newsCount, userCount);
                
                return Json(new { 
                    newsCount, 
                    userCount,
                    success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取系统统计数据失败");
                return Json(new {
                    newsCount = 0,
                    userCount = 0,
                    success = false,
                    message = "获取统计数据时发生错误"
                });
            }
        }
    }
} 