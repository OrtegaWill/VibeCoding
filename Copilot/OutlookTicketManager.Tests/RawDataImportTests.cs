using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using OutlookTicketManager.Data;
using OutlookTicketManager.Services;
using OutlookTicketManager.Models;
using System.Text;
using Xunit;
using Microsoft.Extensions.Logging;

namespace OutlookTicketManager.Tests;

// Simple NotificationService for testing that doesn't require SignalR
public class TestNotificationService : OutlookTicketManager.Services.NotificationService
{
    public TestNotificationService(ILogger<OutlookTicketManager.Services.NotificationService> logger) 
        : base(null!, logger)
    {
        // Pass null for IHubContext since we don't need SignalR in tests
    }

    // The methods will just log instead of sending real notifications
}

public class RawDataImportTests : IDisposable
{
    private readonly TicketDbContext _context;
    private readonly FileImportService _fileImportService;
    private readonly string _testFilePath;

    public RawDataImportTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<TicketDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new TicketDbContext(options);
        
        // Create logger and NotificationService for FileImportService
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<FileImportService>();
        var notificationLogger = loggerFactory.CreateLogger<OutlookTicketManager.Services.NotificationService>();
        
        // Create a test NotificationService since we don't need SignalR for tests
        var testNotificationService = new TestNotificationService(notificationLogger);
        
        _fileImportService = new FileImportService(_context, logger, testNotificationService);

        // Create test RawData CSV file
        _testFilePath = Path.GetTempFileName() + ".csv";
        CreateTestRawDataFile(_testFilePath);
    }

    private void CreateTestRawDataFile(string filePath)
    {
        var csvContent = """
"ID de la Incidencia","ID de la Petición","Detalle de la Descripción","Grupo Asignado","Prioridad","Estatus","Fecha Asignación","Fecha Solución","Apellidos","Nombre","Criticidad","Tipo Queja","Origen","Categoría","Grupo Resolutor","Historial","Avance","Visor / Aplicativo afectado","Problema","Detalle del Problema","Quien atiende?","Tiempo de resolución","Fecha ACK equipo de precargas","Solución Remedy","Precarga","RFC o Solicitud de Cambio","Causa Raíz"
"INC001","PET001","Error en la aplicación de facturación, no permite generar reportes mensuales","IT Support","Alta","Nuevo","2025-01-15","2025-01-16","Usuario","Prueba","Alta","Problema de Sistema","Correo","Software > Application","IT Support","Ticket creado por usuario","50%","Aplicación Facturación","Error de Reportes","No se pueden generar reportes","Juan Perez","2 horas","2025-01-15 10:30","Reiniciar servicio de reportes","Sí","CHG001","Error de configuración"
"INC002","PET002","Problema de conectividad en la red local del edificio 3","Network Team","Media","En Progreso","2025-01-15","2025-01-17","García","María","Media","Problema de Red","Teléfono","Infrastructure > Network","Network Team","En investigación","75%","Red Local","Conectividad","Sin acceso a internet","Ana Garcia","4 horas","2025-01-15 11:45","Revisar switch principal","No","CHG002","Falla de hardware"
""";
        File.WriteAllText(filePath, csvContent, Encoding.UTF8);
    }

    [Fact]
    public async Task ImportRawDataAsync_ShouldImportValidTickets()
    {
        // Act
        var result = await _fileImportService.ImportRawDataAsync(_testFilePath);

        // Assert
        Assert.Equal(2, result);
        
        var tickets = await _context.Tickets.ToListAsync();
        Assert.Equal(2, tickets.Count);

        // Verify first ticket
        var ticket1 = tickets.FirstOrDefault(t => t.EmailId == "INC001");
        Assert.NotNull(ticket1);
        Assert.Equal("Error en la aplicación de facturación, no permite generar reportes mensuales", ticket1.Subject);
        Assert.Equal("Prueba.Usuario@empresa.com", ticket1.FromEmail); // Generated from FirstName.LastName
        Assert.Equal(TicketStatus.Backlog, ticket1.Status); // "Nuevo" maps to Backlog
        Assert.Equal(Priority.High, ticket1.Priority); // "Alta" maps to High
        Assert.Equal("Juan Perez", ticket1.AssignedTo);
        Assert.Contains("Error de Reportes", ticket1.Body); // From "Problema" field
        Assert.Contains("Aplicación Facturación", ticket1.Body); // From "Visor / Aplicativo afectado"
        Assert.Contains("Reiniciar servicio de reportes", ticket1.Body); // From "Solución Remedy"

        // Verify second ticket
        var ticket2 = tickets.FirstOrDefault(t => t.EmailId == "INC002");
        Assert.NotNull(ticket2);
        Assert.Equal("Problema de conectividad en la red local del edificio 3", ticket2.Subject);
        Assert.Equal("María.García@empresa.com", ticket2.FromEmail); // Generated from FirstName.LastName
        Assert.Equal(TicketStatus.InProgress, ticket2.Status); // "En Progreso" maps to InProgress
        Assert.Equal(Priority.Medium, ticket2.Priority); // "Media" maps to Medium
        Assert.Equal("Ana Garcia", ticket2.AssignedTo);
    }

    [Fact]
    public async Task ImportRawDataAsync_ShouldHandleEmptyFile()
    {
        // Arrange
        var emptyFile = Path.GetTempFileName() + ".csv";
        File.WriteAllText(emptyFile, "\"ID de la Incidencia \",\"ID de la Petición\",\"Detalle de la Descripción\"\n", Encoding.UTF8);

        try
        {
            // Act
            var result = await _fileImportService.ImportRawDataAsync(emptyFile);

            // Assert
            Assert.Equal(0, result);
        }
        finally
        {
            File.Delete(emptyFile);
        }
    }

    [Fact]
    public async Task ImportRawDataAsync_ShouldPreventDuplicates()
    {
        // Act - Import twice
        var result1 = await _fileImportService.ImportRawDataAsync(_testFilePath);
        var result2 = await _fileImportService.ImportRawDataAsync(_testFilePath);

        // Assert
        Assert.Equal(2, result1); // First import should succeed
        Assert.Equal(0, result2); // Second import should be 0 (duplicates prevented)
        
        var tickets = await _context.Tickets.ToListAsync();
        Assert.Equal(2, tickets.Count); // Only 2 unique tickets
    }

    [Fact]
    public async Task ImportRawDataAsync_ShouldMapComplexFields()
    {
        // Act
        await _fileImportService.ImportRawDataAsync(_testFilePath);

        // Assert
        var ticket = await _context.Tickets.FirstAsync();
        
        // Verify that complex fields are properly mapped in the body
        Assert.Contains("**Historia del ticket:** Ticket creado por usuario", ticket.Body);
        Assert.Contains("**Categorización:** Software > Application", ticket.Body);
        Assert.Contains("**Aplicativo Afectado:** Aplicación Facturación", ticket.Body);
        Assert.Contains("**Solución propuesta:** Reiniciar servicio de reportes", ticket.Body);
        Assert.Contains("**Progreso:** 50%", ticket.Body);
        Assert.Contains("**Problema:** Error de Reportes", ticket.Body);
    }

    public void Dispose()
    {
        _context?.Dispose();
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }
}
