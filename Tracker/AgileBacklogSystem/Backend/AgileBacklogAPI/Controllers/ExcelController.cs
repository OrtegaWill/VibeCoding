using Microsoft.AspNetCore.Mvc;
using AgileBacklogAPI.Services;
using AgileBacklogAPI.DTOs;
using AgileBacklogAPI.Controllers;

namespace AgileBacklogAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExcelController : ControllerBase
    {
        private readonly ExcelService _excelService;
        private readonly TareasController _tareasController;
        
        public ExcelController(ExcelService excelService, TareasController tareasController)
        {
            _excelService = excelService;
            _tareasController = tareasController;
        }
        
        [HttpPost("import")]
        public async Task<ActionResult<ImportResult>> ImportTareasFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No se ha seleccionado ningún archivo");
                
            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Solo se permiten archivos Excel (.xlsx)");
            
            try
            {
                using var stream = file.OpenReadStream();
                var tareasImportadas = await _excelService.ImportarTareasFromExcel(stream);
                
                return Ok(new ImportResult
                {
                    Success = true,
                    TareasImportadas = tareasImportadas.Count,
                    Tareas = tareasImportadas,
                    Message = $"Se importaron {tareasImportadas.Count} tareas correctamente"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ImportResult
                {
                    Success = false,
                    Message = $"Error al importar el archivo: {ex.Message}"
                });
            }
        }
        
        [HttpGet("export")]
        public async Task<IActionResult> ExportTareasToExcel([FromQuery] int? sprintId = null)
        {
            try
            {
                var tareasResult = await _tareasController.GetTareas(sprintId);
                
                if (tareasResult.Result is not OkObjectResult okResult || 
                    okResult.Value is not IEnumerable<TareaDto> tareas)
                {
                    return BadRequest("No se pudieron obtener las tareas");
                }
                
                var tareasList = tareas.ToList();
                var excelData = await _excelService.ExportarTareasToExcel(tareasList);
                
                var fileName = sprintId.HasValue 
                    ? $"Tareas_Sprint_{sprintId}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
                    : $"Tareas_Backlog_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
                
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al exportar: {ex.Message}");
            }
        }
        
        [HttpGet("template")]
        public IActionResult DownloadTemplate()
        {
            try
            {
                var templateData = CreateExcelTemplate();
                var fileName = $"Template_Backlog_{DateTime.Now:yyyyMMdd}.xlsx";
                
                return File(templateData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al crear template: {ex.Message}");
            }
        }
        
        private byte[] CreateExcelTemplate()
        {
            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Template");
            
            // Encabezados principales
            var headers = new[]
            {
                "ID Incidencia", "ID Petición", "Detalle Descripción", "Grupo Asignado", "Prioridad",
                "Estatus", "Fecha Asignación", "Fecha Solución", "Apellidos", "Nombre", "Criticidad",
                "Tipo Queja", "Origen", "Categoría", "Grupo Resolutor", "Historial", "Avance",
                "Visor/Aplicativo Afectado", "Problema", "Detalle del Problema", "Quien Atiende",
                "Tiempo Resolución", "Fecha ACK Equipo Precargas", "Solución Remedy", "Precarga",
                "RFC o Solicitud de Cambio", "Causa Raíz"
            };
            
            // Agregar encabezados
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                
                // Colorear según especificación (amarillo = campos de alta, azul = campos de seguimiento)
                if (i < 15) // Campos de alta
                {
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                }
                else // Campos de seguimiento
                {
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }
            }
            
            // Agregar fila de ejemplo
            worksheet.Cells[2, 1].Value = "INC-001";
            worksheet.Cells[2, 2].Value = "REQ-001";
            worksheet.Cells[2, 3].Value = "Descripción de la tarea de ejemplo";
            worksheet.Cells[2, 4].Value = "Desarrollo";
            worksheet.Cells[2, 5].Value = "Alta";
            worksheet.Cells[2, 6].Value = "Nuevo";
            worksheet.Cells[2, 7].Value = DateTime.Now.ToString("yyyy-MM-dd");
            worksheet.Cells[2, 9].Value = "Apellido";
            worksheet.Cells[2, 10].Value = "Nombre";
            worksheet.Cells[2, 11].Value = "Alta";
            worksheet.Cells[2, 12].Value = "Bug";
            worksheet.Cells[2, 13].Value = "Usuario";
            worksheet.Cells[2, 14].Value = "Sistema";
            worksheet.Cells[2, 15].Value = "Desarrollo";
            
            // Auto ajustar columnas
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            
            return package.GetAsByteArray();
        }
    }
    
    public class ImportResult
    {
        public bool Success { get; set; }
        public int TareasImportadas { get; set; }
        public List<TareaDto> Tareas { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }
}
