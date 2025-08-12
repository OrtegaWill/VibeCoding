using OfficeOpenXml;
using OutlookTicketManager.Data;
using OutlookTicketManager.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using System.Text;

namespace OutlookTicketManager.Services
{
    public class FileImportService
    {
        private readonly TicketDbContext _context;
        private readonly ILogger<FileImportService> _logger;
        private readonly NotificationService _notificationService;

        public FileImportService(
            TicketDbContext context, 
            ILogger<FileImportService> logger,
            NotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
            
            // Configurar licencia de EPPlus para uso no comercial usando variable de entorno
            Environment.SetEnvironmentVariable("EPPlusLicense", "NonCommercial");
        }

        public async Task<int> ImportTicketsFromFileAsync(string filePath)
        {
            _logger.LogInformation("Iniciando importación desde archivo: {FilePath}", filePath);

            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogError("Archivo no encontrado: {FilePath}", filePath);
                    if (_notificationService != null)
                        await _notificationService.SendNotificationAsync("Error de Importación", "Archivo no encontrado");
                    return 0;
                }

                var fileExtension = Path.GetExtension(filePath).ToLower();
                var fileName = Path.GetFileName(filePath);
                var ticketsImported = 0;

                _logger.LogInformation("Procesando archivo: {FileName} con extensión: {Extension}", fileName, fileExtension);

                if (fileExtension == ".csv")
                {
                    // Para archivos CSV
                    _logger.LogInformation("Archivo CSV detectado. Procesando con CsvHelper...");
                    ticketsImported = await ProcessCsvFile(filePath);
                }
                else if (fileExtension == ".xlsx")
                {
                    // Para archivos .xlsx modernos (formato recomendado)
                    _logger.LogInformation("Archivo .xlsx detectado. Procesando con EPPlus...");
                    ticketsImported = await ProcessExcelFile(filePath);
                }
                else if (fileExtension == ".xls")
                {
                    // Para archivos .xls antiguos, mostrar advertencia pero intentar procesar
                    _logger.LogWarning("Archivo .xls detectado. Se recomienda convertir a .xlsx para mejor compatibilidad.");
                    await _notificationService.SendNotificationAsync("Formato .xls Detectado", 
                        "Archivo .xls detectado. Si encuentras problemas, convierte el archivo a .xlsx desde Excel.");
                    
                    try
                    {
                        ticketsImported = await ProcessExcelFile(filePath);
                    }
                    catch (System.IO.InvalidDataException ex) when (ex.Message.Contains("encryption") || ex.Message.Contains("EncryptionInfo"))
                    {
                        _logger.LogError("El archivo .xls no es compatible. Error: {Error}", ex.Message);
                        await _notificationService.SendNotificationAsync("Archivo .xls No Compatible", 
                            "El archivo .xls seleccionado usa un formato no compatible. Solución:\n\n" +
                            "1. Abre el archivo en Microsoft Excel\n" +
                            "2. Ve a 'Archivo' > 'Guardar como'\n" +
                            "3. Cambia el formato a 'Libro de Excel (.xlsx)'\n" +
                            "4. Guarda y vuelve a intentar la importación");
                        return 0;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error procesando archivo .xls");
                        await _notificationService.SendNotificationAsync("Error con Archivo .xls", 
                            $"Error procesando archivo .xls: {ex.Message}\n\nRecomendación: Convertir a formato .xlsx");
                        return 0;
                    }
                }
                else
                {
                    _logger.LogError("Formato de archivo no soportado: {Extension}", fileExtension);
                    await _notificationService.SendNotificationAsync("Formato No Soportado", 
                        $"El archivo '{fileName}' no es compatible.\n\nFormatos soportados:\n• Excel (.xlsx) - Recomendado\n• Excel (.xls) - Compatibilidad limitada");
                    return 0;
                }

                _logger.LogInformation("Importación completada: {TicketCount} tickets importados desde {FileName}", 
                    ticketsImported, fileName);
                
                if (ticketsImported > 0)
                {
                    if (_notificationService != null)
                        await _notificationService.SendNotificationAsync("Importación Exitosa", 
                            $"✅ Se importaron {ticketsImported} tickets correctamente desde '{fileName}'");
                }
                else
                {
                    if (_notificationService != null)
                        await _notificationService.SendNotificationAsync("Sin Datos para Importar", 
                            $"⚠️ No se encontraron tickets nuevos en '{fileName}'\n\n" +
                            "Posibles causas:\n• Los tickets ya existen en el sistema\n• El archivo no contiene datos válidos\n• Estructura de columnas incorrecta");
                }

                return ticketsImported;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la importación desde Excel");
                
                if (_notificationService != null)
                    await _notificationService.SendNotificationAsync("Error de Importación", 
                        $"❌ Error durante la importación:\n{ex.Message}\n\nRevisa el formato del archivo y vuelve a intentar.");
                        
