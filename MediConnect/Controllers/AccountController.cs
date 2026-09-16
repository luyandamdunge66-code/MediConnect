using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MediConnect.Data;
using MediConnect.Models;

namespace MediConnect.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. REGISTRATION
        // ==========================================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
                return View(model);
            }

            // Doctor-specific validation
            if (model.Role == "Doctor")
            {
                if (string.IsNullOrWhiteSpace(model.Specialization))
                {
                    ModelState.AddModelError("Specialization", "Please select your medical specialization.");
                    return View(model);
                }

                if (model.Specialization == "Other" && string.IsNullOrWhiteSpace(model.OtherSpecialization))
                {
                    ModelState.AddModelError("OtherSpecialization", "Please specify your specialization in the text box.");
                    return View(model);
                }

                if (string.IsNullOrWhiteSpace(model.LicenseNumber))
                {
                    ModelState.AddModelError("LicenseNumber", "Medical license number is required for doctor registration.");
                    return View(model);
                }
            }

            // Create user record (Doctors are 'Pending', Patients are 'Active')
            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = model.Password,
                Role = model.Role,
                Status = model.Role == "Doctor" ? "Pending" : "Active",
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create Doctor or Patient profile
            if (model.Role == "Doctor")
            {
                // If they chose "Other", use what they typed in the text box!
                var finalSpecialization = (model.Specialization == "Other" && !string.IsNullOrWhiteSpace(model.OtherSpecialization))
                    ? model.OtherSpecialization.Trim()
                    : model.Specialization!;

                var doctor = new Doctor
                {
                    UserId = user.UserId,
                    Specialization = finalSpecialization,
                    LicenseNumber = model.LicenseNumber!,
                    ApprovalStatus = "Pending"
                };
                _context.Doctors.Add(doctor);
            }
            else
            {
                var patient = new Patient
                {
                    UserId = user.UserId
                };
                _context.Patients.Add(patient);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = model.Role == "Doctor"
                ? "Doctor registration submitted! Your account is pending administrator approval."
                : "Registration successful! You can now sign in.";

            return RedirectToAction("Login");
        }

        // ==========================================
        // 2. LOGIN
        // ==========================================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            // Check if user exists and password matches
            if (user == null || user.Password != model.Password)
            {
                ModelState.AddModelError(string.Empty, "Invalid email address or password.");
                return View(model);
            }

            // Check if Doctor is still Pending
            if (user.Role == "Doctor" && user.Status == "Pending")
            {
                ModelState.AddModelError(string.Empty, "Your doctor account is currently pending administrator approval. Please wait for an administrator to verify your credentials.");
                return View(model);
            }

            // Check if account was rejected
            if (user.Status == "Rejected")
            {
                ModelState.AddModelError(string.Empty, "Your account has been rejected. Please contact clinic administration.");
                return View(model);
            }

            // Build Claims (User Identity)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTime.UtcNow.AddDays(7)
            };

            // Sign in the user
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            TempData["SuccessMessage"] = $"Welcome back, {user.FullName}!";

            // Smart Routing: Sends each user straight to their workspace
            if (user.Role == "Admin")
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else if (user.Role == "Doctor")
            {
                return RedirectToAction("DoctorSchedule", "Appointments");
            }
            else
            {
                return RedirectToAction("PatientAppointments", "Appointments");
            }
        }

        // ==========================================
        // 3. LOGOUT
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }
    }
}