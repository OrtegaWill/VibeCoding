using Microsoft.EntityFrameworkCore;
using AgileBacklogAPI.Models;

namespace AgileBacklogAPI.Data
{
    public class AgileBacklogContext : DbContext
    {
        public AgileBacklogContext(DbContextOptions<AgileBacklogContext> options) : base(options)
        {
        }
        
        public DbSet<Tarea> Tareas { get; set; }
        public DbSet<Sprint> Sprints { get; set; }
        public DbSet<Catalogo> Catalogos { get; set; }
        public DbSet<ComentarioTarea> ComentariosTarea { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configuración para Tarea
            modelBuilder.Entity<Tarea>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Relaciones con catálogos
                entity.HasOne(e => e.GrupoAsignado)
                    .WithMany()
                    .HasForeignKey(e => e.GrupoAsignadoId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Prioridad)
                    .WithMany()
                    .HasForeignKey(e => e.PrioridadId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Estatus)
                    .WithMany()
                    .HasForeignKey(e => e.EstatusId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Criticidad)
                    .WithMany()
                    .HasForeignKey(e => e.CriticidadId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.TipoQueja)
                    .WithMany()
                    .HasForeignKey(e => e.TipoQuejaId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Origen)
                    .WithMany()
                    .HasForeignKey(e => e.OrigenId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.Categoria)
                    .WithMany()
                    .HasForeignKey(e => e.CategoriaId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.GrupoResolutor)
                    .WithMany()
                    .HasForeignKey(e => e.GrupoResolutorId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.EstadoTarea)
                    .WithMany()
                    .HasForeignKey(e => e.EstadoTareaId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Relación con Sprint
                entity.HasOne(e => e.Sprint)
                    .WithMany(s => s.Tareas)
                    .HasForeignKey(e => e.SprintId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            
            // Configuración para Sprint
            modelBuilder.Entity<Sprint>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
            });
            
            // Configuración para Catálogo
            modelBuilder.Entity<Catalogo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Tipo).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Valor).IsRequired().HasMaxLength(200);
                
                // Índice único compuesto para Tipo + Valor
                entity.HasIndex(e => new { e.Tipo, e.Valor }).IsUnique();
            });
            
            // Configuración para ComentarioTarea
            modelBuilder.Entity<ComentarioTarea>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(e => e.Tarea)
                    .WithMany(t => t.Comentarios)
                    .HasForeignKey(e => e.TareaId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.Property(e => e.Contenido).IsRequired().HasMaxLength(2000);
            });
            
            // Datos iniciales para catálogos
            SeedCatalogos(modelBuilder);
        }
        
        private void SeedCatalogos(ModelBuilder modelBuilder)
        {
            var catalogos = new List<Catalogo>();
            int id = 1;
            
            // Estados de tarea para Kanban
            var estadosTarea = new[] { "Por Hacer", "En Progreso", "Hecho" };
            foreach (var estado in estadosTarea)
            {
                catalogos.Add(new Catalogo
                {
                    Id = id++,
                    Tipo = "EstadoTarea",
                    Valor = estado,
                    Activo = true,
                    Orden = id - 1
                });
            }
            
            // Prioridades
            var prioridades = new[] { "Baja", "Media", "Alta", "Crítica" };
            foreach (var prioridad in prioridades)
            {
                catalogos.Add(new Catalogo
                {
                    Id = id++,
                    Tipo = "Prioridad",
                    Valor = prioridad,
                    Activo = true,
                    Orden = id - 1
                });
            }
            
            // Estados generales
            var estatus = new[] { "Nuevo", "Asignado", "En Progreso", "Resuelto", "Cerrado" };
            foreach (var est in estatus)
            {
                catalogos.Add(new Catalogo
                {
                    Id = id++,
                    Tipo = "Estatus",
                    Valor = est,
                    Activo = true,
                    Orden = id - 1
                });
            }
            
            // Criticidad
            var criticidades = new[] { "Baja", "Media", "Alta", "Crítica" };
            foreach (var criticidad in criticidades)
            {
                catalogos.Add(new Catalogo
                {
                    Id = id++,
                    Tipo = "Criticidad",
                    Valor = criticidad,
                    Activo = true,
                    Orden = id - 1
                });
            }
            
            modelBuilder.Entity<Catalogo>().HasData(catalogos);
        }
    }
}
