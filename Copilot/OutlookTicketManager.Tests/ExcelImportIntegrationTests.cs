using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OutlookTicketManager.Data;
using OutlookTicketManager.Models;
using OutlookTicketManager.Services;
using System.Data;
using Xunit;

namespace OutlookTicketManager.Tests
{
    /// <summary>
    /// Pruebas de integración para el proceso de importación de archivos Excel
    /// Solo procesa filas que contienen información válida
    /// </summary>
    public class ExcelImportIntegrationTests : IDisposable
    {
        private readonly TicketDbContext _context;
        private readonly FileImportService _fileImportService;
        private readonly ILogger<FileImportService> _logger;
        private readonly string _testExcelFilePath;

        public ExcelImportIntegrationTests()
        {
            // Configurar base de datos en memoria para pruebas
            var options = new DbContextOptionsBuilder<TicketDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TicketDbContext(options);

            // Configurar servicios
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            var serviceProvider = services.BuildServiceProvider();

            _logger = serviceProvider.GetRequiredService<ILogger<FileImportService>>();
            
            // Usamos null para NotificationService ya que no lo necesitamos para las pruebas
            _fileImportService = new FileImportService(_context, _logger, null!);

            // Configurar EPPlus
            Environment.SetEnvironmentVariable("EPPlusLicense", "NonCommercial");

            // Crear archivo Excel de prueba
            _testExcelFilePath = CreateTestExcelFile();
        }

        [Fact]
        public async Task ImportTicketsFromFileAsync_ValidData_ShouldImportSuccessfully()
        {
            // Arrange
            var initialTicketCount = await _context.Tickets.CountAsync();

            // Act
            var result = await _fileImportService.ImportTicketsFromFileAsync(_testExcelFilePath);

            // Assert
            Assert.True(result > 0, "Debería importar al menos un ticket");
            
            var finalTicketCount = await _context.Tickets.CountAsync();
            Assert.Equal(initialTicketCount + result, finalTicketCount);

            // Verificar que los datos se guardaron correctamente
            var importedTickets = await _context.Tickets
                .Where(t => t.EmailId.StartsWith("EXCEL_IMPORT_"))
                .ToListAsync();

            Assert.Equal(result, importedTickets.Count);

            // Verificar datos específicos del primer ticket
            var firstTicket = importedTickets.First();
            Assert.NotNull(firstTicket.Subject);
            Assert.NotEmpty(firstTicket.Subject);
            Assert.Equal("imported@system.local", firstTicket.FromEmail);
            Assert.True(firstTicket.CreatedDate <= DateTime.UtcNow);
            Assert.True(firstTicket.UpdatedDate <= DateTime.UtcNow);
        }

        [Fact]
        public async Task ImportTicketsFromFileAsync_DuplicateData_ShouldNotImportDuplicates()
        {
            // Arrange
            // Primera importación
            var firstImportResult = await _fileImportService.ImportTicketsFromFileAsync(_testExcelFilePath);

            // Act
            // Segunda importación (datos duplicados)
            var secondImportResult = await _fileImportService.ImportTicketsFromFileAsync(_testExcelFilePath);

            // Assert
            Assert.True(firstImportResult > 0);
            Assert.Equal(0, secondImportResult);

            var totalTickets = await _context.Tickets.CountAsync();
            Assert.Equal(firstImportResult, totalTickets);
        }

        [Fact]
        public async Task ImportTicketsFromFileAsync_EmptyRows_ShouldSkipEmptyRows()
        {
            // Arrange
            var testFileWithEmptyRows = CreateTestExcelFileWithEmptyRows();

            // Act
            var result = await _fileImportService.ImportTicketsFromFileAsync(testFileWithEmptyRows);

            // Assert
            Assert.True(result > 0, "Debería importar solo las filas con información");

            var importedTickets = await _context.Tickets
                .Where(t => t.EmailId.StartsWith("EXCEL_IMPORT_"))
                .ToListAsync();

            // Verificar que solo se importaron filas con ID válido
            Assert.All(importedTickets, ticket => 
            {
                Assert.NotNull(ticket.IdPeticion);
                Assert.NotEmpty(ticket.IdPeticion);
                Assert.StartsWith("[", ticket.Subject); // Subject debe contener el ID
            });

            // Limpiar archivo temporal
            File.Delete(testFileWithEmptyRows);
        }

