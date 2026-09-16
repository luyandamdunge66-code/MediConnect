using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediConnect.Models
{
    [Table("Appointments")]
    public class Appointment
    {
        [Key]
        [Column("AppointmentID")]
        public int AppointmentId { get; set; }

        [Required]
        [Column("PatientID")]
        public int PatientId { get; set; }

        [Required]
        [Column("DoctorID")]
        public int DoctorId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan AppointmentTime { get; set; }

        public string? Reason { get; set; }

        public string Status { get; set; } = "Pending";

        [StringLength(255)]
        public string? Diagnosis { get; set; }

        public string? DoctorNotes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation relationships
        [ForeignKey("PatientId")]
        public virtual Patient? Patient { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor? Doctor { get; set; }

        // Review tied to this specific completed appointment
        public virtual Review? Review { get; set; }
    }
}