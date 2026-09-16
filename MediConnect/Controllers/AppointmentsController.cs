using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MediConnect.Data;
using MediConnect.Models;

namespace MediConnect.Controllers
{
    [Authorize] // Must be logged in
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AppointmentsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // ==========================================
        // 1. PATIENT: BOOK AN APPOINTMENT
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Book()
        {
            var model = new BookAppointmentViewModel();
            await PopulateDoctorsList(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(BookAppointmentViewModel model)
        {
            if (model.AppointmentDate.Date < DateTime.Today)
            {
                ModelState.AddModelError("AppointmentDate", "You cannot book an appointment for a past date. Please select today or a future date.");
            }

            var isSlotTaken = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == model.DoctorId &&
                a.AppointmentDate.Date == model.AppointmentDate.Date &&
                a.AppointmentTime == model.AppointmentTime &&
                a.Status != "Cancelled");

            if (isSlotTaken)
            {
                ModelState.AddModelError("AppointmentTime", "This doctor is already booked for this date and time slot. Please select another time or date.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDoctorsList(model);
                return View(model);
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            if (patient == null)
            {
                patient = new Patient { UserId = userId };
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
            }

            var appointment = new Appointment
            {
                PatientId = patient.PatientId,
                DoctorId = model.DoctorId,
                AppointmentDate = model.AppointmentDate,
                AppointmentTime = model.AppointmentTime,
                Reason = model.Reason,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your appointment has been booked! Waiting for doctor confirmation.";
            return RedirectToAction("PatientAppointments");
        }

        // ==========================================
        // 2. PATIENT: VIEW MY APPOINTMENTS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> PatientAppointments()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return View(new List<Appointment>());
            }

            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d!.User)
                .Include(a => a.Review)
                .Where(a => a.PatientId == patient.PatientId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        // ==========================================
        // 3. PATIENT: MEDICAL HEALTH PROFILE
        // ==========================================

        [Authorize(Roles = "Patient")]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users.FindAsync(userId);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);

            var model = new PatientProfileViewModel
            {
                FullName = user?.FullName ?? "",
                Email = user?.Email ?? "",
                Phone = patient?.Phone,
                BloodGroup = patient?.BloodGroup,
                Allergies = patient?.Allergies,
                EmergencyContact = patient?.EmergencyContact
            };

            return View(model);
        }

        [Authorize(Roles = "Patient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(PatientProfileViewModel model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                patient = new Patient { UserId = userId };
                _context.Patients.Add(patient);
            }

            patient.Phone = model.Phone;
            patient.BloodGroup = model.BloodGroup;
            patient.Allergies = model.Allergies;
            patient.EmergencyContact = model.EmergencyContact;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your health profile has been updated successfully!";
            return RedirectToAction("Profile");
        }

        // ==========================================
        // 4. PATIENT: LEAVE VERIFIED DOCTOR REVIEW
        // ==========================================

        [Authorize(Roles = "Patient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveReview(int appointmentId, int rating, string? comment)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return RedirectToAction("PatientAppointments");
            }

            var appointment = await _context.Appointments
                .Include(a => a.Review)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.PatientId == patient.PatientId);

            if (appointment == null || appointment.Status != "Completed")
            {
                TempData["SuccessMessage"] = "You can only review completed consultations.";
                return RedirectToAction("PatientAppointments");
            }

            if (appointment.Review != null)
            {
                TempData["SuccessMessage"] = "You have already reviewed this consultation.";
                return RedirectToAction("PatientAppointments");
            }

            rating = Math.Clamp(rating, 1, 5);

            var review = new Review
            {
                AppointmentId = appointmentId,
                DoctorId = appointment.DoctorId,
                PatientId = patient.PatientId,
                Rating = rating,
                Comment = comment?.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thank you! Your doctor rating and review have been submitted.";
            return RedirectToAction("PatientAppointments");
        }

        // ==========================================
        // 5. DOCTOR: VIEW SCHEDULE & QUEUE + PATIENT REVIEWS
        // ==========================================

        [Authorize(Roles = "Doctor,Admin")]
        [HttpGet]
        public async Task<IActionResult> DoctorSchedule()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Load Doctor with User profile and Reviews (including Patient info)
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Reviews)
                    .ThenInclude(r => r.Patient)
                        .ThenInclude(p => p!.User)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            ViewBag.CurrentDoctor = doctor;
            ViewBag.DoctorReviews = doctor?.Reviews.OrderByDescending(r => r.CreatedAt).ToList() ?? new List<Review>();

            if (doctor == null)
            {
                return View(new List<Appointment>());
            }

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p!.User)
                .Where(a => a.DoctorId == doctor.DoctorId)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();

            return View(appointments);
        }

        // ==========================================
        // 6. DOCTOR: COMPLETE CONSULTATION & WRITE PRESCRIPTION
        // ==========================================

        [Authorize(Roles = "Doctor,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteConsultation(int appointmentId, string diagnosis, string doctorNotes)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment != null)
            {
                appointment.Status = "Completed";
                appointment.Diagnosis = string.IsNullOrWhiteSpace(diagnosis) ? "General Consultation" : diagnosis.Trim();
                appointment.DoctorNotes = doctorNotes?.Trim();

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Consultation completed! Medical notes and prescription recorded.";
            }

            return RedirectToAction("DoctorSchedule");
        }

        // ==========================================
        // 7. DOCTOR: UPLOAD PROFILE PHOTO
        // ==========================================

        [Authorize(Roles = "Doctor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPhoto(IFormFile? photoFile)
        {
            if (photoFile == null || photoFile.Length == 0)
            {
                TempData["SuccessMessage"] = "Please select an image file to upload.";
                return RedirectToAction("DoctorSchedule");
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor != null)
            {
                var extension = Path.GetExtension(photoFile.FileName).ToLower();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                if (!allowedExtensions.Contains(extension))
                {
                    TempData["SuccessMessage"] = "Invalid image type. Please upload a JPG, PNG, or WEBP image.";
                    return RedirectToAction("DoctorSchedule");
                }

                var fileName = $"doctor_{doctor.DoctorId}_{Guid.NewGuid().ToString().Substring(0, 8)}{extension}";
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "doctors");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photoFile.CopyToAsync(stream);
                }

                doctor.ProfilePicture = fileName;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Profile picture updated successfully!";
            }

            return RedirectToAction("DoctorSchedule");
        }

        // ==========================================
        // 8. DOCTOR: UPDATE APPOINTMENT STATUS (Accept/Cancel)
        // ==========================================

        [Authorize(Roles = "Doctor,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int appointmentId, string status)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment != null)
            {
                appointment.Status = status;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Appointment marked as {status}.";
            }

            return RedirectToAction("DoctorSchedule");
        }

        // Helper method
        private async Task PopulateDoctorsList(BookAppointmentViewModel model)
        {
            var approvedDoctors = await _context.Doctors
                .Include(d => d.User)
                .Where(d => d.ApprovalStatus == "Approved")
                .ToListAsync();

            model.AvailableDoctors = approvedDoctors.Select(d => new SelectListItem
            {
                Value = d.DoctorId.ToString(),
                Text = $"Dr. {d.User?.FullName} ({d.Specialization})"
            }).ToList();
        }
    }
}