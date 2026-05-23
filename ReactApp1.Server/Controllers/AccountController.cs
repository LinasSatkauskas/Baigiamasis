using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using ReactApp1.Server.Services.Email;
using System.Text;
using System.Net;
using System.Net.Mail;

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
        private readonly IEmailSenderService _emailSender;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly ReactApp1.Server.Data.AppDbContext _db;

        public AccountController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IOptionsMonitor<CookieAuthenticationOptions> cookieOptions,
            IEmailSenderService emailSender,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            ReactApp1.Server.Data.AppDbContext db)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _cookieOptions = cookieOptions;
            _emailSender = emailSender;
            _configuration = configuration;
            _environment = environment;
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public sealed class RegisterRequest { public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
        public sealed class LoginRequest { public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; public bool RememberMe { get; set; } }
        public sealed class ForgotPasswordRequest { public string Email { get; set; } = string.Empty; }
        public sealed class ResetPasswordRequest { public string Email { get; set; } = string.Empty; public string Token { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
        public sealed class UserListItem
        {
            public string Id { get; set; } = string.Empty;
            public string? Email { get; set; }
            public string? UserName { get; set; }
            public string[] Roles { get; set; } = [];
            public bool IsCurrentUser { get; set; }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var email = request.Email?.Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { title = "Invalid input", detail = "Email and password are required." });

            var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
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
            var email = request.Email?.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                return Unauthorized(new { message = "You've typed in the wrong password or email." });
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) return Unauthorized(new { message = "You've typed in the wrong password or email." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!result.Succeeded) return Unauthorized(new { message = "You've typed in the wrong password or email." });

            await _signInManager.SignInAsync(user, isPersistent: request.RememberMe);
            return Ok(new { message = "Logged in" });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var email = request.Email?.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { title = "Invalid input", detail = "Email is required." });
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                return Ok(new { message = "Jei toks el. paštas egzistuoja, atstatymo nuoroda buvo išsiųsta." });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var baseUrl = GetFrontendBaseUrl();
            var resetUrl = $"{baseUrl.TrimEnd('/')}/reset-password?email={WebUtility.UrlEncode(email)}&token={WebUtility.UrlEncode(encodedToken)}";

            var htmlBody = $"""
                <html>
                  <body style=\"font-family:Arial,sans-serif;line-height:1.5;color:#1f2937;\">
                    <h2 style=\"margin:0 0 16px;\">Slaptažodžio atstatymas</h2>
                    <p>Gavome užklausą atstatyti slaptažodį jūsų paskyrai.</p>
                    <p>Paspauskite šią nuorodą, kad nustatytumėte naują slaptažodį:</p>
                    <p><a href=\"{resetUrl}\">Atstatyti slaptažodį</a></p>
                    <p>Jei jūs neprašėte atstatymo, šį laišką galite ignoruoti.</p>
                  </body>
                </html>
                """;

            var textBody = $"Gavome užklausą atstatyti slaptažodį. Atstatymo nuoroda: {resetUrl}";

            try
            {
                await _emailSender.SendAsync(email, "Slaptažodžio atstatymas", htmlBody, textBody);
            }
            catch (SmtpException ex)
            {
                // If in development, fallback to FileEmailSender so dev testers still receive the link
                if (_environment.IsDevelopment())
                {
                    try
                    {
                        // Resolve a FileEmailSender and write the email locally
                        var fileSender = HttpContext.RequestServices.GetService(typeof(IEmailSenderService)) as IEmailSenderService;
                        if (fileSender is not null)
                        {
                            await fileSender.SendAsync(email, "Slaptažodžio atstatymas (fallback)", htmlBody, textBody);
                        }
                    }
                    catch
                    {
                        // ignore fallback failures
                    }

                    return Ok(new { message = "Vystymo režime: atstatymo nuoroda sugeneruota ir įrašyta į DevEmails (SMTP klaida).", detail = ex.Message });
                }

                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    message = "Nepavyko išsiųsti el. laiško per SMTP.",
                    detail = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    message = "El. pašto siuntimas nėra sukonfigūruotas.",
                    detail = ex.Message
                });
            }

            if (_environment.IsDevelopment())
            {
                return Ok(new
                {
                    message = "Vystymo režime atstatymo nuoroda sugeneruota.",
                    resetUrl
                });
            }

            return Ok(new { message = "Jei toks el. paštas egzistuoja, atstatymo nuoroda buvo išsiųsta." });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var email = request.Email?.Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { title = "Invalid input", detail = "Email, token and password are required." });
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                return BadRequest(new { title = "Reset failed", detail = "Paskyra nerasta." });
            }

            string token;
            try
            {
                token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            }
            catch
            {
                return BadRequest(new { title = "Reset failed", detail = "Neteisinga atstatymo nuoroda." });
            }

            var result = await _userManager.ResetPasswordAsync(user, token, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "Slaptažodis sėkmingai atnaujintas." });
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

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var users = await _userManager.Users
                .OrderBy(u => u.Email ?? u.UserName)
                .ToListAsync();

            var result = new List<UserListItem>();
            foreach (var identityUser in users)
            {
                var roles = await _userManager.GetRolesAsync(identityUser);
                result.Add(new UserListItem
                {
                    Id = identityUser.Id,
                    Email = identityUser.Email,
                    UserName = identityUser.UserName,
                    Roles = roles.ToArray(),
                    IsCurrentUser = currentUser?.Id == identityUser.Id
                });
            }

            return Ok(result);
        }

        [HttpDelete("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new { title = "Invalid input", detail = "User id is required." });
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == id)
            {
                return BadRequest(new { title = "Delete blocked", detail = "Negalite ištrinti savo paskyros." });
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                return NotFound(new { title = "Not found", detail = "Vartotojas nerastas." });
            }

            var email = user.Email?.Trim();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Remove related data for this deleted user: comments by their email.
            try
            {
                if (!string.IsNullOrWhiteSpace(email))
                {
                    var key = email.Trim().ToLowerInvariant();
                    var comments = await _db.Comments.Where(c => c.Email.ToLower() == key).ToListAsync();
                    if (comments.Count > 0)
                    {
                        _db.Comments.RemoveRange(comments);
                        await _db.SaveChangesAsync();
                    }
                }
            }
            catch
            {
                // Swallow errors deleting related data to avoid failing the whole operation.
            }

            return Ok(new { message = "User deleted" });
        }

        private string GetFrontendBaseUrl()
        {
            var configured = _configuration["Frontend:BaseUrl"];
            if (!string.IsNullOrWhiteSpace(configured))
            {
                return configured;
            }

            return $"{Request.Scheme}://{Request.Host}";
        }
    }
}