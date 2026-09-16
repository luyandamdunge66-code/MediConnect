using System.Collections.Generic;

namespace MediConnect.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public int PendingApprovals { get; set; }
        public int TotalAppointments { get; set; }
        public List<Appointment> RecentAppointments { get; set; } = new List<Appointment>();
    }
}