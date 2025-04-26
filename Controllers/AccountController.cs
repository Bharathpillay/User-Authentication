using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleSite.Data;
using SimpleSite.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;

namespace SimpleSite.Controllers
{
    public class AccountController : Controller
    {
        private readonly SimpleSiteDbContext _context;

        public AccountController(SimpleSiteDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password);

            if (user == null || !user.IsConfirmed || user.Status != 1)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AcceptInvitation(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Invalid token.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.ConfirmationToken == token && !u.IsConfirmed);

            if (user == null)
            {
                return NotFound("Invalid or expired invitation.");
            }

            var model = new SetPasswordViewModel
            {
                Token = token
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptInvitation(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.ConfirmationToken == model.Token && !u.IsConfirmed);

            if (user == null)
            {
                return NotFound("Invalid or expired invitation.");
            }

            user.Password = model.Password; 
            user.IsConfirmed = true;
            user.Status = 1;
            user.ConfirmationToken = null;

            await _context.SaveChangesAsync();

            TempData["Message"] = "Your account has been activated. Please log in.";
            return RedirectToAction("Login");
        }
    }
}