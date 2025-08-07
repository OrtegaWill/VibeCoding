namespace OutlookTicketManager.Models
{
    /// <summary>
    /// Clase que representa las estadísticas del dashboard
    /// </summary>
    public class DashboardStats
    {
        public int TotalTickets { get; set; }
        public int BacklogTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int InReviewTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int BlockedTickets { get; set; }
        
        public int CriticalPriorityTickets { get; set; }
        public int HighPriorityTickets { get; set; }
        public int MediumPriorityTickets { get; set; }
        public int LowPriorityTickets { get; set; }
        
        public int UnassignedTickets { get; set; }
        public DateTime? LastEmailImport { get; set; }
        public DateTime? LastImportDate { get; set; } // Alias para compatibilidad
        
        public double AverageResolutionTimeHours { get; set; }
        public int TodayCreatedTickets { get; set; }
        public int TodayResolvedTickets { get; set; }
        
        /// <summary>
        /// Porcentaje de resolución (tickets resueltos / total tickets)
        /// </summary>
        public double ResolutionPercentage => TotalTickets > 0 ? (double)ResolvedTickets / TotalTickets * 100 : 0;
        
        /// <summary>
        /// Porcentaje de tickets críticos y altos
        /// </summary>
        public double HighPriorityPercentage => TotalTickets > 0 ? (double)(CriticalPriorityTickets + HighPriorityTickets) / TotalTickets * 100 : 0;
    }
}
