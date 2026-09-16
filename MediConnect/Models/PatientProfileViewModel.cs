using System.ComponentModel.DataAnnotations;

namespace MediConnect.Models
{
    public class PatientProfileViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [Display(Name = "Blood Group")]
        public string? BloodGroup { get; set; } // e.g. A+, B+, O+, AB-

        [Display(Name = "Known Allergies (e.g. Penicillin, Peanuts, Latex)")]
        public string? Allergies { get; set; }

        [Display(Name = "Emergency Contact (Name & Phone)")]
        public string? EmergencyContact { get; set; }
    }
}