                throw;
            }
        }

        private async Task<int> ProcessCsvFile(string filePath)
        {
            var importedTickets = 0;
            var fileName = Path.GetFileName(filePath);

            try
            {
                _logger.LogInformation("Procesando archivo CSV: {FileName}", fileName);

                // Configurar CsvHelper para manejar archivos con encoding UTF-8 y delimitador de coma
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ",",
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    ReadingExceptionOccurred = null,
                    TrimOptions = TrimOptions.Trim
                };

                using var reader = new StreamReader(filePath, Encoding.UTF8);
                using var csv = new CsvReader(reader, config);
                
                var csvRecords = new List<CsvTicketRecord>();
                
                try 
                {
                    csvRecords = csv.GetRecords<CsvTicketRecord>().ToList();
                    _logger.LogInformation("Leídos {Count} registros del archivo CSV", csvRecords.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al leer registros del archivo CSV");
                    if (_notificationService != null)
                        await _notificationService.SendNotificationAsync("Error CSV", $"Error al leer el archivo CSV: {ex.Message}");
                    return 0;
                }

                foreach (var csvRecord in csvRecords)
                {
                    try
                    {
                        var ticket = await ProcessCsvRow(csvRecord);
                        if (ticket != null)
                        {
                            importedTickets++;
                            _logger.LogInformation("Ticket procesado: {Subject}", ticket.Subject);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error procesando registro CSV: {IncidentId}", csvRecord.IncidentId);
                        continue;
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Importación CSV completada: {Count} tickets importados desde {FileName}", importedTickets, fileName);

                if (_notificationService != null)
                {
                    if (importedTickets > 0)
                    {
                        await _notificationService.SendNotificationAsync("Importación CSV Exitosa", 
                            $"✅ Se importaron {importedTickets} tickets desde {fileName}");
                    }
                    else
                    {
                        await _notificationService.SendNotificationAsync("Importación CSV Sin Resultados", 
                            $"⚠️ No se encontraron tickets válidos para importar desde {fileName}");
                    }
                }

                return importedTickets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando archivo CSV: {FileName}", fileName);
                if (_notificationService != null)
                    await _notificationService.SendNotificationAsync("Error CSV", 
                        $"❌ Error procesando {fileName}: {ex.Message}");
                throw;
            }
        }

        private async Task<Ticket?> ProcessCsvRow(CsvTicketRecord csvRecord)
        {
            // Validar que los campos esenciales no estén vacíos
            if (string.IsNullOrWhiteSpace(csvRecord.IncidentId) || 
                string.IsNullOrWhiteSpace(csvRecord.DetailedDescription))
            {
                _logger.LogDebug("Fila CSV omitida - falta ID de incidencia o descripción");
                return null;
            }

            // Verificar si ya existe un ticket con este ID de incidencia
            var existingTicket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.EmailId == csvRecord.IncidentId);

            if (existingTicket != null)
            {
                _logger.LogDebug("Ticket duplicado omitido: {IncidentId}", csvRecord.IncidentId);
                return null;
            }

            // Crear nuevo ticket
            var ticket = new Ticket
            {
                EmailId = csvRecord.IncidentId!, // Ya validamos que no es null
                Subject = !string.IsNullOrWhiteSpace(csvRecord.IncidentId) ? 
                         $"Incidencia {csvRecord.IncidentId}" : "Sin asunto",
                Description = csvRecord.DetailedDescription ?? string.Empty,
                Body = csvRecord.DetailedDescription,
                Status = MapCsvStatusToTicketStatus(csvRecord.Status),
                Priority = MapCsvPriorityToTicketPriority(csvRecord.Priority),
                Category = csvRecord.ProductName ?? "Sin categoría",
                FromEmail = "csv@import.local", // Valor por defecto para archivos CSV
                FromName = !string.IsNullOrWhiteSpace(csvRecord.FirstName) && !string.IsNullOrWhiteSpace(csvRecord.LastName) ?
                          $"{csvRecord.FirstName} {csvRecord.LastName}".Trim() : "Importación CSV",
                AssignedTo = csvRecord.AssignedUser,
                CreatedDate = ParseCsvDate(csvRecord.NotificationDate) ?? DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                ResolvedDate = ParseCsvDate(csvRecord.SolutionDate),
                Tags = $"CSV-Import,{csvRecord.AssignedGroup ?? "Sin-Grupo"}".Trim(',')
            };

            // Si hay solución, agregar como comentario
            if (!string.IsNullOrWhiteSpace(csvRecord.Solution))
            {
                var comment = new TicketComment
                {
                    Content = csvRecord.Solution,
                    AuthorName = csvRecord.AssignedUser ?? "Sistema",
                    CreatedDate = ticket.ResolvedDate ?? DateTime.UtcNow,
                    Ticket = ticket
                };
                
                ticket.Comments = new List<TicketComment> { comment };
            }

            await _context.Tickets.AddAsync(ticket);
            _logger.LogDebug("Ticket creado desde CSV: {Subject}", ticket.Subject);

            return ticket;
        }

        private static TicketStatus MapCsvStatusToTicketStatus(string? csvStatus)
        {
            if (string.IsNullOrWhiteSpace(csvStatus))
                return TicketStatus.Backlog;

            return csvStatus.ToLower().Trim() switch
            {
                "en curso" => TicketStatus.InProgress,
                "abierto" => TicketStatus.Backlog,
                "cerrado" => TicketStatus.Resolved,
                "resuelto" => TicketStatus.Resolved,
                "pendiente" => TicketStatus.Blocked,
                "bloqueado" => TicketStatus.Blocked,
                "en revisión" => TicketStatus.InReview,
                "revisión" => TicketStatus.InReview,
                _ => TicketStatus.Backlog
            };
        }

        private static Priority MapCsvPriorityToTicketPriority(string? csvPriority)
        {
            if (string.IsNullOrWhiteSpace(csvPriority))
                return Priority.Medium;

            return csvPriority.ToLower().Trim() switch
            {
                "alta" => Priority.High,
                "high" => Priority.High,
                "crítica" => Priority.Critical,
                "critical" => Priority.Critical,
                "baja" => Priority.Low,
                "low" => Priority.Low,
                "media" => Priority.Medium,
                "medium" => Priority.Medium,
                _ => Priority.Medium
            };
        }

        private static DateTime? ParseCsvDate(string? csvDate)
        {
            if (string.IsNullOrWhiteSpace(csvDate))
                return null;

            // Formatos comunes de fecha en español
            string[] dateFormats = {
                "dd/MM/yy HH:mm",
                "dd/MM/yyyy HH:mm",
                "dd/MM/yy",
                "dd/MM/yyyy",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd",
                "MM/dd/yyyy HH:mm",
                "MM/dd/yyyy"
            };

            foreach (var format in dateFormats)
            {
                if (DateTime.TryParseExact(csvDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    return parsedDate;
                }
            }

            // Intento de parsing genérico
            if (DateTime.TryParse(csvDate, CultureInfo.GetCultureInfo("es-ES"), DateTimeStyles.None, out var genericDate))
            {
                return genericDate;
            }

            return null;
        }

        private async Task<int> ProcessExcelFile(string filePath)
        {
            var importedTickets = 0;
            var fileName = Path.GetFileName(filePath);

            try
            {
                _logger.LogInformation("Procesando archivo Excel: {FileName}", fileName);

                // Configurar EPPlus - simplificado
                System.Environment.SetEnvironmentVariable("EPPlusLicense", "NonCommercial");

                using var package = new ExcelPackage(new FileInfo(filePath));
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                {
                    _logger.LogWarning("No se encontraron hojas de trabajo en {FileName}", fileName);
                    await _notificationService.SendNotificationAsync("Archivo Vacío", 
                        $"El archivo '{fileName}' no contiene hojas de trabajo válidas.");
                    return 0;
                }

                var rowCount = worksheet.Dimension?.Rows ?? 0;
                if (rowCount <= 1)
                {
                    _logger.LogWarning("El archivo {FileName} está vacío o solo contiene encabezados", fileName);
                    await _notificationService.SendNotificationAsync("Sin Datos", 
                        $"El archivo '{fileName}' no contiene datos para importar (solo encabezados o está vacío).");
                    return 0;
                }

                _logger.LogInformation("Procesando {RowCount} filas desde {FileName}", rowCount - 1, fileName);

                // Procesar cada fila (comenzando desde la fila 2, asumiendo encabezados en la fila 1)
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        _logger.LogInformation("🔍 Procesando fila {Row}", row);
                        
                        // Debug: Verificar qué valores está leyendo
                        var debugCol1 = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                        _logger.LogInformation("🔍 Fila {Row} - Col1 (ID): '{DebugCol1}'", row, debugCol1 ?? "NULL");
                        
                        var ticket = ProcessExcelRow(worksheet, row);
                        
                        if (ticket != null)
                        {
                            _logger.LogInformation("✅ Fila {Row} procesada correctamente - Ticket: {Subject}", row, ticket.Subject);
                            
                            // Verificar si el ticket ya existe
                            var existingTicket = await _context.Tickets
                                .FirstOrDefaultAsync(t => t.Subject == ticket.Subject);
                            if (existingTicket == null)
                            {
                                _context.Tickets.Add(ticket);
                                await _context.SaveChangesAsync();
                                importedTickets++;
                                _logger.LogInformation("✅ Ticket importado desde fila {Row}: {Subject}", row, ticket.Subject);
                            }
                            else
                            {
                                _logger.LogInformation("⚠️ Ticket ya existe (fila {Row}): {Subject}", row, ticket.Subject);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("❌ Fila {Row} no pudo ser procesada - ProcessExcelRow devolvió null", row);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Error procesando fila {Row} en {FileName}", row, fileName);
                        // Continuar con la siguiente fila en lugar de fallar completamente
                    }
                }

                _logger.LogInformation("Procesamiento de {FileName} completado. {ImportedCount}/{TotalRows} tickets importados", 
                    fileName, importedTickets, rowCount - 1);
                
                return importedTickets;
            }
            catch (InvalidDataException ex) when (ex.Message.Contains("Excel") || ex.Message.Contains("format"))
            {
                _logger.LogError(ex, "Formato de archivo Excel inválido: {FileName}", fileName);
                await _notificationService.SendNotificationAsync("Formato Excel Inválido", 
                    $"El archivo '{fileName}' no tiene un formato Excel válido.\n\n" +
                    "Solución:\n" +
                    "1. Abre el archivo en Excel\n" +
                    "2. Guárdalo como .xlsx\n" +
                    "3. Vuelve a intentar la importación");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando archivo Excel: {FileName}", fileName);
                return 0;
            }
        }

        private Ticket? CreateTicketFromExcelRow(ExcelWorksheet worksheet, int row)
        {
            try
            {
                // Mapeo de columnas basado en el archivo BackLog.csv proporcionado
                // Columnas: ID de la incidencia*+, ID de petición de servicio, Descripción Detallada, 
                // Grupo asignado*+, Prioridad*, Estado*, Fecha de notificación+, Fecha de solucion, 
                // Apellidos+, Nombre+, Solución, Usuario asignado+, Nombre del producto+
                
                var idIncidencia = GetCellValue(worksheet, row, 1);    // A: ID de la incidencia*+
                var idPeticion = GetCellValue(worksheet, row, 2);      // B: ID de petición de servicio
                var descripcion = GetCellValue(worksheet, row, 3);     // C: Descripción Detallada
                var grupoAsignado = GetCellValue(worksheet, row, 4);   // D: Grupo asignado*+
                var prioridad = GetCellValue(worksheet, row, 5);       // E: Prioridad*
                var estado = GetCellValue(worksheet, row, 6);          // F: Estado*
                var fechaNotificacion = GetCellValue(worksheet, row, 7); // G: Fecha de notificación+
                var fechaSolucion = GetCellValue(worksheet, row, 8);   // H: Fecha de solucion
                var apellidos = GetCellValue(worksheet, row, 9);       // I: Apellidos+
                var nombre = GetCellValue(worksheet, row, 10);         // J: Nombre+
                var solucion = GetCellValue(worksheet, row, 11);       // K: Solución
                var usuarioAsignado = GetCellValue(worksheet, row, 12); // L: Usuario asignado+
                // Ignoramos la columna 13: Nombre del producto+ según la solicitud

                // Crear el ticket con el mapeo específico del archivo BackLog.csv
                var ticket = new Ticket
                {
                    EmailId = idIncidencia, // Usar ID de incidencia como EmailId
                    Subject = !string.IsNullOrEmpty(descripcion) ? TruncateString(descripcion, 200) : "Importado desde Excel",
                    Description = descripcion ?? "",
                    Body = descripcion,
                    ConversationId = idPeticion,
                    
                    // Campos extendidos mapeados del archivo BackLog.csv
                    IdPeticion = idPeticion,
                    GrupoAsignado = grupoAsignado,
                    Nombre = nombre,
                    Apellidos = apellidos,
                    AssignedTo = usuarioAsignado,
                    SolucionRemedy = solucion,

                    // Mapear Status desde columna "Estado*"
                    Status = MapStatus(estado),
                    
                    // Mapear Priority desde columna "Prioridad*"
                    Priority = MapPriority(prioridad),
                    
                    // Category - no mapeamos "Nombre del producto+" según solicitud
                    Category = "General",
                    
                    // Campos de auditoría
                    CreatedDate = ParseDateFromString(fechaNotificacion) ?? DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow,
                    ResolvedDate = ParseDateFromString(fechaSolucion),
                    
                    // Campos de email (valores por defecto)
                    FromEmail = "excel-import@system.local",
                    FromName = !string.IsNullOrEmpty(nombre) && !string.IsNullOrEmpty(apellidos) 
                        ? $"{nombre} {apellidos}" 
                        : "Importación Excel"
                };

                return ticket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando ticket desde fila {Row}", row);
                return null;
            }
        }

        private string GetCellValue(ExcelWorksheet worksheet, int row, int col)
        {
            var cell = worksheet.Cells[row, col];
            return cell?.Value?.ToString()?.Trim() ?? string.Empty;
        }

        private string TruncateString(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
                return input ?? string.Empty;
            
            return input.Substring(0, maxLength - 3) + "...";
        }

        private TicketStatus MapStatus(string status)
        {
            return status?.ToLower().Trim() switch
            {
                "en curso" or "en progreso" or "abierto" => TicketStatus.InProgress,
                "pendiente" or "backlog" => TicketStatus.Backlog,
                "cerrado" or "resuelto" or "completado" => TicketStatus.Resolved,
                _ => TicketStatus.Backlog
            };
        }

        private Priority MapPriority(string priority)
        {
            return priority?.ToLower().Trim() switch
            {
                "baja" => Priority.Low,
                "media" or "medio" => Priority.Medium,
                "alta" => Priority.High,
                "critica" or "crítica" or "crítico" => Priority.Critical,
                _ => Priority.Medium
            };
        }

        private Ticket? ProcessExcelRow(ExcelWorksheet worksheet, int row)
        {
            try
            {
                _logger.LogInformation("🔍 Iniciando ProcessExcelRow para fila {Row}", row);
                
                // Mapeo según el archivo "backlog 06 agosto.xlsx":
                // Col 1: ID de la incidencia*+
                // Col 2: ID de petición de servicio  
                // Col 3: Descripción Detallada
                // Col 4: Grupo asignado*+
                // Col 5: Prioridad*
                // Col 6: Estado*
                // Col 7: Fecha de notificación+
                // Col 8: Fecha de solución
                // Col 9: Apellidos+
                // Col10: Nombre+
                // Col11: Solución
                // Col12: Usuario asignado+
                // Col13: Nombre del producto+
                
                var idIncidencia = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                _logger.LogInformation("🔍 Fila {Row} - ID Incidencia extraído: '{IdIncidencia}'", row, idIncidencia ?? "NULL");
                
                var idPeticion = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                var descripcionDetallada = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                var grupoAsignado = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                var prioridad = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                var estado = worksheet.Cells[row, 6].Value?.ToString()?.Trim();
                var fechaNotificacion = worksheet.Cells[row, 7].Value?.ToString()?.Trim();
                var fechaSolucion = worksheet.Cells[row, 8].Value?.ToString()?.Trim();
                var apellidos = worksheet.Cells[row, 9].Value?.ToString()?.Trim();
                var nombre = worksheet.Cells[row, 10].Value?.ToString()?.Trim();
                var solucion = worksheet.Cells[row, 11].Value?.ToString()?.Trim();
                var usuarioAsignado = worksheet.Cells[row, 12].Value?.ToString()?.Trim();
                var nombreProducto = worksheet.Cells[row, 13].Value?.ToString()?.Trim();

                // Validar que tenga ID de incidencia (campo obligatorio)
                if (string.IsNullOrWhiteSpace(idIncidencia))
                {
                    _logger.LogWarning("❌ Fila {Row}: ID de incidencia vacío, omitiendo. Valor: '{Value}'", row, idIncidencia ?? "NULL");
                    return null;
                }

                _logger.LogInformation("✅ Fila {Row} - Validación ID pasada, creando ticket...", row);

                // Crear un subject combinando ID y descripción
                var subject = !string.IsNullOrWhiteSpace(descripcionDetallada) 
                    ? $"[{idIncidencia}] {descripcionDetallada}"
                    : idIncidencia;

                var ticket = new Ticket
                {
                    EmailId = $"EXCEL_IMPORT_{DateTime.Now:yyyyMMddHHmmss}_{row}_{idIncidencia}",
                    Subject = subject.Length > 500 ? subject[..500] : subject,
                    Description = descripcionDetallada ?? "",
                    Body = descripcionDetallada,
                    FromEmail = "imported@system.local",
                    FromName = !string.IsNullOrWhiteSpace(nombre) && !string.IsNullOrWhiteSpace(apellidos) 
                        ? $"{nombre} {apellidos}".Trim() 
                        : "Sistema de Importación",
                    Status = ParseStatusFromString(estado),
                    Priority = ParsePriorityFromString(prioridad),
                    CreatedDate = ParseDateFromString(fechaNotificacion) ?? DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    
                    // Mapeo específico para campos del backlog
                    Category = nombreProducto ?? "",
                    AssignedTo = usuarioAsignado,
                    Tags = !string.IsNullOrWhiteSpace(grupoAsignado) ? $"Grupo:{grupoAsignado}" : null,
                    
                    // Campos extendidos específicos del modelo
                    IdPeticion = idPeticion,
                    GrupoAsignado = grupoAsignado,
                    Nombre = nombre,
                    Apellidos = apellidos,
                    QuienAtiende = usuarioAsignado,
                    SolucionRemedy = solucion
                };

                // Parsear fecha de resolución si existe
                if (!string.IsNullOrWhiteSpace(fechaSolucion))
                {
                    ticket.ResolvedDate = ParseDateFromString(fechaSolucion);
                }

                // Si hay fecha de notificación, usarla como fecha de asignación
                if (!string.IsNullOrWhiteSpace(fechaNotificacion))
                {
                    ticket.FechaAsignacion = ParseDateFromString(fechaNotificacion);
                }

                _logger.LogInformation("✅ Fila {Row} - Ticket creado exitosamente: {Subject}", row, ticket.Subject);
                return ticket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error procesando fila {Row} de Excel", row);
                return null;
            }
        }

        private TicketStatus ParseStatusFromString(string? statusText)
        {
            if (string.IsNullOrWhiteSpace(statusText))
                return TicketStatus.Backlog;

            return statusText.ToLowerInvariant().Trim() switch
            {
                "nuevo" or "new" => TicketStatus.Backlog,
                "en curso" or "en progreso" or "in progress" => TicketStatus.InProgress,
                "resuelto" or "resolved" or "completado" or "done" => TicketStatus.Resolved,
                "cerrado" or "closed" => TicketStatus.Resolved,
                "pendiente" or "pending" => TicketStatus.InProgress,
                _ => TicketStatus.Backlog
            };
        }

        private DateTime? ParseDateFromString(string? dateText)
        {
            if (string.IsNullOrWhiteSpace(dateText))
                return null;

            // Intentar varios formatos de fecha
            var formats = new[]
            {
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd",
                "dd/MM/yyyy",
                "dd/MM/yyyy HH:mm:ss",
                "MM/dd/yyyy",
                "MM/dd/yyyy HH:mm:ss"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateText.Trim(), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                {
                    return result;
                }
            }

            // Intento genérico como fallback
            if (DateTime.TryParse(dateText.Trim(), out var genericResult))
            {
                return genericResult;
            }

            return null;
        }

        private Priority ParsePriorityFromString(string? priorityText)
        {
            if (string.IsNullOrWhiteSpace(priorityText))
                return Priority.Medium;

            return priorityText.ToLowerInvariant().Trim() switch
            {
                "alta" or "high" or "crítica" or "critica" or "urgent" => Priority.High,
                "baja" or "low" => Priority.Low,
                "media" or "medium" or "normal" => Priority.Medium,
                _ => Priority.Medium
            };
        }

        private decimal? ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
                return result;

            return null;
        }

        public async Task<int> ImportRawDataAsync(string filePath)
        {
            _logger.LogInformation("Iniciando importación de RawData desde archivo: {FilePath}", filePath);

            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogError("Archivo RawData no encontrado: {FilePath}", filePath);
                    if (_notificationService != null)
                        await _notificationService.SendNotificationAsync("Error de Importación RawData", "Archivo no encontrado");
                    return 0;
                }

                var fileName = Path.GetFileName(filePath);
                var importedTickets = 0;

                _logger.LogInformation("Procesando archivo RawData: {FileName}", fileName);

                // Configurar CsvHelper para manejar archivos RawData
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ",",
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    ReadingExceptionOccurred = null,
                    TrimOptions = TrimOptions.Trim,
                    BadDataFound = null // Ignorar datos malformados
                };

                using var reader = new StreamReader(filePath, Encoding.UTF8);
                using var csv = new CsvReader(reader, config);
                
                var rawDataRecords = new List<RawDataRecord>();
                
                try 
                {
                    rawDataRecords = csv.GetRecords<RawDataRecord>().ToList();
                    _logger.LogInformation("Leídos {Count} registros RawData del archivo", rawDataRecords.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al leer registros RawData del archivo");
                    if (_notificationService != null)
                        await _notificationService.SendNotificationAsync("Error RawData", $"Error al leer el archivo RawData: {ex.Message}");
                    return 0;
                }

                foreach (var rawRecord in rawDataRecords)
                {
                    try
                    {
                        var ticket = await ProcessRawDataRow(rawRecord);
                        if (ticket != null)
                        {
                            importedTickets++;
                            _logger.LogInformation("Ticket RawData procesado: {Subject}", ticket.Subject);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error procesando registro RawData: {IncidentId}", rawRecord.IncidentId);
                        continue;
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Importación RawData completada: {Count} tickets importados desde {FileName}", importedTickets, fileName);

                if (_notificationService != null)
                {
                    if (importedTickets > 0)
                    {
                        await _notificationService.SendNotificationAsync("Importación RawData Exitosa", 
                            $"✅ Se importaron {importedTickets} tickets RawData desde {fileName}");
                    }
                    else
                    {
                        await _notificationService.SendNotificationAsync("Importación RawData Sin Resultados", 
                            $"⚠️ No se encontraron tickets válidos para importar desde RawData {fileName}");
                    }
                }

                return importedTickets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando archivo RawData: {FileName}", Path.GetFileName(filePath));
                if (_notificationService != null)
                    await _notificationService.SendNotificationAsync("Error RawData", 
                        $"❌ Error procesando RawData: {ex.Message}");
                throw;
            }
        }

        private async Task<Ticket?> ProcessRawDataRow(RawDataRecord rawRecord)
        {
            // Validar que los campos esenciales no estén vacíos
            if (string.IsNullOrWhiteSpace(rawRecord.IncidentId) || 
                string.IsNullOrWhiteSpace(rawRecord.DescriptionDetail))
            {
                _logger.LogDebug("Fila RawData omitida - falta ID de incidencia o descripción");
                return null;
            }

            // Limpiar y normalizar el ID de incidencia (quitar espacios extra)
            var cleanIncidentId = rawRecord.IncidentId.Trim();

            // Verificar si ya existe un ticket con este ID de incidencia
            var existingTicket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.EmailId == cleanIncidentId);

            if (existingTicket != null)
            {
                _logger.LogDebug("Ticket RawData duplicado omitido: {IncidentId}", cleanIncidentId);
                return null;
            }

            // Crear nuevo ticket desde RawData
            var ticket = new Ticket
            {
                EmailId = cleanIncidentId,
                Subject = rawRecord.DescriptionDetail ?? $"RawData {cleanIncidentId}",
                Description = rawRecord.DescriptionDetail ?? string.Empty,
                Body = BuildRawDataBody(rawRecord),
                Status = MapRawDataStatus(rawRecord.Status),
                Priority = MapRawDataPriority(rawRecord.Priority, rawRecord.Criticality),
                Category = rawRecord.Category ?? rawRecord.AffectedApplication ?? "RawData",
                FromEmail = !string.IsNullOrWhiteSpace(rawRecord.FirstName) && !string.IsNullOrWhiteSpace(rawRecord.LastName) ?
                           $"{rawRecord.FirstName}.{rawRecord.LastName}@empresa.com" : "rawdata@import.local",
                FromName = !string.IsNullOrWhiteSpace(rawRecord.FirstName) && !string.IsNullOrWhiteSpace(rawRecord.LastName) ?
                          $"{rawRecord.FirstName} {rawRecord.LastName}".Trim() : "RawData Import",
                AssignedTo = rawRecord.AssignedTo ?? rawRecord.AssignedGroup,
                CreatedDate = ParseRawDataDate(rawRecord.AssignmentDate) ?? DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                ResolvedDate = ParseRawDataDate(rawRecord.SolutionDate),
                Tags = BuildRawDataTags(rawRecord)
            };

            // Si hay historial o solución, agregar como comentario
            var commentContent = BuildRawDataComments(rawRecord);
            if (!string.IsNullOrWhiteSpace(commentContent))
            {
                var comment = new TicketComment
                {
                    Content = commentContent,
                    AuthorName = rawRecord.AssignedTo ?? "Sistema RawData",
                    CreatedDate = ticket.ResolvedDate ?? DateTime.UtcNow,
                    Ticket = ticket
                };
                
                ticket.Comments = new List<TicketComment> { comment };
            }

            await _context.Tickets.AddAsync(ticket);
            _logger.LogDebug("Ticket RawData creado: {Subject}", ticket.Subject);

            return ticket;
        }

        private static string BuildRawDataBody(RawDataRecord rawRecord)
        {
            var body = new StringBuilder();
            body.AppendLine($"**Descripción:** {rawRecord.DescriptionDetail}");
            
            if (!string.IsNullOrWhiteSpace(rawRecord.Problem))
                body.AppendLine($"**Problema:** {rawRecord.Problem}");
            
            if (!string.IsNullOrWhiteSpace(rawRecord.ProblemDetail))
                body.AppendLine($"**Detalle del Problema:** {rawRecord.ProblemDetail}");
            
            if (!string.IsNullOrWhiteSpace(rawRecord.AffectedApplication))
                body.AppendLine($"**Aplicativo Afectado:** {rawRecord.AffectedApplication}");
            
            if (!string.IsNullOrWhiteSpace(rawRecord.Origin))
                body.AppendLine($"**Origen:** {rawRecord.Origin}");
            
            if (!string.IsNullOrWhiteSpace(rawRecord.ComplaintType))
                body.AppendLine($"**Tipo de Queja:** {rawRecord.ComplaintType}");

            if (!string.IsNullOrWhiteSpace(rawRecord.History))
                body.AppendLine($"**Historia del ticket:** {rawRecord.History}");
            
            if (!string.IsNullOrWhiteSpace(rawRecord.Category))
                body.AppendLine($"**Categorización:** {rawRecord.Category}");
            
            if (!string.IsNullOrWhiteSpace(rawRecord.RemedySolution))
                body.AppendLine($"**Solución propuesta:** {rawRecord.RemedySolution}");
            
            if (!string.IsNullOrWhiteSpace(rawRecord.Progress))
                body.AppendLine($"**Progreso:** {rawRecord.Progress}");

            return body.ToString();
        }

        private static string BuildRawDataTags(RawDataRecord rawRecord)
        {
            var tags = new List<string> { "RawData-Import" };
            
            if (!string.IsNullOrWhiteSpace(rawRecord.AssignedGroup))
                tags.Add(rawRecord.AssignedGroup);
            
            if (!string.IsNullOrWhiteSpace(rawRecord.ResolverGroup))
                tags.Add(rawRecord.ResolverGroup);
            
            if (!string.IsNullOrWhiteSpace(rawRecord.Origin))
                tags.Add(rawRecord.Origin);
            
            if (!string.IsNullOrWhiteSpace(rawRecord.Preload))
                tags.Add(rawRecord.Preload);

            return string.Join(",", tags.Where(t => !string.IsNullOrWhiteSpace(t)));
        }

        private static string BuildRawDataComments(RawDataRecord rawRecord)
        {
            var comments = new StringBuilder();
            
            if (!string.IsNullOrWhiteSpace(rawRecord.History))
                comments.AppendLine($"**Historial:**\n{rawRecord.History}");
            
            if (!string.IsNullOrWhiteSpace(rawRecord.RemedySolution))
                comments.AppendLine($"**Solución Remedy:**\n{rawRecord.RemedySolution}");
            
            if (!string.IsNullOrWhiteSpace(rawRecord.Progress))
                comments.AppendLine($"**Avance:** {rawRecord.Progress}");
            
            if (!string.IsNullOrWhiteSpace(rawRecord.RootCause))
                comments.AppendLine($"**Causa Raíz:** {rawRecord.RootCause}");

            return comments.ToString();
        }

        private static TicketStatus MapRawDataStatus(string? rawStatus)
        {
            if (string.IsNullOrWhiteSpace(rawStatus))
                return TicketStatus.Backlog;

            return rawStatus.ToLower().Trim() switch
            {
                "resuelto" => TicketStatus.Resolved,
                "en progreso" => TicketStatus.InProgress,
                "reasignado" => TicketStatus.InProgress,
                "atendido" => TicketStatus.InProgress,
                "rechazado" => TicketStatus.Blocked,
                "falta informacion" => TicketStatus.Blocked,
                "cerrado" => TicketStatus.Resolved,
                _ => TicketStatus.Backlog
            };
        }

        private static Priority MapRawDataPriority(string? rawPriority, string? criticality)
        {
            // Primero revisar criticidad si está disponible
            if (!string.IsNullOrWhiteSpace(criticality))
            {
                return criticality.ToLower().Trim() switch
                {
                    "alta" => Priority.High,
                    "crítica" => Priority.Critical,
                    "critical" => Priority.Critical,
                    "medio" => Priority.Medium,
                    "baja" => Priority.Low,
                    _ => Priority.Medium
                };
            }

            // Luego revisar prioridad
            if (!string.IsNullOrWhiteSpace(rawPriority))
            {
                return rawPriority.ToLower().Trim() switch
                {
                    "alta" => Priority.High,
                    "high" => Priority.High,
                    "crítica" => Priority.Critical,
                    "critical" => Priority.Critical,
                    "baja" => Priority.Low,
                    "low" => Priority.Low,
                    "media" => Priority.Medium,
                    "medium" => Priority.Medium,
                    _ => Priority.Medium
                };
            }

            return Priority.Medium;
        }

        private static DateTime? ParseRawDataDate(string? rawDate)
        {
            if (string.IsNullOrWhiteSpace(rawDate))
                return null;

            // Formatos comunes de fecha en RawData
            string[] dateFormats = {
                "dd/MM/yy HH:mm",
                "dd/MM/yyyy HH:mm",
                "dd/MM/yy",
                "dd/MM/yyyy",
                "M/d/yyyy h:mm:ss tt", // 7/16/2025
                "M/dd/yyyy",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd",
                "MM/dd/yyyy HH:mm",
                "MM/dd/yyyy"
            };

            foreach (var format in dateFormats)
            {
                if (DateTime.TryParseExact(rawDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    return parsedDate;
                }
            }

            // Intento de parsing genérico
            if (DateTime.TryParse(rawDate, CultureInfo.GetCultureInfo("es-ES"), DateTimeStyles.None, out var genericDate))
            {
                return genericDate;
            }

            return null;
        }
    }
}
