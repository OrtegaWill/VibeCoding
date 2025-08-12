using Microsoft.EntityFrameworkCore;
using OutlookTicketManager.Models;

namespace OutlookTicketManager.Data
{
    /// <summary>
    /// Contexto de Entity Framework para la base de datos SQLite
    /// </summary>
    public class TicketDbContext : DbContext
    {
        public TicketDbContext(DbContextOptions<TicketDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Conjunto de datos de Tickets
        /// </summary>
        public DbSet<Ticket> Tickets { get; set; }

        /// <summary>
        /// Conjunto de datos de Comentarios de Tickets
        /// </summary>
        public DbSet<TicketComment> TicketComments { get; set; }

        /// <summary>
        /// Conjunto de datos de Filtros de Email
        /// </summary>
        public DbSet<EmailFilter> EmailFilters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración del modelo Ticket
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.EmailId).IsRequired();
                entity.Property(t => t.Subject).IsRequired().HasMaxLength(500);
                entity.Property(t => t.FromEmail).IsRequired().HasMaxLength(200);
                entity.Property(t => t.FromName).HasMaxLength(200);
                entity.Property(t => t.Category).HasMaxLength(100);
                entity.Property(t => t.AssignedTo).HasMaxLength(200);
                entity.Property(t => t.Tags).HasMaxLength(500);
                
                // Configuración de nuevos campos
                entity.Property(t => t.IdPeticion).HasMaxLength(100);
                entity.Property(t => t.GrupoAsignado).HasMaxLength(100);
                entity.Property(t => t.GrupoResolutor).HasMaxLength(100);
                entity.Property(t => t.TipoQueja).HasMaxLength(100);
                entity.Property(t => t.Origen).HasMaxLength(100);
                entity.Property(t => t.Apellidos).HasMaxLength(200);
                entity.Property(t => t.Nombre).HasMaxLength(200);
                entity.Property(t => t.QuienAtiende).HasMaxLength(200);
                entity.Property(t => t.VisorAplicativoAfectado).HasMaxLength(200);
                entity.Property(t => t.Problema).HasMaxLength(200);
                entity.Property(t => t.Precarga).HasMaxLength(200);
                entity.Property(t => t.RfcSolicitudCambio).HasMaxLength(100);
                
                // Conversión de enums a enteros
                entity.Property(t => t.Status).HasConversion<int>();
                entity.Property(t => t.Priority).HasConversion<int>();
                entity.Property(t => t.Criticidad).HasConversion<int?>();
                
                // Precisión para decimales
                entity.Property(t => t.EstimatedHours).HasColumnType("decimal(5,2)");
                entity.Property(t => t.ActualHours).HasColumnType("decimal(5,2)");
                entity.Property(t => t.TiempoResolucionHoras).HasColumnType("decimal(8,2)");
                entity.Property(t => t.Avance).HasColumnType("decimal(5,2)");
                
                // Índices para consultas eficientes
                entity.HasIndex(t => t.EmailId).IsUnique();
                entity.HasIndex(t => t.FromEmail);
                entity.HasIndex(t => t.Status);
                entity.HasIndex(t => t.Priority);
                entity.HasIndex(t => t.CreatedDate);
                entity.HasIndex(t => t.IdPeticion);
                entity.HasIndex(t => t.GrupoAsignado);
                entity.HasIndex(t => t.Criticidad);
            });

            // Configuración del modelo TicketComment
            modelBuilder.Entity<TicketComment>(entity =>
            {
                entity.HasKey(tc => tc.Id);
                entity.Property(tc => tc.Content).IsRequired();
                entity.Property(tc => tc.Author).IsRequired().HasMaxLength(200);
                entity.Property(tc => tc.AuthorEmail).HasMaxLength(200);
                
                // Relación con Ticket
                entity.HasOne(tc => tc.Ticket)
                      .WithMany(t => t.Comments)
                      .HasForeignKey(tc => tc.TicketId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasIndex(tc => tc.TicketId);
                entity.HasIndex(tc => tc.CreatedDate);
            });

            // Configuración del modelo EmailFilter
            modelBuilder.Entity<EmailFilter>(entity =>
            {
                entity.HasKey(ef => ef.Id);
                entity.Property(ef => ef.Name).IsRequired().HasMaxLength(200);
                entity.Property(ef => ef.FromEmail).HasMaxLength(200);
                entity.Property(ef => ef.FromDomain).HasMaxLength(100);
                entity.Property(ef => ef.AutoCategory).HasMaxLength(100);
                entity.Property(ef => ef.AutoAssignTo).HasMaxLength(200);
                
                // Conversión de enum nullable a entero nullable
                entity.Property(ef => ef.AutoPriority).HasConversion<int?>();
                
                entity.HasIndex(ef => ef.FromEmail);
                entity.HasIndex(ef => ef.FromDomain);
                entity.HasIndex(ef => ef.IsActive);
            });

            // Datos iniciales (Seed Data)
            SeedData(modelBuilder);
        }

        /// <summary>
        /// Configuración de datos iniciales para la aplicación
        /// </summary>
        private void SeedData(ModelBuilder modelBuilder)
        {
            // Filtros de ejemplo
            modelBuilder.Entity<EmailFilter>().HasData(
                new EmailFilter
                {
                    Id = 1,
                    Name = "Filtro Cognizant",
                    FromDomain = "@cognizant.com",
                    AutoCategory = "Consulta Interna",
                    AutoPriority = Priority.Medium,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new EmailFilter
                {
                    Id = 2,
                    Name = "Bugs Críticos",
                    SubjectKeywords = "bug,error,critical,urgente",
                    AutoCategory = "Bug",
                    AutoPriority = Priority.High,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new EmailFilter
                {
                    Id = 3,
                    Name = "Solicitudes de Soporte",
                    SubjectKeywords = "soporte,ayuda,problema,issue",
                    AutoCategory = "Soporte",
                    AutoPriority = Priority.Medium,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            );

            // Tickets de ejemplo
            modelBuilder.Entity<Ticket>().HasData(
                new Ticket
                {
                    Id = 1,
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
                    Id = 2,
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
                    Id = 3,
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
                    Id = 4,
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
            );

            // Comentarios de ejemplo
            modelBuilder.Entity<TicketComment>().HasData(
                new TicketComment
                {
                    Id = 1,
                    TicketId = 1,
                    Content = "Ticket asignado al equipo de desarrollo para investigación.",
                    Author = "Sistema",
                    AuthorEmail = "system@company.com",
                    IsSystemComment = true,
                    CreatedDate = DateTime.UtcNow.AddDays(-3)
                },
                new TicketComment
                {
                    Id = 2,
                    TicketId = 1,
                    Content = "He identificado el problema. Está relacionado con la actualización reciente de la base de datos. Trabajando en la solución.",
                    Author = "Admin Usuario",
                    AuthorEmail = "admin@company.com",
                    IsSystemComment = false,
                    CreatedDate = DateTime.UtcNow.AddDays(-1)
                },
                new TicketComment
                {
                    Id = 3,
                    TicketId = 4,
                    Content = "El problema ha sido escalado al equipo de infraestructura de red.",
                    Author = "Tech Support",
                    AuthorEmail = "techsupport@company.com",
                    IsSystemComment = false,
                    CreatedDate = DateTime.UtcNow.AddHours(-2)
                }
            );
        }
    }
}
