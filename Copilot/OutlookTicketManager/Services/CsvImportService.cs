using OutlookTicketManager.Data;
using OutlookTicketManager.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace OutlookTicketManager.Services
{
    public class CsvImportService
    {
        private readonly TicketDbContext _context;
        private readonly ILogger<CsvImportService> _logger;
        private readonly NotificationService _notificationService;

        public CsvImportService(
            TicketDbContext context, 
            ILogger<CsvImportService> logger,
            NotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<int> ImportTicketsFromCsvAsync(string filePath)
        {
            _logger.LogInformation("Iniciando importación desde archivo CSV: {FilePath}", filePath);

            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogError("Archivo CSV no encontrado: {FilePath}", filePath);
                    await _notificationService.SendNotificationAsync("Error de Importación", "Archivo CSV no encontrado");
                    return 0;
                }

                var ticketsImported = 0;
                var lines = await File.ReadAllLinesAsync(filePath);

                if (lines.Length <= 1)
                {
                    _logger.LogWarning("El archivo CSV está vacío o solo contiene headers");
                    await _notificationService.SendNotificationAsync("Advertencia de Importación", "El archivo CSV está vacío");
                    return 0;
                }

                _logger.LogInformation("Procesando {LineCount} líneas del archivo CSV", lines.Length - 1);

                // Saltar la primera línea (headers) y procesar el resto
                for (int i = 1; i < lines.Length; i++)
                {
                    try
                    {
                        var ticket = CreateTicketFromCsvLine(lines[i], i + 1);
                        if (ticket != null)
                        {
                            // Verificar si el ticket ya existe por IdPeticion o EmailId
                            var existingTicket = await _context.Tickets
                                .FirstOrDefaultAsync(t => 
                                    (!string.IsNullOrEmpty(ticket.IdPeticion) && t.IdPeticion == ticket.IdPeticion) ||
                                    (!string.IsNullOrEmpty(ticket.EmailId) && t.EmailId == ticket.EmailId));

                            if (existingTicket == null)
                            {
                                _context.Tickets.Add(ticket);
                                ticketsImported++;
                                _logger.LogDebug("Ticket agregado: {Subject}", ticket.Subject);
                            }
                            else
                            {
                                _logger.LogDebug("Ticket ya existe, saltando: {Subject}", ticket.Subject);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error procesando línea {LineNumber}", i + 1);
                        continue; // Continuar con la siguiente línea
                    }
                }

                if (ticketsImported > 0)
                {
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Importación completada: {TicketCount} tickets importados", ticketsImported);
                await _notificationService.SendNotificationAsync("Importación Exitosa", $"Importación de CSV completada, {ticketsImported} tickets importados");

                return ticketsImported;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la importación desde CSV");
                await _notificationService.SendNotificationAsync("Error de Importación", "Error durante la importación desde CSV");
                throw;
            }
        }

        private Ticket? CreateTicketFromCsvLine(string csvLine, int lineNumber)
        {
            try
            {
                var fields = ParseCsvLine(csvLine);
                
                if (fields.Count < 27) // Verificar que tengamos todas las columnas esperadas
                {
                    _logger.LogWarning("Línea {LineNumber} no tiene suficientes columnas: {FieldCount}", lineNumber, fields.Count);
                    return null;
                }

                // Mapeo de campos basado en el CSV proporcionado
                var idIncidencia = fields[0].Trim();      // A: ID de la Incidencia
                var idPeticion = fields[1].Trim();        // B: ID de la Petición
                var descripcion = fields[2].Trim();       // C: Detalle de la Descripción
                var grupoAsignado = fields[3].Trim();     // D: Grupo Asignado
                var prioridad = fields[4].Trim();         // E: Prioridad
                var estatus = fields[5].Trim();           // F: Estatus
                var fechaAsignacion = fields[6].Trim();   // G: Fecha Asignación
                var fechaSolucion = fields[7].Trim();     // H: Fecha Solución
                var apellidos = fields[8].Trim();         // I: Apellidos
                var nombre = fields[9].Trim();            // J: Nombre
                var criticidad = fields[10].Trim();       // K: Criticidad
                var tipoQueja = fields[11].Trim();        // L: Tipo Queja
                var origen = fields[12].Trim();           // M: Origen
                var categoria = fields[13].Trim();        // N: Categoría
                var grupoResolutor = fields[14].Trim();   // O: Grupo Resolutor
                var historial = fields[15].Trim();        // P: Historial
                var avance = fields[16].Trim();           // Q: Avance
                var visorAplicativo = fields[17].Trim();  // R: Visor / Aplicativo afectado
                var problema = fields[18].Trim();         // S: Problema
                var detalleProblema = fields[19].Trim();  // T: Detalle del Problema
                var quienAtiende = fields[20].Trim();     // U: Quien atiende?
                var tiempoResolucion = fields[21].Trim(); // V: Tiempo de resolución
                var fechaAck = fields[22].Trim();         // W: Fecha ACK equipo de precargas
                var solucionRemedy = fields[23].Trim();   // X: Solución Remedy
                var precarga = fields[24].Trim();         // Y: Precarga
                var rfc = fields[25].Trim();              // Z: RFC o Solicitud de Cambio
                var causaRaiz = fields[26].Trim();        // AA: Causa Raíz

                // Crear el ticket solo si tiene datos mínimos
                if (string.IsNullOrEmpty(idIncidencia) && string.IsNullOrEmpty(idPeticion) && string.IsNullOrEmpty(descripcion))
                {
                    _logger.LogDebug("Línea {LineNumber} saltada: no tiene datos suficientes", lineNumber);
                    return null;
                }

                var ticket = new Ticket
                {
                    EmailId = idIncidencia, // Usar ID de incidencia como EmailId
                    Subject = !string.IsNullOrEmpty(descripcion) ? TruncateString(descripcion, 200) : "Importado desde CSV",
                    Description = descripcion ?? "",
                    Body = descripcion,
                    ConversationId = idPeticion,
                    
                    // Campos extendidos
                    IdPeticion = idPeticion,
                    GrupoAsignado = grupoAsignado,
                    Nombre = nombre,
                    Apellidos = apellidos,
                    GrupoResolutor = grupoResolutor,
                    Historial = historial,
                    VisorAplicativoAfectado = visorAplicativo,
                    Problema = problema,
                    DetalleProblema = detalleProblema,
                    QuienAtiende = quienAtiende,
                    Precarga = precarga,
                    SolucionRemedy = solucionRemedy,
                    RfcSolicitudCambio = rfc,
                    CausaRaiz = causaRaiz,
                    TipoQueja = tipoQueja,
                    Origen = origen,

                    // Mapear Status
                    Status = MapStatus(estatus),
                    
                    // Mapear Priority
                    Priority = MapPriority(prioridad),
                    
                    // Mapear Criticidad
                    Criticidad = MapCriticidad(criticidad),
                    
                    // Mapear Category
                    Category = categoria ?? "General",
                    
                    // Campos de auditoría
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow,
                    
                    // Campos de email (valores por defecto)
                    FromEmail = "csv-import@system.local",
                    FromName = !string.IsNullOrEmpty(nombre) && !string.IsNullOrEmpty(apellidos) 
                        ? $"{nombre} {apellidos}" 
                        : "Importación CSV"
                };

                // Parsear fechas si están disponibles
                if (!string.IsNullOrEmpty(fechaAsignacion))
                {
                    if (DateTime.TryParseExact(fechaAsignacion, new[] { "dd/MM/yy HH:mm", "dd/MM/yyyy HH:mm", "MM/dd/yyyy", "dd/MM/yyyy" }, 
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaAsignacionParsed))
                    {
                        ticket.FechaAsignacion = fechaAsignacionParsed;
                    }
                }

                if (!string.IsNullOrEmpty(fechaSolucion))
                {
                    if (DateTime.TryParseExact(fechaSolucion, new[] { "dd/MM/yy HH:mm", "dd/MM/yyyy HH:mm", "MM/dd/yyyy", "dd/MM/yyyy" }, 
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaSolucionParsed))
                    {
                        ticket.ResolvedDate = fechaSolucionParsed;
                    }
                }

                if (!string.IsNullOrEmpty(fechaAck))
                {
                    if (DateTime.TryParseExact(fechaAck, new[] { "dd/MM/yy", "dd/MM/yyyy", "MM/dd/yyyy" }, 
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaAckParsed))
                    {
                        ticket.FechaAckPrecargas = fechaAckParsed;
                    }
                }

                // Parsear tiempo de resolución
                if (!string.IsNullOrEmpty(tiempoResolucion))
                {
                    if (decimal.TryParse(tiempoResolucion.Replace(" min", "").Replace(" horas", "").Replace(" h", ""), out var tiempoNum))
                    {
                        if (tiempoResolucion.Contains("min"))
                        {
                            ticket.TiempoResolucionHoras = tiempoNum / 60; // Convertir minutos a horas
                        }
                        else
                        {
                            ticket.TiempoResolucionHoras = tiempoNum;
                        }
                    }
                }

                // Parsear avance si es numérico
                if (!string.IsNullOrEmpty(avance) && decimal.TryParse(avance, out var avanceNum))
                {
                    ticket.Avance = avanceNum;
                }

                return ticket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando ticket desde línea {LineNumber}", lineNumber);
                return null;
            }
        }

        private List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var currentField = new System.Text.StringBuilder();
            var inQuotes = false;
            var i = 0;

            while (i < line.Length)
            {
                var c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Double quote - add single quote to field
                        currentField.Append('"');
                        i += 2;
                    }
                    else
                    {
                        // Toggle quote mode
                        inQuotes = !inQuotes;
                        i++;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    // End of field
                    fields.Add(currentField.ToString());
                    currentField.Clear();
                    i++;
                }
                else
                {
                    currentField.Append(c);
                    i++;
                }
            }

            // Add the last field
            fields.Add(currentField.ToString());

            return fields;
        }

        private string TruncateString(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
                return input ?? string.Empty;
            
            return input.Substring(0, maxLength - 3) + "...";
        }

        private TicketStatus MapStatus(string status)
        {
            return status?.ToLower() switch
            {
                "abierto" or "en curso" or "en atención" => TicketStatus.InProgress,
                "reasignado" => TicketStatus.InProgress,
                "cerrado" or "resuelto" => TicketStatus.Resolved,
                _ => TicketStatus.Backlog
            };
        }

        private Priority MapPriority(string priority)
        {
            return priority?.ToLower() switch
            {
                "baja" => Priority.Low,
                "medio" or "media" => Priority.Medium,
                "alta" => Priority.High,
                "critica" or "crítica" => Priority.Critical,
                _ => Priority.Medium
            };
        }

        private CriticidadLevel? MapCriticidad(string criticidad)
        {
            return criticidad?.ToLower() switch
            {
                "muy baja" => CriticidadLevel.MuyBaja,
                "baja" => CriticidadLevel.Baja,
                "media" or "medio" => CriticidadLevel.Media,
                "alta" => CriticidadLevel.Alta,
                "muy alta" => CriticidadLevel.MuyAlta,
                "critica" or "crítica" => CriticidadLevel.Critica,
                _ => null
            };
        }
    }
}
