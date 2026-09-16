using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediConnect.Data;
using MediConnect.Models;

namespace MediConnect.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. ADMIN DASHBOARD & LIVE ANALYTICS
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var model = new AdminDashboardViewModel
            {
                TotalPatients = await _context.Users.CountAsync(u => u.Role == "Patient"),
                TotalDoctors = await _context.Doctors.CountAsync(d => d.ApprovalStatus == "Approved"),
                PendingApprovals = await _context.Doctors.CountAsync(d => d.ApprovalStatus == "Pending"),
                TotalAppointments = await _context.Appointments.CountAsync(),

                // Fetch the 10 most recent appointments across the clinic
                RecentAppointments = await _context.Appointments
                    .Include(a => a.Patient)
                        .ThenInclude(p => p!.User)
                    .Include(a => a.Doctor)
                        .ThenInclude(d => d!.User)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(10)
                    .ToListAsync()
            };

            return View(model);
        }

        // ==========================================
        // 2. DOCTOR APPROVALS QUEUE
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> DoctorApprovals()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .OrderByDescending(d => d.ApprovalStatus == "Pending")
                .ToListAsync();

            return View(doctors);
        }

        // POST: /Admin/ApproveDoctor/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveDoctor(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DoctorId == id);

            if (doctor != null)
            {
                doctor.ApprovalStatus = "Approved";
                if (doctor.User != null)
                {
                    doctor.User.Status = "Active";
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Dr. {doctor.User?.FullName} has been approved successfully!";
            }

            return RedirectToAction("DoctorApprovals");
        }

        // POST: /Admin/RejectDoctor/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectDoctor(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DoctorId == id);

            if (doctor != null)
            {
                doctor.ApprovalStatus = "Rejected";
                if (doctor.User != null)
                {
                    doctor.User.Status = "Rejected";
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Dr. {doctor.User?.FullName}'s application was rejected.";
            }

            return RedirectToAction("DoctorApprovals");
        }
    // POST: /Admin/DeleteDoctor/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DoctorId == id);

            if (doctor != null)
            {
                var doctorName = doctor.User?.FullName;

                // Deleting the user account will also cascade and remove the doctor record in MySQL
                if (doctor.User != null)
                {
                    _context.Users.Remove(doctor.User);
                }
                else
                {
                    _context.Doctors.Remove(doctor);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Dr. {doctorName} has been completely removed from the system.";
            }

            return RedirectToAction("DoctorApprovals");
        } } }
    