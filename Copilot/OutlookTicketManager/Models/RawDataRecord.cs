using CsvHelper.Configuration.Attributes;

namespace OutlookTicketManager.Models
{
    /// <summary>
    /// Modelo para mapear los datos del archivo RAWDATA CSV
    /// </summary>
    public class RawDataRecord
    {
        [Name("ID de la Incidencia")]
        public string? IncidentId { get; set; }

        [Name("ID de la Petición")]
        public string? PetitionId { get; set; }

        [Name("Detalle de la Descripción")]
        public string? DescriptionDetail { get; set; }

        [Name("Grupo Asignado")]
        public string? AssignedGroup { get; set; }

        [Name("Prioridad")]
        public string? Priority { get; set; }

        [Name("Estatus")]
        public string? Status { get; set; }

        [Name("Fecha Asignación")]
        public string? AssignmentDate { get; set; }

        [Name("Fecha Solución")]
        public string? SolutionDate { get; set; }

        [Name("Apellidos")]
        public string? LastName { get; set; }

        [Name("Nombre")]
        public string? FirstName { get; set; }

        [Name("Criticidad")]
        public string? Criticality { get; set; }

        [Name("Tipo Queja")]
        public string? ComplaintType { get; set; }

        [Name("Origen")]
        public string? Origin { get; set; }

        [Name("Categoría")]
        public string? Category { get; set; }

        [Name("Grupo Resolutor")]
        public string? ResolverGroup { get; set; }

        [Name("Historial")]
        public string? History { get; set; }

        [Name("Avance")]
        public string? Progress { get; set; }

        [Name("Visor / Aplicativo afectado")]
        public string? AffectedApplication { get; set; }

        [Name("Problema")]
        public string? Problem { get; set; }

        [Name("Detalle del Problema")]
        public string? ProblemDetail { get; set; }

        [Name("Quien atiende?")]
        public string? AssignedTo { get; set; }

        [Name("Tiempo de resolución")]
        public string? ResolutionTime { get; set; }

        [Name("Fecha ACK equipo de precargas")]
        public string? AckDate { get; set; }

        [Name("Solución Remedy")]
        public string? RemedySolution { get; set; }

        [Name("Precarga")]
        public string? Preload { get; set; }

        [Name("RFC o Solicitud de Cambio")]
        public string? ChangeRequest { get; set; }

        [Name("Causa Raíz")]
        public string? RootCause { get; set; }
    }
}
