using System.ComponentModel;

namespace OutlookTicketManager.Models
{
    /// <summary>
    /// Estados posibles de un ticket en el sistema
    /// </summary>
    public enum TicketStatus
    {
        [Description("Backlog")]
        Backlog = 0,
        
        [Description("En Progreso")]
        InProgress = 1,
        
        [Description("En Revisión")]
        InReview = 2,
        
        [Description("Resuelto")]
        Resolved = 3,
        
        [Description("Bloqueado")]
        Blocked = 4
    }
}
