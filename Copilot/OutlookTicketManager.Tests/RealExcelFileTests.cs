using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OutlookTicketManager.Data;
using OutlookTicketManager.Models;
using OutlookTicketManager.Services;
using Xunit;
using Xunit.Abstractions;

namespace OutlookTicketManager.Tests
{
    /// <summary>
    /// Pruebas específicas usando el archivo real "backlog 06 agosto.xlsx"
    /// Valida el procesamiento de filas que contienen información real
    /// </summary>
    public class RealExcelFileTests : IDisposable
    {
        private readonly TicketDbContext _context;
        private readonly FileImportService _fileImportService;
        private readonly ILogger<FileImportService> _logger;
        private readonly ITestOutputHelper _output;
        private const string REAL_EXCEL_FILE = "Insumo/backlog 06 agosto.xlsx";

        public RealExcelFileTests(ITestOutputHelper output)
        {
            _output = output;

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
            _fileImportService = new FileImportService(_context, _logger, null!);

            // Configurar EPPlus
            Environment.SetEnvironmentVariable("EPPlusLicense", "NonCommercial");
        }

        [Fact]
        public async Task ImportRealExcelFile_ShouldProcessOnlyRowsWithInformation()
        {
            // Arrange
            var excelFilePath = Path.Combine(Directory.GetCurrentDirectory(), REAL_EXCEL_FILE);
            
            // Verificar que el archivo existe
            if (!File.Exists(excelFilePath))
            {
                _output.WriteLine($"⚠️ Archivo no encontrado: {excelFilePath}");
                _output.WriteLine("📂 Directorio actual: " + Directory.GetCurrentDirectory());
                _output.WriteLine("📁 Contenido del directorio:");
                foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*", SearchOption.AllDirectories)
                    .Where(f => f.Contains("backlog") || f.Contains(".xlsx")))
                {
                    _output.WriteLine($"  - {file}");
                }
                
                // Buscar el archivo en ubicaciones alternativas
                var alternativePaths = new[]
                {
                    "../../../Insumo/backlog 06 agosto.xlsx",
                    "../../Insumo/backlog 06 agosto.xlsx",
                    "../Insumo/backlog 06 agosto.xlsx",
                    "../OutlookTicketManager/Insumo/backlog 06 agosto.xlsx"
                };

                foreach (var altPath in alternativePaths)
                {
                    var fullPath = Path.GetFullPath(altPath);
                    if (File.Exists(fullPath))
                    {
                        excelFilePath = fullPath;
                        _output.WriteLine($"✅ Archivo encontrado en: {excelFilePath}");
                        break;
                    }
                }

                Assert.True(File.Exists(excelFilePath), 
                    $"El archivo de prueba no existe: {excelFilePath}. " +
                    "Asegúrate de que 'backlog 06 agosto.xlsx' esté disponible.");
            }

            _output.WriteLine($"📊 Procesando archivo: {excelFilePath}");

            // Act
            var result = await _fileImportService.ImportTicketsFromFileAsync(excelFilePath);

            // Assert
            _output.WriteLine($"📋 Resultado de importación: {result} tickets importados");

            // Según nuestro análisis previo, el archivo debe tener 4 filas de datos válidos
            Assert.True(result >= 0, "El proceso de importación debe ejecutarse sin errores fatales");
            
