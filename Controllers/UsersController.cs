using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleSite.Data;
using SimpleSite.Models;
using SimpleSite.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleSite.Controllers
{
<<<<<<< HEAD
=======
    //private Method to calling
>>>>>>> 54aece9 (sample update)

    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly SimpleSiteDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public UsersController(SimpleSiteDbContext context, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(int? editingUserId = null)
        {
            var users = await GetUserViewModelsAsync();

            ViewBag.EditingUserId = editingUserId;
            if (editingUserId.HasValue)
            {
                ViewBag.EditModel = await GetStudentEditModelAsync(editingUserId.Value);
                ViewBag.Role = (await _context.Users.FindAsync(editingUserId.Value))?.Role;
            }

            ViewBag.Departments = await GetDepartmentsAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return await ReturnIndexWithModelError(model);
            }

            var user = await _context.Users
                .Where(u => u.Role != "Admin")
                .Include(u => u.Student)
                .FirstOrDefaultAsync(u => u.Id == model.Id);

            if (user == null) return NotFound();

            if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.Id != model.Id))
            {
                ModelState.AddModelError("Email", "Email is already taken.");
                return await ReturnIndexWithModelError(model);
            }

            user.Name = model.Name;
            user.Email = model.Email;
            if (!string.IsNullOrEmpty(model.Password))
            {
                user.Password = model.Password;
            }

            if (user.Role == "Student")
            {
                if (user.Student == null && model.DepartmentId > 0)
                {
                    user.Student = new Student { UserId = user.Id };
                    _context.Students.Add(user.Student);
                }
                if (user.Student != null)
                {
                    user.Student.DepartmentId = model.DepartmentId;
                    user.Student.CGPA = model.CGPA;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> CreateStudent()
        {
            ViewBag.Departments = await GetDepartmentsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudent(StudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Departments = await GetDepartmentsAsync();
                return View(model);
            }

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email is already taken.");
                ViewBag.Departments = await GetDepartmentsAsync();
                return View(model);
            }

            var token = Guid.NewGuid().ToString();
            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Password = null,
                Role = "Student",
                ConfirmationToken = token,
                IsConfirmed = false,
                Status = 2
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var student = new Student
            {
                UserId = user.Id,
                DepartmentId = model.DepartmentId,
                CGPA = model.CGPA
            };
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            var invitationLink = Url.Action("AcceptInvitation", "Account", new { token }, Request.Scheme);
            if (!await SendInvitationEmailAsync(model.Name, model.Email, invitationLink, "Student"))
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                ModelState.AddModelError("", "Failed to send invitation email.");
                ViewBag.Departments = await GetDepartmentsAsync();
                return View(model);
            }

            TempData["Message"] = $"Invitation sent to {model.Email}.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult CreateStaff()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStaff(StaffViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email is already taken.");
                return View(model);
            }

            var token = Guid.NewGuid().ToString();
            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Password = null,
                Role = "Staff",
                ConfirmationToken = token,
                IsConfirmed = false,
                Status = 2
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var invitationLink = Url.Action("AcceptInvitation", "Account", new { token }, Request.Scheme);
            if (!await SendInvitationEmailAsync(model.Name, model.Email, invitationLink, "Staff"))
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                ModelState.AddModelError("", "Failed to send invitation email.");
                return View(model);
            }

            TempData["Message"] = $"Invitation sent to {model.Email}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users
                .Where(u => u.Role != "Admin")
                .Include(u => u.Student)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (user.Student != null)
                {
                    _context.Students.Remove(user.Student);
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"User {user.Name} deleted successfully.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Failed to delete user due to database constraints.";
            }

            return RedirectToAction(nameof(Index));
        }

<<<<<<< HEAD
//<<<<<<< HEAD
        
//=======
//        // PRIVATE METHODS
//>>>>>>> 012d05d (admin)
=======
        
>>>>>>> 54aece9 (sample update)

        private async Task<List<UserViewModel>> GetUserViewModelsAsync()
        {
            return await _context.Users
                .Where(u => u.Role != "Admin")
                .GroupJoin(_context.Students,
                    u => u.Id,
                    s => s.UserId,
                    (u, s) => new { u, s })
                .SelectMany(
                    x => x.s.DefaultIfEmpty(),
                    (x, s) => new
                    {
                        x.u.Id,
                        Name = x.u.Name ?? "Unknown",
                        Email = x.u.Email ?? "N/A",
                        Role = x.u.Role ?? "N/A",
                        x.u.Status,
                        DepartmentId = s != null ? s.DepartmentId : (int?)null,
                        CGPA = s != null ? s.CGPA : null
                    })
                .GroupJoin(_context.Departments,
                    x => x.DepartmentId,
                    d => d.Id,
                    (x, depts) => new { x, depts })
                .SelectMany(
                    x => x.depts.DefaultIfEmpty(),
                    (x, dept) => new UserViewModel
                    {
                        Id = x.x.Id,
                        Name = x.x.Name,
                        Email = x.x.Email,
                        Role = x.x.Role,
                        Status = x.x.Status,
                        DepartmentName = dept != null ? (dept.Name ?? "N/A") : "N/A",
                        CGPA = x.x.CGPA
                    })
                .ToListAsync();
        }

        private async Task<StudentViewModel?> GetStudentEditModelAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Student)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            return new StudentViewModel
            {
                Id = user.Id,
                Name = user.Name ?? "",
                Email = user.Email ?? "",
                Password = null,
                DepartmentId = user.Student?.DepartmentId ?? 0,
                CGPA = user.Student?.CGPA
            };
        }

        private async Task<List<Department>> GetDepartmentsAsync()
        {
            return await _context.Departments.ToListAsync();
        }

        private async Task<bool> SendInvitationEmailAsync(string name, string email, string link, string role)
        {
            try
            {
                var emailBody = $@"
                    <h2>Welcome, {name}!</h2>
                    <p>You have been invited to join SimpleSite as a {role.ToLower()}.</p>
                    <p>Please click the link below to set your password and activate your account:</p>
                    <p><a href=""{link}"">Set Your Password</a></p>
                    <p>Or copy and paste this URL: {link}</p>
                    <p>This link is valid for 24 hours.</p>
                    <p>Best regards,<br>SimpleSite Team</p>";
                await _emailService.SendEmailAsync(email, $"SimpleSite {role} Invitation", emailBody);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<IActionResult> ReturnIndexWithModelError(StudentViewModel model)
        {
            ViewBag.EditingUserId = model.Id;
            ViewBag.EditModel = model;
            ViewBag.Role = (await _context.Users.FindAsync(model.Id))?.Role;
            ViewBag.Departments = await GetDepartmentsAsync();
            var users = await GetUserViewModelsAsync();
            return View("Index", users);
        }
    }
}
