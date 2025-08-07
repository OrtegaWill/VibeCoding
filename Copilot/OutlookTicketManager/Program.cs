using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using OutlookTicketManager.Data;
using OutlookTicketManager.Services;
using OutlookTicketManager.Hubs;
using OutlookTicketManager.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Configure Entity Framework with SQLite
builder.Services.AddDbContext<TicketDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                     "Data Source=tickets.db"));

// Add custom services
builder.Services.AddScoped<OutlookServiceSimplified>();
builder.Services.AddScoped<EmailClassifierServiceSimplified>();
builder.Services.AddScoped<TicketManagerService>();
builder.Services.AddScoped<OutlookTicketManager.Services.NotificationService>();

// Add SignalR
builder.Services.AddSignalR();

// Add logging
builder.Services.AddLogging();

// Add CORS for development (si necesario)
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });
}

var app = builder.Build();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
    context.Database.EnsureCreated();
    
    // Add sample data if database is empty
    if (!context.Tickets.Any())
    {
        // Sample Email Filters
        var filters = new List<EmailFilter>
        {
            new EmailFilter
            {
                Name = "Filtro Cognizant",
                FromDomain = "@cognizant.com",
                AutoCategory = "Consulta Interna",
                AutoPriority = Priority.Medium,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new EmailFilter
            {
                Name = "Bugs Críticos",
                SubjectKeywords = "bug,error,critical,urgente",
                AutoCategory = "Bug",
                AutoPriority = Priority.High,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new EmailFilter
            {
                Name = "Solicitudes de Soporte",
                SubjectKeywords = "soporte,ayuda,problema,issue",
                AutoCategory = "Soporte",
                AutoPriority = Priority.Medium,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            }
        };
        context.EmailFilters.AddRange(filters);
        
        // Sample Tickets
        var tickets = new List<Ticket>
        {
            new Ticket
            {
                EmailId = "sample-email-001",
                Subject = "Error en sistema de facturación",
                Description = "El sistema de facturación presenta errores al generar reportes mensuales. Los usuarios reportan que no pueden acceder a los documentos.",
                FromEmail = "juan.perez@cognizant.com",
                FromName = "Juan Pérez",
                Category = "Bug",
                Priority = Priority.High,
                Status = TicketStatus.InProgress,
                AssignedTo = "admin@company.com",
                Tags = "facturación,bug,urgente",
                CreatedDate = DateTime.UtcNow.AddDays(-3),
                UpdatedDate = DateTime.UtcNow.AddDays(-1),
                EstimatedHours = 8.5m,
                ActualHours = 4.0m
            },
            new Ticket
            {
                EmailId = "sample-email-002",
                Subject = "Solicitud de nuevo usuario",
                Description = "Se requiere crear una nueva cuenta de usuario para María González del departamento de ventas.",
                FromEmail = "hr@company.com",
                FromName = "Recursos Humanos",
                Category = "Solicitud",
                Priority = Priority.Medium,
                Status = TicketStatus.Backlog,
                Tags = "usuario,acceso",
                CreatedDate = DateTime.UtcNow.AddDays(-2),
                UpdatedDate = DateTime.UtcNow.AddDays(-2),
                EstimatedHours = 2.0m
            },
            new Ticket
            {
                EmailId = "sample-email-003",
                Subject = "Actualización de políticas de seguridad",
                Description = "Implementar nuevas políticas de seguridad según las directrices corporativas actualizadas.",
                FromEmail = "security@company.com",
                FromName = "Equipo de Seguridad",
                Category = "Mejora",
                Priority = Priority.Low,
                Status = TicketStatus.Resolved,
                AssignedTo = "admin@company.com",
                Tags = "seguridad,políticas,mejora",
                CreatedDate = DateTime.UtcNow.AddDays(-7),
                UpdatedDate = DateTime.UtcNow.AddDays(-1),
                EstimatedHours = 16.0m,
                ActualHours = 14.5m
            },
            new Ticket
            {
                EmailId = "sample-email-004",
                Subject = "Problema de conectividad en red",
                Description = "Varios usuarios reportan problemas intermitentes de conectividad a internet en el tercer piso.",
                FromEmail = "soporte@company.com",
                FromName = "Mesa de Ayuda",
                Category = "Soporte",
                Priority = Priority.High,
                Status = TicketStatus.InReview,
                AssignedTo = "techsupport@company.com",
                Tags = "red,conectividad,infraestructura",
                CreatedDate = DateTime.UtcNow.AddHours(-6),
                UpdatedDate = DateTime.UtcNow.AddHours(-2),
                EstimatedHours = 4.0m,
                ActualHours = 2.0m
            }
        };
        context.Tickets.AddRange(tickets);
        context.SaveChanges();
        
        // Sample Comments (after tickets are saved to get IDs)
        var comments = new List<TicketComment>
        {
            new TicketComment
            {
                TicketId = tickets[0].Id,
                Content = "Ticket asignado al equipo de desarrollo para investigación.",
                Author = "Sistema",
                AuthorEmail = "system@company.com",
                IsSystemComment = true,
                CreatedDate = DateTime.UtcNow.AddDays(-3)
            },
            new TicketComment
            {
                TicketId = tickets[0].Id,
                Content = "He identificado el problema. Está relacionado con la actualización reciente de la base de datos. Trabajando en la solución.",
                Author = "Admin Usuario",
                AuthorEmail = "admin@company.com",
                IsSystemComment = false,
                CreatedDate = DateTime.UtcNow.AddDays(-1)
            },
            new TicketComment
            {
                TicketId = tickets[3].Id,
                Content = "El problema ha sido escalado al equipo de infraestructura de red.",
                Author = "Tech Support",
                AuthorEmail = "techsupport@company.com",
                IsSystemComment = false,
                CreatedDate = DateTime.UtcNow.AddHours(-2)
            }
        };
        context.TicketComments.AddRange(comments);
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseCors();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Map SignalR hub
app.MapHub<TicketHub>("/tickethub");

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