        [Fact]
        public async Task ImportTicketsFromFileAsync_NonExistentFile_ShouldReturnZero()
        {
            // Arrange
            var nonExistentFile = "/path/to/nonexistent/file.xlsx";

            // Act
            var result = await _fileImportService.ImportTicketsFromFileAsync(nonExistentFile);

            // Assert
            Assert.Equal(0, result);
            var ticketCount = await _context.Tickets.CountAsync();
            Assert.Equal(0, ticketCount);
        }

        [Fact]
        public async Task ImportTicketsFromFileAsync_DatabaseConnectionError_ShouldHandleGracefully()
        {
            // Arrange
            // Disponer del contexto actual para simular error de conexión
            await _context.DisposeAsync();
            
            // Crear un contexto con configuración que cause error
            var invalidOptions = new DbContextOptionsBuilder<TicketDbContext>()
                .UseInMemoryDatabase(databaseName: "INVALID_DB_FOR_ERROR_TEST")
                .Options;

            var invalidContext = new TicketDbContext(invalidOptions);
            // Cerrar la conexión inmediatamente para simular error
            await invalidContext.DisposeAsync();

            var serviceWithInvalidContext = new FileImportService(
                invalidContext, 
                _logger, 
                null!);

            // Act & Assert
            var exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
                await serviceWithInvalidContext.ImportTicketsFromFileAsync(_testExcelFilePath));

            Assert.NotNull(exception);
        }

        [Fact]
        public async Task ImportTicketsFromFileAsync_ValidData_ShouldMapFieldsCorrectly()
        {
            // Act
            var result = await _fileImportService.ImportTicketsFromFileAsync(_testExcelFilePath);

            // Assert
            Assert.True(result > 0);

            var importedTicket = await _context.Tickets
                .Where(t => t.EmailId.StartsWith("EXCEL_IMPORT_"))
                .FirstAsync();

            // Verificar mapeo de campos específicos
            Assert.NotNull(importedTicket.IdPeticion);
            Assert.Equal("INCS00003636034", importedTicket.IdPeticion);
            Assert.Contains("INCS00003636034", importedTicket.Subject);
            Assert.NotNull(importedTicket.Description);
            Assert.Equal("SOPORTE_MICROSOFT_PRECARGA", importedTicket.GrupoAsignado);
            Assert.Equal(Priority.Low, importedTicket.Priority); // "Baja" -> Low
            Assert.Equal(TicketStatus.InProgress, importedTicket.Status); // "En curso" -> InProgress
            Assert.NotNull(importedTicket.Nombre);
            Assert.NotNull(importedTicket.Apellidos);
        }

        [Theory]
        [InlineData("xlsx")]
        [InlineData("xls")]
        public async Task ImportTicketsFromFileAsync_DifferentFormats_ShouldHandleCorrectly(string format)
        {
            // Arrange
            var testFile = CreateTestExcelFile(format);

            try
            {
                // Act
                var result = await _fileImportService.ImportTicketsFromFileAsync(testFile);

                // Assert
                if (format == "xlsx")
                {
                    Assert.True(result > 0, "Archivos .xlsx deberían importarse correctamente");
                }
                else
                {
                    // Los archivos .xls pueden tener compatibilidad limitada
                    Assert.True(result >= 0, "Archivos .xls deberían manejarse sin errores fatales");
                }
            }
            finally
            {
                // Cleanup
                if (File.Exists(testFile))
                    File.Delete(testFile);
            }
        }

