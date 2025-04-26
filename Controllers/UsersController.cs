using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleSite.Data;
using SimpleSite.Models;
using SimpleSite.Services;
using System;
using System.Threading.Tasks;

namespace SimpleSite.Controllers
{
    //Admin 
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
            var users = await _context.Users
                .Where(u => u.Role != "Admin")
                .GroupJoin(_context.Students,
                    user => user.Id,
                    student => student.UserId,
                    (user, students) => new { user, students })
                .SelectMany(
                    x => x.students.DefaultIfEmpty(),
                    (x, student) => new
                    {
                        x.user.Id,
                        Name = x.user.Name ?? "Unknown",
                        Email = x.user.Email ?? "N/A",
                        Role = x.user.Role ?? "N/A",
                        x.user.Status,
                        DepartmentId = student != null ? student.DepartmentId : (int?)null,
                        CGPA = student != null ? student.CGPA : null
                    })
                .GroupJoin(_context.Departments,
                    x => x.DepartmentId,
                    dept => dept.Id,
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

            ViewBag.EditingUserId = editingUserId;
            if (editingUserId.HasValue)
            {
                var user = await _context.Users
                    .Where(u => u.Role != "Admin")
                    .Include(u => u.Student)
                    .FirstOrDefaultAsync(u => u.Id == editingUserId);

                if (user != null)
                {
                    ViewBag.EditModel = new StudentViewModel
                    {
                        Id = user.Id,
                        Name = user.Name ?? "",
                        Email = user.Email ?? "",
                        Password = null,
                        DepartmentId = user.Student?.DepartmentId ?? 0,
                        CGPA = user.Student?.CGPA
                    };
                    ViewBag.Role = user.Role;
                }
            }

            ViewBag.Departments = await _context.Departments.ToListAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var users = await _context.Users
                    .Where(u => u.Role != "Admin")
                    .GroupJoin(_context.Students,
                        user => user.Id,
                        student => student.UserId,
                        (user, students) => new { user, students })
                    .SelectMany(
                        x => x.students.DefaultIfEmpty(),
                        (x, student) => new
                        {
                            x.user.Id,
                            Name = x.user.Name ?? "Unknown",
                            Email = x.user.Email ?? "N/A",
                            Role = x.user.Role ?? "N/A",
                            x.user.Status,
                            DepartmentId = student != null ? student.DepartmentId : (int?)null,
                            CGPA = student != null ? student.CGPA : null
                        })
                    .GroupJoin(_context.Departments,
                        x => x.DepartmentId,
                        dept => dept.Id,
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

                ViewBag.EditingUserId = model.Id;
                ViewBag.EditModel = model;
                ViewBag.Role = (await _context.Users.FindAsync(model.Id))?.Role;
                ViewBag.Departments = await _context.Departments.ToListAsync();
                return View("Index", users);
            }

            var user = await _context.Users
                .Where(u => u.Role != "Admin")
                .Include(u => u.Student)
                .FirstOrDefaultAsync(u => u.Id == model.Id);

            if (user == null) return NotFound();

            if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.Id != model.Id))
            {
                ModelState.AddModelError("Email", "Email is already taken.");
                ViewBag.EditingUserId = model.Id;
                ViewBag.EditModel = model;
                ViewBag.Role = user.Role;
                ViewBag.Departments = await _context.Departments.ToListAsync();
                var users = await _context.Users
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
                        dept => dept.Id,
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
                return View("Index", users);
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
            ViewBag.Departments = await _context.Departments.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudent(StudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Departments = await _context.Departments.ToListAsync();
                return View(model);
            }

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email is already taken.");
                ViewBag.Departments = await _context.Departments.ToListAsync();
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

            try
            {
                var invitationLink = Url.Action("AcceptInvitation", "Account", new { token }, Request.Scheme);
                if (string.IsNullOrEmpty(invitationLink))
                {
                    throw new Exception("Failed to generate invitation link.");
                }
                var emailBody = $@"
                    <h2>Welcome, {model.Name}!</h2>
                    <p>You have been invited to join SimpleSite as a student.</p>
                    <p>Please click the link below to set your password and activate your account:</p>
                    <p><a href=""{invitationLink}"">Set Your Password</a></p>
                    <p>Or copy and paste this URL: {invitationLink}</p>
                    <p>This link is valid for 24 hours.</p>
                    <p>Best regards,<br>SimpleSite Team</p>";
                await _emailService.SendEmailAsync(model.Email, "SimpleSite Student Invitation", emailBody);
                TempData["Message"] = $"Invitation sent to {model.Email}. They need to accept it to activate their account.";
            }
            catch (Exception ex)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                ModelState.AddModelError("", $"Failed to send invitation: {ex.Message}");
                ViewBag.Departments = await _context.Departments.ToListAsync();
                return View(model);
            }

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
            if (!ModelState.IsValid)
            {
                return View(model);
            }

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

            try
            {
                var invitationLink = Url.Action("AcceptInvitation", "Account", new { token }, Request.Scheme);
                if (string.IsNullOrEmpty(invitationLink))
                {
                    throw new Exception("Failed to generate invitation link.");
                }
                var emailBody = $@"
                    <h2>Welcome, {model.Name}!</h2>
                    <p>You have been invited to join SimpleSite as a staff member.</p>
                    <p>Please click the link below to set your password and activate your account:</p>
                    <p><a href=""{invitationLink}"">Set Your Password</a></p>
                    <p>Or copy and paste this URL: {invitationLink}</p>
                    <p>This link is valid for 24 hours.</p>
                    <p>Best regards,<br>SimpleSite Team</p>";
                await _emailService.SendEmailAsync(model.Email, "SimpleSite Staff Invitation", emailBody);
                TempData["Message"] = $"Invitation sent to {model.Email}. They need to accept it to activate their account.";
            }
            catch (Exception ex)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                ModelState.AddModelError("", $"Failed to send invitation: {ex.Message}");
                return View(model);
            }

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
            catch (DbUpdateException ex)
            {
                TempData["Error"] = "Failed to delete user due to database constraints.";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}