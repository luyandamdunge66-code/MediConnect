using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediConnect.Models
{
    [Table("Reviews")]
    public class Review
    {
        [Key]
        [Column("ReviewID")]
        public int ReviewId { get; set; }

        [Required]
        [Column("AppointmentID")]
        public int AppointmentId { get; set; }

        [Required]
        [Column("DoctorID")]
        public int DoctorId { get; set; }

        [Required]
        [Column("PatientID")]
        public int PatientId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("AppointmentId")]
        public virtual Appointment? Appointment { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor? Doctor { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }
    }
}