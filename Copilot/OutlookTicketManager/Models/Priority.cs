using System.ComponentModel;

namespace OutlookTicketManager.Models
{
    /// <summary>
    /// Niveles de prioridad para los tickets
    /// </summary>
    public enum Priority
    {
        [Description("Baja")]
        Low = 0,
        
        [Description("Media")]
        Medium = 1,
        
        [Description("Alta")]
        High = 2,
        
        [Description("Crítica")]
        Critical = 3
    }
}
