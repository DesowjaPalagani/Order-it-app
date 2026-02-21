using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderItApp.Models;
using OrderItApp.Data;
using OrderItApp.Utilities;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OrderItApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly Microsoft.AspNetCore.Identity.IPasswordHasher<User> _hasher;
        private readonly Microsoft.Extensions.Logging.ILogger<AccountController> _logger;

        public AccountController(IUserService userService,
                                 Microsoft.AspNetCore.Identity.IPasswordHasher<User> hasher,
                                 Microsoft.Extensions.Logging.ILogger<AccountController> logger)
        {
            _userService = userService;
            _hasher = hasher;
            _logger = logger;
        }
        // GET: /Account/Login
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Mode"] = "Login";
            return View(new AuthViewModel());
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AuthViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Mode"] = "Login";

            // Only validate the Login submodel — the AuthViewModel also contains
            // a Signup submodel which will be empty when logging in and would
            // otherwise cause ModelState to be invalid. Clear previous state and
            // re-validate only the login portion.
            ModelState.Clear();
            TryValidateModel(model.Login, nameof(model.Login));

            _logger?.LogInformation("Login POST received. Login ModelState.IsValid={valid}", ModelState.IsValid);

            if (ModelState.IsValid)
            {
                var login = model.Login;
                // same normalization as registration – treat login identifiers
                // case‑insensitively by lowercasing before lookup
                var identifier = login.UserName.Trim().ToLower();
                var user = await _userService.GetByIdentifierAsync(identifier);

                _logger?.LogInformation("Login attempt for identifier '{identifier}' (found user: {found})", identifier, user != null);
                System.Console.WriteLine($"[DEBUG] Login attempt for {identifier}. User found: {user != null}");

                if (user != null)
                {
                    var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, login.Password);
                    _logger?.LogInformation("Password verification result for user {userId}: {result}", user.Id.ToString(), result.ToString());
                    System.Console.WriteLine($"[DEBUG] Password verification result for {user.Id}: {result}");

                    if (result == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(ClaimTypes.Email, user.Email)
                        };

                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }

                        // After successful login redirect to Home/Index.
                        return RedirectToAction("Index", "Home");
                    }
                }
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
            }
            return View(model);
        }

        // GET: /Account/Register
        [AllowAnonymous]
        public IActionResult Register()
        {
            ViewData["Mode"] = "Register";
            return View("Login", new AuthViewModel());
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AuthViewModel model)
        {
            // always show the registration form when redisplaying the page
            ViewData["Mode"] = "Register";

            // the incoming view model contains both a Login and a Signup section.  when
            // the user is registering we only care about the Signup portion, yet the
            // login submodel is decorated with [Required] attributes.  the default
            // model binder will validate everything and ModelState.IsValid will be false
            // as soon as the empty login fields cause validation errors.  the result is
            // that the form is redisplayed without the registration logic ever running
            // (and confusing "username required" messages appear in the login form).
            //
            // clear the previous validation state and explicitly re‑validate only the
            // signup submodel so that we ignore the unrelated login errors.
            ModelState.Clear();
            TryValidateModel(model.Signup, nameof(model.Signup));

            if (ModelState.IsValid)
            {
                // normalize and store identifiers as lowercase so we can rely on
                // simple equality comparisons in queries (MongoDB doesn't support
                // string methods like ToLower() inside filters).
                var userName = model.Signup.UserName.Trim().ToLower();
                var email = model.Signup.Email.Trim().ToLower();

                if (await _userService.ExistsByEmailOrUserNameAsync(email, userName))
                {
                    ModelState.AddModelError(string.Empty, "Username or email already registered.");
                    return View("Login", model);
                }

                var user = new User
                {
                    UserName = userName,
                    Email = email,
                    CreatedAt = DateTime.UtcNow
                };
                user.PasswordHash = _hasher.HashPassword(user, model.Signup.Password);

                await _userService.CreateUserAsync(user);

                TempData["SuccessMessage"] = "Registration successful. Please login.";
                return RedirectToAction("Login", "Account");
            }

            // If we get here something failed; the helper above has populated
            // ModelState with signup-specific errors, and the login fields will be
            // ignored.
            return View("Login", model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}