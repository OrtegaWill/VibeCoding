using CsvHelper.Configuration.Attributes;

namespace OutlookTicketManager.Models
{
    /// <summary>
    /// Modelo para mapear los datos del archivo CSV de tickets
    /// </summary>
    public class CsvTicketRecord
    {
        [Name("ID de la incidencia*+")]
        public string? IncidentId { get; set; }

        [Name("ID de petición de servicio")]
        public string? ServiceRequestId { get; set; }

        [Name("Descripción Detallada")]
        public string? DetailedDescription { get; set; }

        [Name("Grupo asignado*+")]
        public string? AssignedGroup { get; set; }

        [Name("Prioridad*")]
        public string? Priority { get; set; }

        [Name("Estado*")]
        public string? Status { get; set; }

        [Name("Fecha de notificación+")]
        public string? NotificationDate { get; set; }

        [Name("Fecha de solucion")]
        public string? SolutionDate { get; set; }

        [Name("Apellidos+")]
        public string? LastName { get; set; }

        [Name("Nombre+")]
        public string? FirstName { get; set; }

        [Name("Solución")]
        public string? Solution { get; set; }

        [Name("Usuario asignado+")]
        public string? AssignedUser { get; set; }

        [Name("Nombre del producto+")]
        public string? ProductName { get; set; }
    }
}
