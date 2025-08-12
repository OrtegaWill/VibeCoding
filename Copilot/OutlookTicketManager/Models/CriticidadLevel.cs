using System.ComponentModel;

namespace OutlookTicketManager.Models
{
    /// <summary>
    /// Niveles de criticidad para los tickets
    /// </summary>
    public enum CriticidadLevel
    {
        [Description("Muy Baja")]
        MuyBaja = 0,
        
        [Description("Baja")]
        Baja = 1,
        
        [Description("Media")]
        Media = 2,
        
        [Description("Alta")]
        Alta = 3,
        
        [Description("Muy Alta")]
        MuyAlta = 4,
        
        [Description("Crítica")]
        Critica = 5
    }
}
