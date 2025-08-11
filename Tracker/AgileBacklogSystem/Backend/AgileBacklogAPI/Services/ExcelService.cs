using OfficeOpenXml;
using AgileBacklogAPI.Data;
using AgileBacklogAPI.Models;
using AgileBacklogAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AgileBacklogAPI.Services
{
    public class ExcelService
    {
        private readonly AgileBacklogContext _context;
        
        public ExcelService(AgileBacklogContext context)
        {
            _context = context;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }
        
        public async Task<List<TareaDto>> ImportarTareasFromExcel(Stream excelStream)
        {
            var tareasImportadas = new List<TareaDto>();
            
            using var package = new ExcelPackage(excelStream);
            var worksheet = package.Workbook.Worksheets[0];
            
            if (worksheet == null)
                throw new InvalidOperationException("No se encontró la hoja de Excel");
                
            var rowCount = worksheet.Dimension?.Rows ?? 0;
            if (rowCount <= 1)
                throw new InvalidOperationException("El archivo Excel está vacío o solo contiene encabezados");
            
            // Mapeo de columnas (basado en el archivo BacKLog.xlsx)
            var columnMapping = new Dictionary<string, int>
            {
                ["IdIncidencia"] = 1,
                ["IdPeticion"] = 2,
                ["DetalleDescripcion"] = 3,
                ["GrupoAsignado"] = 4,
                ["Prioridad"] = 5,
                ["Estatus"] = 6,
                ["FechaAsignacion"] = 7,
                ["FechaSolucion"] = 8,
                ["Apellidos"] = 9,
                ["Nombre"] = 10,
                ["Criticidad"] = 11,
                ["TipoQueja"] = 12,
                ["Origen"] = 13,
                ["Categoria"] = 14,
                ["GrupoResolutor"] = 15,
                ["Historial"] = 16,
                ["Avance"] = 17,
                ["VisorAplicativoAfectado"] = 18,
                ["Problema"] = 19,
                ["DetalleProblema"] = 20,
                ["QuienAtiende"] = 21,
                ["TiempoResolucion"] = 22,
                ["FechaAckEquipoPrecargas"] = 23,
                ["SolucionRemedy"] = 24,
                ["Precarga"] = 25,
                ["RfcSolicitudCambio"] = 26,
                ["CausaRaiz"] = 27
            };
            
            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var tarea = new Tarea
                    {
                        IdIncidencia = GetCellValue(worksheet, row, columnMapping["IdIncidencia"]),
                        IdPeticion = GetCellValue(worksheet, row, columnMapping["IdPeticion"]),
                        DetalleDescripcion = GetCellValue(worksheet, row, columnMapping["DetalleDescripcion"]),
                        GrupoAsignadoId = await GetOrCreateCatalogoId("GrupoAsignado", GetCellValue(worksheet, row, columnMapping["GrupoAsignado"])),
                        PrioridadId = await GetOrCreateCatalogoId("Prioridad", GetCellValue(worksheet, row, columnMapping["Prioridad"])),
                        EstatusId = await GetOrCreateCatalogoId("Estatus", GetCellValue(worksheet, row, columnMapping["Estatus"])),
                        FechaAsignacion = GetDateValue(worksheet, row, columnMapping["FechaAsignacion"]),
                        FechaSolucion = GetDateValue(worksheet, row, columnMapping["FechaSolucion"]),
                        Apellidos = GetCellValue(worksheet, row, columnMapping["Apellidos"]),
                        Nombre = GetCellValue(worksheet, row, columnMapping["Nombre"]),
                        CriticidadId = await GetOrCreateCatalogoId("Criticidad", GetCellValue(worksheet, row, columnMapping["Criticidad"])),
                        TipoQuejaId = await GetOrCreateCatalogoId("TipoQueja", GetCellValue(worksheet, row, columnMapping["TipoQueja"])),
                        OrigenId = await GetOrCreateCatalogoId("Origen", GetCellValue(worksheet, row, columnMapping["Origen"])),
                        CategoriaId = await GetOrCreateCatalogoId("Categoria", GetCellValue(worksheet, row, columnMapping["Categoria"])),
                        GrupoResolutorId = await GetOrCreateCatalogoId("GrupoResolutor", GetCellValue(worksheet, row, columnMapping["GrupoResolutor"])),
                        
                        // Campos de seguimiento
                        Historial = GetCellValue(worksheet, row, columnMapping["Historial"]),
                        Avance = GetIntValue(worksheet, row, columnMapping["Avance"]),
                        VisorAplicativoAfectado = GetCellValue(worksheet, row, columnMapping["VisorAplicativoAfectado"]),
                        Problema = GetCellValue(worksheet, row, columnMapping["Problema"]),
                        DetalleProblema = GetCellValue(worksheet, row, columnMapping["DetalleProblema"]),
                        QuienAtiende = GetCellValue(worksheet, row, columnMapping["QuienAtiende"]),
                        TiempoResolucion = GetIntValue(worksheet, row, columnMapping["TiempoResolucion"]),
                        FechaAckEquipoPrecargas = GetDateValue(worksheet, row, columnMapping["FechaAckEquipoPrecargas"]),
                        SolucionRemedy = GetCellValue(worksheet, row, columnMapping["SolucionRemedy"]),
                        Precarga = GetCellValue(worksheet, row, columnMapping["Precarga"]),
                        RfcSolicitudCambio = GetCellValue(worksheet, row, columnMapping["RfcSolicitudCambio"]),
                        CausaRaiz = GetCellValue(worksheet, row, columnMapping["CausaRaiz"]),
                        
                        // Estado por defecto
                        EstadoTareaId = await GetDefaultEstadoTareaId(),
                        FechaCreacion = DateTime.Now
                    };
                    
                    _context.Tareas.Add(tarea);
                    await _context.SaveChangesAsync();
                    
                    // Cargar las relaciones para el DTO
                    await _context.Entry(tarea)
                        .Reference(t => t.GrupoAsignado)
                        .LoadAsync();
                    await _context.Entry(tarea)
                        .Reference(t => t.Prioridad)
                        .LoadAsync();
                    await _context.Entry(tarea)
                        .Reference(t => t.Estatus)
                        .LoadAsync();
                    await _context.Entry(tarea)
                        .Reference(t => t.Criticidad)
                        .LoadAsync();
                    await _context.Entry(tarea)
                        .Reference(t => t.TipoQueja)
                        .LoadAsync();
                    await _context.Entry(tarea)
                        .Reference(t => t.Origen)
                        .LoadAsync();
                    await _context.Entry(tarea)
                        .Reference(t => t.Categoria)
                        .LoadAsync();
                    await _context.Entry(tarea)
                        .Reference(t => t.GrupoResolutor)
                        .LoadAsync();
                    await _context.Entry(tarea)
                        .Reference(t => t.EstadoTarea)
                        .LoadAsync();
                    
                    tareasImportadas.Add(MapToDto(tarea));
                }
                catch (Exception ex)
                {
                    // Log error pero continúa con la siguiente fila
                    Console.WriteLine($"Error importando fila {row}: {ex.Message}");
                }
            }
            
            return tareasImportadas;
        }
        
        public async Task<byte[]> ExportarTareasToExcel(List<TareaDto> tareas)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Tareas");
            
            // Encabezados
            var headers = new[]
            {
                "ID Incidencia", "ID Petición", "Detalle Descripción", "Grupo Asignado", "Prioridad",
                "Estatus", "Fecha Asignación", "Fecha Solución", "Apellidos", "Nombre", "Criticidad",
                "Tipo Queja", "Origen", "Categoría", "Grupo Resolutor", "Historial", "Avance",
                "Visor/Aplicativo Afectado", "Problema", "Detalle del Problema", "Quien Atiende",
                "Tiempo Resolución", "Fecha ACK Equipo Precargas", "Solución Remedy", "Precarga",
                "RFC o Solicitud de Cambio", "Causa Raíz", "Sprint", "Estado Tarea", "Fecha Creación"
            };
            
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            }
            
            // Datos
            for (int i = 0; i < tareas.Count; i++)
            {
                var tarea = tareas[i];
                var row = i + 2;
                
                worksheet.Cells[row, 1].Value = tarea.IdIncidencia;
                worksheet.Cells[row, 2].Value = tarea.IdPeticion;
                worksheet.Cells[row, 3].Value = tarea.DetalleDescripcion;
                worksheet.Cells[row, 4].Value = tarea.GrupoAsignadoNombre;
                worksheet.Cells[row, 5].Value = tarea.PrioridadNombre;
                worksheet.Cells[row, 6].Value = tarea.EstatusNombre;
                worksheet.Cells[row, 7].Value = tarea.FechaAsignacion?.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 8].Value = tarea.FechaSolucion?.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 9].Value = tarea.Apellidos;
                worksheet.Cells[row, 10].Value = tarea.Nombre;
                worksheet.Cells[row, 11].Value = tarea.CriticidadNombre;
                worksheet.Cells[row, 12].Value = tarea.TipoQuejaNombre;
                worksheet.Cells[row, 13].Value = tarea.OrigenNombre;
                worksheet.Cells[row, 14].Value = tarea.CategoriaNombre;
                worksheet.Cells[row, 15].Value = tarea.GrupoResolutorNombre;
                worksheet.Cells[row, 16].Value = tarea.Historial;
                worksheet.Cells[row, 17].Value = tarea.Avance;
                worksheet.Cells[row, 18].Value = tarea.VisorAplicativoAfectado;
                worksheet.Cells[row, 19].Value = tarea.Problema;
                worksheet.Cells[row, 20].Value = tarea.DetalleProblema;
                worksheet.Cells[row, 21].Value = tarea.QuienAtiende;
                worksheet.Cells[row, 22].Value = tarea.TiempoResolucion;
                worksheet.Cells[row, 23].Value = tarea.FechaAckEquipoPrecargas?.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 24].Value = tarea.SolucionRemedy;
                worksheet.Cells[row, 25].Value = tarea.Precarga;
                worksheet.Cells[row, 26].Value = tarea.RfcSolicitudCambio;
                worksheet.Cells[row, 27].Value = tarea.CausaRaiz;
                worksheet.Cells[row, 28].Value = tarea.SprintNombre;
                worksheet.Cells[row, 29].Value = tarea.EstadoTareaNombre;
                worksheet.Cells[row, 30].Value = tarea.FechaCreacion.ToString("yyyy-MM-dd HH:mm");
            }
            
            // Auto ajustar columnas
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            
            return package.GetAsByteArray();
        }
        
        private string? GetCellValue(ExcelWorksheet worksheet, int row, int col)
        {
            var cell = worksheet.Cells[row, col];
            return cell?.Value?.ToString()?.Trim();
        }
        
        private DateTime? GetDateValue(ExcelWorksheet worksheet, int row, int col)
        {
            var cellValue = GetCellValue(worksheet, row, col);
            if (string.IsNullOrEmpty(cellValue))
                return null;
                
            if (DateTime.TryParse(cellValue, out var date))
                return date;
                
            return null;
        }
        
        private int? GetIntValue(ExcelWorksheet worksheet, int row, int col)
        {
            var cellValue = GetCellValue(worksheet, row, col);
            if (string.IsNullOrEmpty(cellValue))
                return null;
                
            if (int.TryParse(cellValue, out var value))
                return value;
                
            return null;
        }
        
        private async Task<int?> GetOrCreateCatalogoId(string tipo, string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;
                
            var catalogo = await _context.Catalogos
                .FirstOrDefaultAsync(c => c.Tipo == tipo && c.Valor == valor && c.Activo);
                
            if (catalogo == null)
            {
                catalogo = new Catalogo
                {
                    Tipo = tipo,
                    Valor = valor,
                    Activo = true,
                    FechaCreacion = DateTime.Now
                };
                
                _context.Catalogos.Add(catalogo);
                await _context.SaveChangesAsync();
            }
            
            return catalogo.Id;
        }
        
        private async Task<int> GetDefaultEstadoTareaId()
        {
            var estado = await _context.Catalogos
                .Where(c => c.Tipo == "EstadoTarea" && c.Valor == "Por Hacer" && c.Activo)
                .FirstOrDefaultAsync();
            return estado?.Id ?? 1;
        }
        
        private static TareaDto MapToDto(Tarea tarea)
        {
            return new TareaDto
            {
                Id = tarea.Id,
                IdIncidencia = tarea.IdIncidencia,
                IdPeticion = tarea.IdPeticion,
                DetalleDescripcion = tarea.DetalleDescripcion,
                GrupoAsignadoId = tarea.GrupoAsignadoId,
                GrupoAsignadoNombre = tarea.GrupoAsignado?.Valor,
                PrioridadId = tarea.PrioridadId,
                PrioridadNombre = tarea.Prioridad?.Valor,
                EstatusId = tarea.EstatusId,
                EstatusNombre = tarea.Estatus?.Valor,
                FechaAsignacion = tarea.FechaAsignacion,
                FechaSolucion = tarea.FechaSolucion,
                Apellidos = tarea.Apellidos,
                Nombre = tarea.Nombre,
                CriticidadId = tarea.CriticidadId,
                CriticidadNombre = tarea.Criticidad?.Valor,
                TipoQuejaId = tarea.TipoQuejaId,
                TipoQuejaNombre = tarea.TipoQueja?.Valor,
                OrigenId = tarea.OrigenId,
                OrigenNombre = tarea.Origen?.Valor,
                CategoriaId = tarea.CategoriaId,
                CategoriaNombre = tarea.Categoria?.Valor,
                GrupoResolutorId = tarea.GrupoResolutorId,
                GrupoResolutorNombre = tarea.GrupoResolutor?.Valor,
                Historial = tarea.Historial,
                Avance = tarea.Avance,
                VisorAplicativoAfectado = tarea.VisorAplicativoAfectado,
                Problema = tarea.Problema,
                DetalleProblema = tarea.DetalleProblema,
                QuienAtiende = tarea.QuienAtiende,
                TiempoResolucion = tarea.TiempoResolucion,
                FechaAckEquipoPrecargas = tarea.FechaAckEquipoPrecargas,
                SolucionRemedy = tarea.SolucionRemedy,
                Precarga = tarea.Precarga,
                RfcSolicitudCambio = tarea.RfcSolicitudCambio,
                CausaRaiz = tarea.CausaRaiz,
                SprintId = tarea.SprintId,
                SprintNombre = tarea.Sprint?.Nombre,
                EstadoTareaId = tarea.EstadoTareaId,
                EstadoTareaNombre = tarea.EstadoTarea?.Valor,
                FechaCreacion = tarea.FechaCreacion,
                FechaActualizacion = tarea.FechaActualizacion
            };
        }
    }
}
