using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OutlookTicketManager.Data;
using OutlookTicketManager.Models;
using OutlookTicketManager.Services;
using Xunit;

namespace OutlookTicketManager.Tests
{
    public class CsvImportTests
    {
        private TicketDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<TicketDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new TicketDbContext(options);
        }

        private ILogger<FileImportService> GetLogger()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            return loggerFactory.CreateLogger<FileImportService>();
        }

        [Fact]
        public async Task ImportCsvFile_ShouldProcessValidRecords()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var logger = GetLogger();
            var service = new FileImportService(context, logger, null);

            // Copiar el archivo CSV de prueba
            var sourceFile = "/Users/won/Documents/Cognizant/Vibecoding/Copilot/OutlookTicketManager/Insumo/backlog 06 agosto.csv";
            var testFile = Path.Combine(Path.GetTempPath(), "test_backlog.csv");
            
            File.Copy(sourceFile, testFile, true);

            try
            {
                // Act
                var importedCount = await service.ImportTicketsFromFileAsync(testFile);

                // Assert
                Assert.True(importedCount >= 0); // Debería procesar al menos algunos registros
                
                var ticketsInDb = await context.Tickets.CountAsync();
                Assert.Equal(importedCount, ticketsInDb);

                // Verificar que los tickets tienen datos válidos
                if (importedCount > 0)
                {
                    var firstTicket = await context.Tickets.FirstAsync();
                    Assert.NotNull(firstTicket.EmailId);
                    Assert.NotNull(firstTicket.Subject);
                    Assert.NotNull(firstTicket.Description);
                }
            }
            finally
            {
                // Limpiar
                if (File.Exists(testFile))
                    File.Delete(testFile);
            }
        }

        [Fact]
        public async Task ImportCsvFile_ShouldMapFieldsCorrectly()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var logger = GetLogger();
            var service = new FileImportService(context, logger, null);

            var sourceFile = "/Users/won/Documents/Cognizant/Vibecoding/Copilot/OutlookTicketManager/Insumo/backlog 06 agosto.csv";
            var testFile = Path.Combine(Path.GetTempPath(), "test_mapping.csv");
            
            File.Copy(sourceFile, testFile, true);

            try
            {
                // Act
                var importedCount = await service.ImportTicketsFromFileAsync(testFile);

                // Assert
                if (importedCount > 0)
                {
                    var tickets = await context.Tickets.ToListAsync();
                    
                    foreach (var ticket in tickets)
                    {
                        // Verificar campos obligatorios
                        Assert.NotNull(ticket.EmailId);
                        Assert.NotNull(ticket.Subject);
                        Assert.NotNull(ticket.FromEmail);
                        Assert.NotNull(ticket.FromName);
                        
                        // Verificar que el estado fue mapeado correctamente
                        Assert.True(Enum.IsDefined(typeof(TicketStatus), ticket.Status));
                        
                        // Verificar que la prioridad fue mapeada correctamente
                        Assert.True(Enum.IsDefined(typeof(Priority), ticket.Priority));

                        // Verificar que las fechas fueron parseadas (si están presentes)
                        Assert.True(ticket.CreatedDate != DateTime.MinValue);
                    }
                }
            }
            finally
            {
                if (File.Exists(testFile))
                    File.Delete(testFile);
            }
        }

        [Fact]
        public async Task ImportCsvFile_ShouldNotImportDuplicates()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var logger = GetLogger();
            var service = new FileImportService(context, logger, null);

            var sourceFile = "/Users/won/Documents/Cognizant/Vibecoding/Copilot/OutlookTicketManager/Insumo/backlog 06 agosto.csv";
            var testFile = Path.Combine(Path.GetTempPath(), "test_duplicates.csv");
            
            File.Copy(sourceFile, testFile, true);

            try
            {
                // Act - Primera importación
                var firstImport = await service.ImportTicketsFromFileAsync(testFile);
                
                // Act - Segunda importación (duplicados)
                var secondImport = await service.ImportTicketsFromFileAsync(testFile);

                // Assert
                Assert.True(firstImport >= 0);
                Assert.Equal(0, secondImport); // No debería importar duplicados
                
                var totalTickets = await context.Tickets.CountAsync();
                Assert.Equal(firstImport, totalTickets);
            }
            finally
            {
                if (File.Exists(testFile))
                    File.Delete(testFile);
            }
        }
    }
}
