using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediConnect.Models
{
    [Table("Doctors")]
    public class Doctor
    {
        [Key]
        [Column("DoctorID")]
        public int DoctorId { get; set; }

        [Required]
        [Column("UserID")]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Specialization { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LicenseNumber { get; set; } = string.Empty;

        public string ApprovalStatus { get; set; } = "Pending";

        [StringLength(255)]
        public string? ProfilePicture { get; set; }

        // Connects to User details
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        // Collection of patient reviews for this doctor
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}