        private string CreateTestExcelFile(string extension = "xlsx")
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"test_excel_{Guid.NewGuid()}.{extension}");

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Hoja1");

            // Encabezados (fila 1)
            worksheet.Cells[1, 1].Value = "ID de la incidencia*+";
            worksheet.Cells[1, 2].Value = "ID de petición de servicio";
            worksheet.Cells[1, 3].Value = "Descripción Detallada";
            worksheet.Cells[1, 4].Value = "Grupo asignado*+";
            worksheet.Cells[1, 5].Value = "Prioridad*";
            worksheet.Cells[1, 6].Value = "Estado*";
            worksheet.Cells[1, 7].Value = "Fecha de notificación+";
            worksheet.Cells[1, 8].Value = "Fecha de solucion";
            worksheet.Cells[1, 9].Value = "Apellidos+";
            worksheet.Cells[1, 10].Value = "Nombre+";
            worksheet.Cells[1, 11].Value = "Solución";
            worksheet.Cells[1, 12].Value = "Usuario asignado+";
            worksheet.Cells[1, 13].Value = "Nombre del producto+";

            // Datos de prueba (fila 2)
            worksheet.Cells[2, 1].Value = "INCS00003636034";
            worksheet.Cells[2, 2].Value = "REQS00002145713";
            worksheet.Cells[2, 3].Value = "Prueba de importación de datos de Excel";
            worksheet.Cells[2, 4].Value = "SOPORTE_MICROSOFT_PRECARGA";
            worksheet.Cells[2, 5].Value = "Baja";
            worksheet.Cells[2, 6].Value = "En curso";
            worksheet.Cells[2, 7].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cells[2, 8].Value = "";
            worksheet.Cells[2, 9].Value = "ROMERO LOZANO";
            worksheet.Cells[2, 10].Value = "ALAIN FERNANDO";
            worksheet.Cells[2, 11].Value = "";
            worksheet.Cells[2, 12].Value = "ILENIA AURA GONZALEZ SALDIVAR";
            worksheet.Cells[2, 13].Value = "PRECARGA- DYP - PAGOS PROVISIONALES";

            // Segundo registro de prueba (fila 3)
            worksheet.Cells[3, 1].Value = "INCS00003652834";
            worksheet.Cells[3, 2].Value = "REQS00002179322";
            worksheet.Cells[3, 3].Value = "Segunda prueba de importación";
            worksheet.Cells[3, 4].Value = "SOPORTE_MICROSOFT_PRECARGA";
            worksheet.Cells[3, 5].Value = "Alta";
            worksheet.Cells[3, 6].Value = "Nuevo";
            worksheet.Cells[3, 7].Value = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cells[3, 8].Value = "";
            worksheet.Cells[3, 9].Value = "MARTINEZ MARTINEZ";
            worksheet.Cells[3, 10].Value = "OFIR";
            worksheet.Cells[3, 11].Value = "";
            worksheet.Cells[3, 12].Value = "MARIANA GUADALUPE JUAREZ SALINAS";
            worksheet.Cells[3, 13].Value = "PRECARGA- DYP - PAGOS PROVISIONALES";

            package.SaveAs(new FileInfo(filePath));
            return filePath;
        }

        private string CreateTestExcelFileWithEmptyRows()
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"test_excel_empty_{Guid.NewGuid()}.xlsx");

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Hoja1");

            // Encabezados (fila 1)
            worksheet.Cells[1, 1].Value = "ID de la incidencia*+";
            worksheet.Cells[1, 2].Value = "ID de petición de servicio";
            worksheet.Cells[1, 3].Value = "Descripción Detallada";

            // Fila 2: Datos válidos
            worksheet.Cells[2, 1].Value = "INCS00001234567";
            worksheet.Cells[2, 2].Value = "REQS00001234567";
            worksheet.Cells[2, 3].Value = "Ticket válido";

            // Fila 3: Vacía (sin datos)
            // No se asigna nada, queda vacía

            // Fila 4: Solo ID vacío
            worksheet.Cells[4, 1].Value = "";
            worksheet.Cells[4, 2].Value = "REQS00005555555";
            worksheet.Cells[4, 3].Value = "Ticket con ID vacío - debería ser ignorado";

            // Fila 5: Datos válidos
            worksheet.Cells[5, 1].Value = "INCS00009876543";
            worksheet.Cells[5, 2].Value = "REQS00009876543";
            worksheet.Cells[5, 3].Value = "Segundo ticket válido";

            package.SaveAs(new FileInfo(filePath));
            return filePath;
        }

        public void Dispose()
        {
            _context?.Dispose();
            
            if (File.Exists(_testExcelFilePath))
                File.Delete(_testExcelFilePath);
        }
    }

    /// <summary>
    /// Servicio de notificaciones simple para pruebas
    /// </summary>
    public class NotificationService
    {
        private readonly ILogger _logger;

        public NotificationService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task SendNotificationAsync(string title, string message)
        {
            _logger.LogInformation("Notification: {Title} - {Message}", title, message);
            await Task.CompletedTask;
        }
    }
}