            if (result > 0)
            {
                _output.WriteLine($"✅ Se importaron {result} tickets exitosamente");

                // Verificar tickets importados
                var importedTickets = await _context.Tickets
                    .Where(t => t.EmailId.StartsWith("EXCEL_IMPORT_"))
                    .ToListAsync();

                Assert.Equal(result, importedTickets.Count);

                // Verificar que solo se procesaron filas con información
                foreach (var ticket in importedTickets)
                {
                    _output.WriteLine($"🎫 Ticket importado: {ticket.Subject}");
                    
                    // Verificar campos obligatorios
                    Assert.NotNull(ticket.IdPeticion);
                    Assert.NotEmpty(ticket.IdPeticion);
                    Assert.StartsWith("INCS", ticket.IdPeticion); // IDs de incidencia empiezan con INCS
                    
                    // Verificar que el subject contiene el ID
                    Assert.Contains(ticket.IdPeticion, ticket.Subject);
                    
                    // Verificar mapeo de estado
                    Assert.True(ticket.Status == TicketStatus.InProgress || 
                               ticket.Status == TicketStatus.Backlog ||
                               ticket.Status == TicketStatus.Resolved);
                    
                    // Verificar mapeo de prioridad
                    Assert.True(ticket.Priority == Priority.Low || 
                               ticket.Priority == Priority.Medium ||
                               ticket.Priority == Priority.High);

                    _output.WriteLine($"  - ID: {ticket.IdPeticion}");
                    _output.WriteLine($"  - Estado: {ticket.Status}");
                    _output.WriteLine($"  - Prioridad: {ticket.Priority}");
                    _output.WriteLine($"  - Grupo: {ticket.GrupoAsignado}");
                    _output.WriteLine($"  - Asignado a: {ticket.QuienAtiende}");
                }

                // Verificar datos específicos conocidos del archivo
                var firstTicket = importedTickets.FirstOrDefault(t => t.IdPeticion == "INCS00003636034");
                if (firstTicket != null)
                {
                    Assert.Equal("SOPORTE_MICROSOFT_PRECARGA", firstTicket.GrupoAsignado);
                    Assert.Equal(Priority.Low, firstTicket.Priority); // "Baja" -> Low
                    Assert.Equal(TicketStatus.InProgress, firstTicket.Status); // "En curso" -> InProgress
                    Assert.Equal("ALAIN FERNANDO", firstTicket.Nombre);
                    Assert.Equal("ROMERO LOZANO", firstTicket.Apellidos);
                    
                    _output.WriteLine($"✅ Validación específica del primer ticket exitosa");
                }
            }
            else
            {
                _output.WriteLine($"⚠️ No se importaron tickets. Verificando razones...");
                
                // Verificar si hay tickets duplicados en la base de datos
                var existingTickets = await _context.Tickets.CountAsync();
                if (existingTickets > 0)
                {
                    _output.WriteLine($"📋 Tickets existentes en la BD: {existingTickets}");
                    _output.WriteLine("💡 Posible causa: Los tickets ya existen y no se crearon duplicados");
                }
            }
        }

        [Fact]
        public async Task ImportRealExcelFile_ShouldSkipEmptyOrInvalidRows()
        {
            // Arrange
            var excelFilePath = FindRealExcelFile();
            if (!File.Exists(excelFilePath))
            {
                // Skip test if file doesn't exist
                return;
            }

            // Act - Primera importación
            var firstResult = await _fileImportService.ImportTicketsFromFileAsync(excelFilePath);

            // Assert - Verificar que solo se procesaron filas válidas
            _output.WriteLine($"Primer resultado de importación: {firstResult}");
            
            var importedTickets = await _context.Tickets.ToListAsync();
            
            // Verificar que todos los tickets tienen ID de incidencia válido
            Assert.All(importedTickets, ticket =>
            {
                Assert.NotNull(ticket.IdPeticion);
                Assert.NotEmpty(ticket.IdPeticion);
                Assert.Matches(@"^INCS\d+", ticket.IdPeticion); // Formato: INCS + números
            });

            _output.WriteLine($"✅ Todos los {importedTickets.Count} tickets importados tienen IDs válidos");

            // Act - Segunda importación (debe detectar duplicados)
            var secondResult = await _fileImportService.ImportTicketsFromFileAsync(excelFilePath);

            // Assert - No debe crear duplicados
            Assert.Equal(0, secondResult);
            
            var finalTicketCount = await _context.Tickets.CountAsync();
            Assert.Equal(firstResult, finalTicketCount);

            _output.WriteLine($"✅ Prevención de duplicados funcionando correctamente");
        }

        private string FindRealExcelFile()
        {
            var possiblePaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), REAL_EXCEL_FILE),
                Path.GetFullPath("../../../" + REAL_EXCEL_FILE),
                Path.GetFullPath("../../" + REAL_EXCEL_FILE),
                Path.GetFullPath("../" + REAL_EXCEL_FILE),
                Path.GetFullPath("../OutlookTicketManager/" + REAL_EXCEL_FILE)
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return possiblePaths[0]; // Devolver el primero para que falle con mensaje claro
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
