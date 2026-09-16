using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediConnect.Models
{
    [Table("Patients")]
    public class Patient
    {
        [Key]
        [Column("PatientID")]
        public int PatientId { get; set; }

        [Required]
        [Column("UserID")]
        public int UserId { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        // Medical EMR Details
        [StringLength(10)]
        public string? BloodGroup { get; set; } // e.g. A+, B+, O+, AB-

        [StringLength(255)]
        public string? Allergies { get; set; } // e.g. Penicillin, Peanuts, Latex

        [StringLength(100)]
        public string? EmergencyContact { get; set; } // Name & Phone number

        // Connects to the User account details (Name, Email, etc.)
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}