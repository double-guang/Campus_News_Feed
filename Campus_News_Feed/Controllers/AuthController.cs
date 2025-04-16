using Campus_News_Feed.Models.Domain;
using Campus_News_Feed.Models.ViewModels;
using Campus_News_Feed.Services;
using Microsoft.AspNetCore.Mvc;

namespace Campus_News_Feed.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ISessionService _sessionService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            ISessionService sessionService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _sessionService = sessionService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.RequestLoginAsync(model.Email);
            if (result.success)
            {
                TempData["SuccessMessage"] = result.message;
                return RedirectToAction("LoginConfirmation");
            }

            ModelState.AddModelError(string.Empty, result.message);
            return View(model);
        }

        [HttpGet]
        public IActionResult LoginConfirmation()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> VerifyLogin(string token)
        {
            var result = await _authService.VerifyLoginAsync(token);
            if (result.success && result.user != null)
            {
                await _sessionService.CreateUserSessionAsync(HttpContext, result.user);
                return RedirectToAction("Index", "Home");
            }

            TempData["ErrorMessage"] = result.message;
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.RequestRegistrationAsync(model.Email);
            if (result.success)
            {
                TempData["SuccessMessage"] = result.message;
                return RedirectToAction("RegisterConfirmation");
            }

            ModelState.AddModelError(string.Empty, result.message);
            return View(model);
        }

        [HttpGet]
        public IActionResult RegisterConfirmation()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> VerifyRegistration(string token)
        {
            var result = await _authService.VerifyRegistrationAsync(token);
            if (result.success && result.user != null)
            {
                await _sessionService.CreateUserSessionAsync(HttpContext, result.user);
                return RedirectToAction("Index", "Home");
            }

            TempData["ErrorMessage"] = result.message;
            return RedirectToAction("Register");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _sessionService.ClearUserSessionAsync(HttpContext);
            return RedirectToAction("Index", "Home");
        }
    }
} 