using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace ReactApp1.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IOptionsMonitor<CookieAuthenticationOptions> _cookieOptions;

        public AccountController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IOptionsMonitor<CookieAuthenticationOptions> cookieOptions)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _cookieOptions = cookieOptions;
        }

        public sealed class RegisterRequest { public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
        public sealed class LoginRequest { public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; public bool RememberMe { get; set; } }

        [HttpPost("register")]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { title = "Invalid input", detail = "Email and password are required." });

            var user = new IdentityUser { UserName = request.Email, Email = request.Email, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "User");
            return Ok(new { message = "Registered" });
        }

        [HttpPost("login")] 
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null) return Unauthorized();

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!result.Succeeded) return Unauthorized();

            await _signInManager.SignInAsync(user, isPersistent: request.RememberMe);
            return Ok(new { message = "Logged in" });
        }

        [HttpPost("logout")]
        [Authorize]
        [IgnoreAntiforgeryToken] // allow SPA to log out even if no XSRF header is present
        public async Task<IActionResult> Logout()
        {
            // Sign out scheme and identity
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            // Delete the auth cookie using the actual configured name and matching options
            var opts = _cookieOptions.Get(IdentityConstants.ApplicationScheme);
            var cookieName = opts.Cookie.Name;

            var deleteOptions = new CookieOptions
            {
                Path = opts.Cookie.Path ?? "/",
                Domain = opts.Cookie.Domain, // usually null for localhost
                HttpOnly = opts.Cookie.HttpOnly, // FIX: remove '?? true' since HttpOnly is non-nullable bool
                SameSite = opts.Cookie.SameSite,
                Secure = opts.Cookie.SecurePolicy switch
                {
                    CookieSecurePolicy.Always => true,
                    CookieSecurePolicy.SameAsRequest => HttpContext.Request.IsHttps,
                    _ => false
                }
            };

            if (!string.IsNullOrEmpty(cookieName))
            {
                Response.Cookies.Delete(cookieName!, deleteOptions);
            }
            // Clean up any legacy default cookie
            if (cookieName != ".AspNetCore.Identity.Application")
            {
                Response.Cookies.Delete(".AspNetCore.Identity.Application", new CookieOptions { Path = "/" });
            }

            // As a belt-and-suspenders option in dev, ask browser to clear cookies for this origin
            Response.Headers["Clear-Site-Data"] = "\"cookies\"";

            return Ok(new { message = "Logged out" });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new { user = new { user.Id, user.Email, user.UserName }, roles });
        }
    }
}