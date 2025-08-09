using TaskBoard.API.Models;
using TaskBoardTaskStatus = TaskBoard.API.Models.TaskStatus;

namespace TaskBoard.API.Data
{
    public static class DataSeeder
    {
        public static void SeedData(TaskBoardDbContext context)
        {
            try
            {
                // Check if data already exists
                if (context.Sprints.Any() || context.Tasks.Any())
                {
                    Console.WriteLine("⚠️  Los datos de prueba ya existen en la base de datos.");
                    return; // Database has been seeded
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error verificando datos existentes: {ex.Message}");
                Console.WriteLine("🔄 Continuando con la inserción de datos...");
            }

            // Create Sprints
            var sprints = new List<Sprint>
            {
                new Sprint
                {
                    Name = "Sprint 1 - Funcionalidades Base",
                    Goal = "Implementar las funcionalidades principales del sistema de gestión de tareas",
                    Status = SprintStatus.Completed,
                    StartDate = DateTime.Now.AddDays(-21),
                    EndDate = DateTime.Now.AddDays(-7),
                    CreatedAt = DateTime.Now.AddDays(-28),
                    UpdatedAt = DateTime.Now.AddDays(-7)
                },
                new Sprint
                {
                    Name = "Sprint 2 - Mejoras UI/UX",
                    Goal = "Mejorar la experiencia de usuario y la interfaz visual",
                    Status = SprintStatus.Active,
                    StartDate = DateTime.Now.AddDays(-7),
                    EndDate = DateTime.Now.AddDays(7),
                    CreatedAt = DateTime.Now.AddDays(-14),
                    UpdatedAt = DateTime.Now.AddDays(-1)
                },
                new Sprint
                {
                    Name = "Sprint 3 - Funcionalidades Avanzadas",
                    Goal = "Implementar reportes y analytics avanzados",
                    Status = SprintStatus.Planned,
                    StartDate = DateTime.Now.AddDays(7),
                    EndDate = DateTime.Now.AddDays(21),
                    CreatedAt = DateTime.Now.AddDays(-3),
                    UpdatedAt = DateTime.Now.AddDays(-1)
                }
            };

            context.Sprints.AddRange(sprints);
            context.SaveChanges();

            // Create Tasks with different statuses and priorities
            var tasks = new List<TaskItem>
            {
                // Sprint 1 Tasks (Completed Sprint)
                new TaskItem
                {
                    Title = "Configurar estructura del proyecto",
                    Description = "Crear la estructura base del proyecto con Angular y ASP.NET Core",
                    Status = TaskBoardTaskStatus.Done,
                    Priority = TaskPriority.High,
                    AssignedTo = "Juan Pérez",
                    SprintId = sprints[0].Id,
                    CreatedAt = DateTime.Now.AddDays(-25),
                    UpdatedAt = DateTime.Now.AddDays(-20)
                },
                new TaskItem
                {
                    Title = "Implementar autenticación de usuarios",
                    Description = "Sistema básico de login y registro de usuarios",
                    Status = TaskBoardTaskStatus.Done,
                    Priority = TaskPriority.Critical,
                    AssignedTo = "María García",
                    SprintId = sprints[0].Id,
                    CreatedAt = DateTime.Now.AddDays(-24),
                    UpdatedAt = DateTime.Now.AddDays(-18)
                },
                new TaskItem
                {
                    Title = "Crear componente de lista de tareas",
                    Description = "Componente principal para mostrar y gestionar las tareas",
                    Status = TaskBoardTaskStatus.Done,
                    Priority = TaskPriority.High,
                    AssignedTo = "Carlos Ruiz",
                    SprintId = sprints[0].Id,
                    CreatedAt = DateTime.Now.AddDays(-23),
                    UpdatedAt = DateTime.Now.AddDays(-15)
                },

                // Sprint 2 Tasks (Active Sprint)
                new TaskItem
                {
                    Title = "Rediseñar interfaz del dashboard",
                    Description = "Actualizar el dashboard principal con mejor diseño y métricas",
                    Status = TaskBoardTaskStatus.InProgress,
                    Priority = TaskPriority.Medium,
                    AssignedTo = "Ana López",
                    SprintId = sprints[1].Id,
                    CreatedAt = DateTime.Now.AddDays(-10),
                    UpdatedAt = DateTime.Now.AddDays(-2)
                },
                new TaskItem
                {
                    Title = "Implementar drag & drop en Kanban",
                    Description = "Funcionalidad para arrastrar y soltar tareas entre columnas",
                    Status = TaskBoardTaskStatus.InProgress,
                    Priority = TaskPriority.High,
                    AssignedTo = "Pedro Martínez",
                    SprintId = sprints[1].Id,
                    CreatedAt = DateTime.Now.AddDays(-8),
                    UpdatedAt = DateTime.Now.AddDays(-1)
                },
                new TaskItem
                {
                    Title = "Optimizar consultas de base de datos",
                    Description = "Mejorar el rendimiento de las consultas SQL más frecuentes",
                    Status = TaskBoardTaskStatus.Review,
                    Priority = TaskPriority.Medium,
                    AssignedTo = "Laura Sánchez",
                    SprintId = sprints[1].Id,
                    CreatedAt = DateTime.Now.AddDays(-6),
                    UpdatedAt = DateTime.Now.AddHours(-12)
                },
                new TaskItem
                {
                    Title = "Agregar notificaciones en tiempo real",
                    Description = "Implementar SignalR para notificaciones push",
                    Status = TaskBoardTaskStatus.Todo,
                    Priority = TaskPriority.Medium,
                    AssignedTo = "Miguel Torres",
                    SprintId = sprints[1].Id,
                    CreatedAt = DateTime.Now.AddDays(-4),
                    UpdatedAt = DateTime.Now.AddDays(-4)
                },

                // Backlog Tasks (Not assigned to any sprint)
                new TaskItem
                {
                    Title = "Implementar sistema de reportes",
                    Description = "Generar reportes de productividad y métricas del equipo",
                    Status = TaskBoardTaskStatus.Backlog,
                    Priority = TaskPriority.Low,
                    AssignedTo = null,
                    SprintId = null,
                    CreatedAt = DateTime.Now.AddDays(-12),
                    UpdatedAt = DateTime.Now.AddDays(-12)
                },
                new TaskItem
                {
                    Title = "Integración con herramientas externas",
                    Description = "Conectar con Slack, Teams y otras herramientas de comunicación",
                    Status = TaskBoardTaskStatus.Backlog,
                    Priority = TaskPriority.Low,
                    AssignedTo = null,
                    SprintId = null,
                    CreatedAt = DateTime.Now.AddDays(-8),
                    UpdatedAt = DateTime.Now.AddDays(-8)
                },
                new TaskItem
                {
                    Title = "Mejorar seguridad de la aplicación",
                    Description = "Implementar medidas de seguridad adicionales y validaciones",
                    Status = TaskBoardTaskStatus.Backlog,
                    Priority = TaskPriority.Critical,
                    AssignedTo = null,
                    SprintId = null,
                    CreatedAt = DateTime.Now.AddDays(-5),
                    UpdatedAt = DateTime.Now.AddDays(-5)
                }
            };

            context.Tasks.AddRange(tasks);
            context.SaveChanges();

            // Create Comments
            var comments = new List<Comment>
            {
                new Comment
                {
                    Content = "Gran trabajo en la configuración inicial del proyecto. Todo se ve muy bien estructurado.",
                    Author = "Project Manager",
                    CreatedAt = DateTime.Now.AddDays(-20),
                    TaskItemId = tasks[0].Id
                },
                new Comment
                {
                    Content = "La autenticación está funcionando perfectamente. Excelente implementación.",
                    Author = "Tech Lead",
                    CreatedAt = DateTime.Now.AddDays(-18),
                    TaskItemId = tasks[1].Id
                },
                new Comment
                {
                    Content = "El nuevo dashboard se ve increíble. Los usuarios van a adorar esta interfaz.",
                    Author = "UX Designer",
                    CreatedAt = DateTime.Now.AddDays(-2),
                    TaskItemId = tasks[3].Id
                },
                new Comment
                {
                    Content = "Sprint 2 está progresando muy bien. Estamos en buen camino para cumplir nuestros objetivos.",
                    Author = "Scrum Master",
                    CreatedAt = DateTime.Now.AddHours(-6),
                    SprintId = sprints[1].Id
                },
                new Comment
                {
                    Content = "Necesitamos revisar los requerimientos de seguridad antes de continuar con esta tarea.",
                    Author = "Security Expert",
                    CreatedAt = DateTime.Now.AddDays(-3),
                    TaskItemId = tasks[9].Id
                }
            };

            context.Comments.AddRange(comments);
            context.SaveChanges();

            Console.WriteLine("✅ Datos de prueba insertados correctamente:");
            Console.WriteLine($"   - {sprints.Count} sprints creados");
            Console.WriteLine($"   - {tasks.Count} tareas creadas");
            Console.WriteLine($"   - {comments.Count} comentarios creados");
        }
    }
}
