using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediConnect.Models
{
    public class BookAppointmentViewModel
    {
        [Required(ErrorMessage = "Please select a doctor.")]
        [Display(Name = "Select Doctor")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Please choose an appointment date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Appointment Date")]
        public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Please choose an appointment time.")]
        [DataType(DataType.Time)]
        [Display(Name = "Appointment Time")]
        public TimeSpan AppointmentTime { get; set; } = new TimeSpan(9, 0, 0); // Default 09:00 AM

        [Required(ErrorMessage = "Please provide a brief reason for your visit.")]
        [StringLength(500)]
        [Display(Name = "Reason for Visit / Symptoms")]
        public string Reason { get; set; } = string.Empty;

        // Populates the dropdown list with only Approved doctors!
        public List<SelectListItem> AvailableDoctors { get; set; } = new List<SelectListItem>();
    }
}