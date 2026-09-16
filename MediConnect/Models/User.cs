using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediConnect.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("UserID")]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Patient"; // 'Patient', 'Doctor', 'Admin'

        [Required]
        public string Status { get; set; } = "Active"; // 'Active', 'Pending', 'Rejected'

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}