using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskBoard.API.Data;
using TaskBoard.API.DTOs;

namespace TaskBoard.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly TaskBoardDbContext _context;

        public TestController(TaskBoardDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "Test endpoint working", timestamp = DateTime.Now });
        }

        [HttpGet("simple-tasks")]
        public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetSimpleTasks()
        {
            try
            {
                // Obtener tasks sin navegaciones
                var tasksData = await _context.Tasks
                    .AsNoTracking()
                    .Select(t => new
                    {
                        t.Id,
                        t.Title,
                        t.Description,
                        t.Status,
                        t.Priority,
                        t.AssignedTo,
                        t.SprintId,
                        t.CreatedAt,
                        t.UpdatedAt
                    })
                    .ToListAsync();

                // Obtener nombres de sprints por separado
                var sprintIds = tasksData.Where(t => t.SprintId.HasValue).Select(t => t.SprintId!.Value).Distinct().ToList();
                var sprintNames = new Dictionary<int, string>();
                if (sprintIds.Any())
                {
                    sprintNames = await _context.Sprints
                        .AsNoTracking()
                        .Where(s => sprintIds.Contains(s.Id))
                        .ToDictionaryAsync(s => s.Id, s => s.Name);
                }

                // Mapear a DTOs
                var tasks = tasksData.Select(t => new TaskItemDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status.ToString(),
                    Priority = t.Priority.ToString(),
                    AssignedTo = t.AssignedTo,
                    SprintId = t.SprintId,
                    SprintName = t.SprintId.HasValue && sprintNames.ContainsKey(t.SprintId.Value) 
                        ? sprintNames[t.SprintId.Value] 
                        : null,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    Comments = new List<CommentDto>()
                }).ToList();

                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("simple-sprints")]
        public async Task<ActionResult<IEnumerable<SprintDto>>> GetSimpleSprints()
        {
            try
            {
                var sprints = await _context.Sprints
                    .AsNoTracking()
                    .Select(s => new SprintDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Goal = s.Goal ?? string.Empty,
                        Status = s.Status.ToString(),
                        StartDate = s.StartDate,
                        EndDate = s.EndDate,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt,
                        Tasks = new List<TaskItemSummaryDto>(),
                        Comments = new List<CommentDto>()
                    })
                    .ToListAsync();

                return Ok(sprints);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("simple-comments")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetSimpleComments()
        {
            try
            {
                var comments = await _context.Comments
                    .AsNoTracking()
                    .Select(c => new CommentDto
                    {
                        Id = c.Id,
                        Content = c.Content,
                        Author = c.Author,
                        CreatedAt = c.CreatedAt,
                        TaskItemId = c.TaskItemId,
                        SprintId = c.SprintId
                    })
                    .ToListAsync();

                return Ok(comments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost("create-sprint-scenarios")]
        public async Task<IActionResult> CreateSprintScenarios()
        {
            try
            {
                // Limpiar datos existentes
                _context.Comments.RemoveRange(_context.Comments);
                _context.Tasks.RemoveRange(_context.Tasks);
                _context.Sprints.RemoveRange(_context.Sprints);
                await _context.SaveChangesAsync();

                await CreateTestSprintsAndTasks();
                
                return Ok(new { message = "Escenarios de sprint creados exitosamente", timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creando escenarios: {ex.Message}");
            }
        }

        [HttpGet("sprint-summary")]
        public async Task<ActionResult> GetSprintSummary()
        {
            try
            {
                var sprints = await _context.Sprints.AsNoTracking().ToListAsync();
                var tasks = await _context.Tasks.AsNoTracking().ToListAsync();
                var comments = await _context.Comments.AsNoTracking().ToListAsync();

                var summary = sprints.Select(sprint => new
                {
                    Sprint = sprint.Name,
                    Status = sprint.Status.ToString(),
                    TaskCount = tasks.Count(t => t.SprintId == sprint.Id),
                    TasksByStatus = tasks.Where(t => t.SprintId == sprint.Id)
                        .GroupBy(t => t.Status.ToString())
                        .ToDictionary(g => g.Key, g => g.Count()),
                    CommentCount = comments.Count(c => c.SprintId == sprint.Id),
                    StartDate = sprint.StartDate,
                    EndDate = sprint.EndDate
                }).ToList();

                var backlogTasks = tasks.Count(t => t.SprintId == null);

                return Ok(new
                {
                    SprintSummary = summary,
                    BacklogTaskCount = backlogTasks,
                    TotalTasks = tasks.Count,
                    TotalComments = comments.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        private async Task CreateTestSprintsAndTasks()
        {
            var baseDate = DateTime.Now;

            // Sprint 1: Completado (hace 2 meses)
            var sprint1 = new TaskBoard.API.Models.Sprint
            {
                Name = "Sprint 1 - MVP Foundation",
                Goal = "Establecer la base del MVP con funcionalidades core de autenticación y gestión básica de tareas",
                Status = TaskBoard.API.Models.SprintStatus.Completed,
                StartDate = baseDate.AddDays(-60),
                EndDate = baseDate.AddDays(-46),
                CreatedAt = baseDate.AddDays(-65),
                UpdatedAt = baseDate.AddDays(-46)
            };
            _context.Sprints.Add(sprint1);
            await _context.SaveChangesAsync();

            // Sprint 2: Completado (hace 1 mes)  
            var sprint2 = new TaskBoard.API.Models.Sprint
            {
                Name = "Sprint 2 - User Experience Enhancement",
                Goal = "Mejorar la experiencia de usuario con interfaz moderna y funcionalidades avanzadas",
                Status = TaskBoard.API.Models.SprintStatus.Completed,
                StartDate = baseDate.AddDays(-45),
                EndDate = baseDate.AddDays(-31),
                CreatedAt = baseDate.AddDays(-50),
                UpdatedAt = baseDate.AddDays(-31)
            };
            _context.Sprints.Add(sprint2);
            await _context.SaveChangesAsync();

            // Sprint 3: Activo (en curso)
            var sprint3 = new TaskBoard.API.Models.Sprint
            {
                Name = "Sprint 3 - Advanced Features & Analytics",
                Goal = "Implementar características avanzadas, reportes y analytics para mejorar la productividad",
                Status = TaskBoard.API.Models.SprintStatus.Active,
                StartDate = baseDate.AddDays(-14),
                EndDate = baseDate.AddDays(7),
                CreatedAt = baseDate.AddDays(-20),
                UpdatedAt = baseDate.AddDays(-1)
            };
            _context.Sprints.Add(sprint3);
            await _context.SaveChangesAsync();

            // Sprint 4: Planeado (próximo sprint)
            var sprint4 = new TaskBoard.API.Models.Sprint
            {
                Name = "Sprint 4 - Integration & Performance",
                Goal = "Integrar con servicios externos y optimizar el rendimiento de la aplicación",
                Status = TaskBoard.API.Models.SprintStatus.Planned,
                StartDate = baseDate.AddDays(8),
                EndDate = baseDate.AddDays(22),
                CreatedAt = baseDate.AddDays(-10),
                UpdatedAt = baseDate.AddDays(-2)
            };
            _context.Sprints.Add(sprint4);
            await _context.SaveChangesAsync();

            // Sprint 5: Planeado (futuro)
            var sprint5 = new TaskBoard.API.Models.Sprint
            {
                Name = "Sprint 5 - Mobile & Accessibility",
                Goal = "Desarrollar versión móvil responsive y mejorar accesibilidad",
                Status = TaskBoard.API.Models.SprintStatus.Planned,
                StartDate = baseDate.AddDays(23),
                EndDate = baseDate.AddDays(37),
                CreatedAt = baseDate.AddDays(-5),
                UpdatedAt = baseDate.AddDays(-1)
            };
            _context.Sprints.Add(sprint5);
            await _context.SaveChangesAsync();

            // Crear tareas para Sprint 1 (Completado)
            await CreateTasksForSprint(sprint1.Id, baseDate, "Sprint1");

            // Crear tareas para Sprint 2 (Completado)
            await CreateTasksForSprint(sprint2.Id, baseDate, "Sprint2");

            // Crear tareas para Sprint 3 (Activo)
            await CreateTasksForSprint(sprint3.Id, baseDate, "Sprint3");

            // Crear tareas para Sprint 4 (Planeado)
            await CreateTasksForSprint(sprint4.Id, baseDate, "Sprint4");

            // Crear tareas para Sprint 5 (Planeado)
            await CreateTasksForSprint(sprint5.Id, baseDate, "Sprint5");

            // Crear tareas en Backlog (sin sprint asignado)
            await CreateBacklogTasks(baseDate);

            await _context.SaveChangesAsync();
        }

        private async Task CreateTasksForSprint(int sprintId, DateTime baseDate, string sprintType)
        {
            var tasks = new List<TaskBoard.API.Models.TaskItem>();

            switch (sprintType)
            {
                case "Sprint1":
                    tasks.AddRange(new[]
                    {
                        new TaskBoard.API.Models.TaskItem
                        {
                            Title = "Configurar proyecto base y estructura",
                            Description = "Crear la estructura inicial del proyecto con Angular frontend y ASP.NET Core backend",
                            Status = TaskBoard.API.Models.TaskStatus.Done,
                            Priority = TaskBoard.API.Models.TaskPriority.High,
                            AssignedTo = "Juan Pérez",
                            SprintId = sprintId,
                            CreatedAt = baseDate.AddDays(-65),
                            UpdatedAt = baseDate.AddDays(-55)
                        },
                        new TaskBoard.API.Models.TaskItem
                        {
                            Title = "Implementar sistema de autenticación",
                            Description = "Sistema completo de login, registro y gestión de sesiones de usuario",
                            Status = TaskBoard.API.Models.TaskStatus.Done,
                            Priority = TaskBoard.API.Models.TaskPriority.Critical,
                            AssignedTo = "María García",
                            SprintId = sprintId,
                            CreatedAt = baseDate.AddDays(-63),
                            UpdatedAt = baseDate.AddDays(-50)
                        },
                        new TaskBoard.API.Models.TaskItem
                        {
                            Title = "Crear modelos de datos y DbContext",
                            Description = "Definir entidades de base de datos y configurar Entity Framework",
                            Status = TaskBoard.API.Models.TaskStatus.Done,
                            Priority = TaskBoard.API.Models.TaskPriority.High,
                            AssignedTo = "Carlos Ruiz",
                            SprintId = sprintId,
                            CreatedAt = baseDate.AddDays(-62),
                            UpdatedAt = baseDate.AddDays(-48)
                        }
                    });
                    break;

                case "Sprint2":
                    tasks.AddRange(new[]
                    {
                        new TaskBoard.API.Models.TaskItem
                        {
                            Title = "Rediseñar interfaz de usuario",
                            Description = "Actualizar el diseño con componentes modernos y mejor UX",
                            Status = TaskBoard.API.Models.TaskStatus.Done,
                            Priority = TaskBoard.API.Models.TaskPriority.Medium,
                            AssignedTo = "Ana López",
                            SprintId = sprintId,
                            CreatedAt = baseDate.AddDays(-48),
                            UpdatedAt = baseDate.AddDays(-35)
                        },
                        new TaskBoard.API.Models.TaskItem
                        {
                            Title = "Implementar drag & drop en Kanban",
                            Description = "Funcionalidad para arrastrar y soltar tareas entre estados",
                            Status = TaskBoard.API.Models.TaskStatus.Done,
                            Priority = TaskBoard.API.Models.TaskPriority.High,
                            AssignedTo = "Pedro Martínez",
                            SprintId = sprintId,
                            CreatedAt = baseDate.AddDays(-45),
                            UpdatedAt = baseDate.AddDays(-32)
                        },
                        new TaskBoard.API.Models.TaskItem
                        {
                            Title = "Agregar sistema de comentarios",
                            Description = "Permitir comentarios en tareas y sprints con tiempo real",
                            Status = TaskBoard.API.Models.TaskStatus.Done,
                            Priority = TaskBoard.API.Models.TaskPriority.Medium,
                            AssignedTo = "Laura Sánchez",
                            SprintId = sprintId,
                            CreatedAt = baseDate.AddDays(-44),
                            UpdatedAt = baseDate.AddDays(-33)
                        }
                    });
                    break;

                case "Sprint3":
                    tasks.AddRange(new[]
                    {
                        new TaskBoard.API.Models.TaskItem
                        {
                            Title = "Implementar dashboard de métricas",
                            Description = "Dashboard con gráficos y estadísticas de productividad del equipo",
                            Status = TaskBoard.API.Models.TaskStatus.InProgress,
                            Priority = TaskBoard.API.Models.TaskPriority.High,
                            AssignedTo = "Miguel Torres",
                            SprintId = sprintId,
                            CreatedAt = baseDate.AddDays(-18),
                            UpdatedAt = baseDate.AddDays(-2)
                        },
                        new TaskBoard.API.Models.TaskItem
                        {
                            Title = "Crear sistema de reportes avanzados",
                            Description = "Generar reportes detallados de sprints, tareas y performance",
                            Status = TaskBoard.API.Models.TaskStatus.InProgress,
                            Priority = TaskBoard.API.Models.TaskPriority.Medium,
                            AssignedTo = "Elena Rodríguez",
                            SprintId = sprintId,
                            CreatedAt = baseDate.AddDays(-16),
                            UpdatedAt = baseDate.AddDays(-1)
                        },
                        new TaskBoard.API.Models.TaskItem
                        {
                            Title = "Implementar notificaciones push",
                            Description = "Sistema de notificaciones en tiempo real usando SignalR",
                            Status = TaskBoard.API.Models.TaskStatus.Review,
                            Priority = TaskBoard.API.Models.TaskPriority.Medium,
                            AssignedTo = "Diego Morales",
                            SprintId = sprintId,
                            CreatedAt = baseDate.AddDays(-15),
                            UpdatedAt = baseDate.AddDays(-3)
                        },
                        new TaskBoard.API.Models.TaskItem
                        {
                            Title = "Optimizar consultas de base de datos",
                            Description = "Mejorar performance con índices y queries optimizadas",
                            Status = TaskBoard.API.Models.TaskStatus.Todo,
                            Priority = TaskBoard.API.Models.TaskPriority.High,
                            AssignedTo = "Sofia Vargas",
                            SprintId = sprintId,
                            CreatedAt = baseDate.AddDays(-12),
                            UpdatedAt = baseDate.AddDays(-12)
                        }
                    });
                    break;

                case "Sprint4":
                    tasks.AddRange(new[]
                    {
                        new TaskBoard.API.Models.TaskItem
                        {
                            Title = "Integrar con Slack/Teams",
                            Description = "Conectar la aplicación con herramientas de comunicación del equipo",
                            Status = TaskBoard.API.Models.TaskStatus.Todo,
                            Priority = TaskBoard.API.Models.TaskPriority.Medium,
                            AssignedTo = "Roberto Jiménez",
                            SprintId = sprintId,
                            CreatedAt = baseDate.AddDays(-8),
                            UpdatedAt = baseDate.AddDays(-8)
                        },
                        new TaskBoard.API.Models.TaskItem
                        {
                            Title = "Implementar caché Redis",
                            Description = "Agregar caching para mejorar la velocidad de respuesta",
                            Status = TaskBoard.API.Models.TaskStatus.Todo,
                            Priority = TaskBoard.API.Models.TaskPriority.High,
                            AssignedTo = "Patricia Delgado",
                            SprintId = sprintId,
                            CreatedAt = baseDate.AddDays(-6),
                            UpdatedAt = baseDate.AddDays(-6)
                        },
                        new TaskBoard.API.Models.TaskItem
                        {
                            Title = "Configurar CI/CD Pipeline",
                            Description = "Automatizar despliegues con GitHub Actions o Azure DevOps",
                            Status = TaskBoard.API.Models.TaskStatus.Todo,
                            Priority = TaskBoard.API.Models.TaskPriority.Medium,
                            AssignedTo = "Fernando Castro",
                            SprintId = sprintId,
                            CreatedAt = baseDate.AddDays(-5),
                            UpdatedAt = baseDate.AddDays(-5)
                        }
                    });
                    break;

                case "Sprint5":
                    tasks.AddRange(new[]
                    {
                        new TaskBoard.API.Models.TaskItem
                        {
                            Title = "Desarrollar versión mobile responsive",
                            Description = "Adaptar la interfaz para dispositivos móviles y tablets",
                            Status = TaskBoard.API.Models.TaskStatus.Todo,
                            Priority = TaskBoard.API.Models.TaskPriority.High,
                            AssignedTo = "Gabriela Herrera",
                            SprintId = sprintId,
                            CreatedAt = baseDate.AddDays(-3),
                            UpdatedAt = baseDate.AddDays(-3)
                        },
                        new TaskBoard.API.Models.TaskItem
                        {
                            Title = "Implementar características de accesibilidad",
                            Description = "Agregar soporte ARIA y navegación por teclado",
                            Status = TaskBoard.API.Models.TaskStatus.Todo,
                            Priority = TaskBoard.API.Models.TaskPriority.Medium,
                            AssignedTo = "Andrés Mendoza",
                            SprintId = sprintId,
                            CreatedAt = baseDate.AddDays(-2),
                            UpdatedAt = baseDate.AddDays(-2)
                        }
                    });
                    break;
            }

            _context.Tasks.AddRange(tasks);
        }

        private async Task CreateBacklogTasks(DateTime baseDate)
        {
            var backlogTasks = new[]
            {
                new TaskBoard.API.Models.TaskItem
                {
                    Title = "Investigar migración a .NET 8",
                    Description = "Evaluar la migración del backend a la última versión de .NET",
                    Status = TaskBoard.API.Models.TaskStatus.Backlog,
                    Priority = TaskBoard.API.Models.TaskPriority.Low,
                    AssignedTo = null,
                    SprintId = null,
                    CreatedAt = baseDate.AddDays(-10),
                    UpdatedAt = baseDate.AddDays(-10)
                },
                new TaskBoard.API.Models.TaskItem
                {
                    Title = "Implementar modo oscuro",
                    Description = "Agregar tema oscuro a la aplicación con preferencias de usuario",
                    Status = TaskBoard.API.Models.TaskStatus.Backlog,
                    Priority = TaskBoard.API.Models.TaskPriority.Low,
                    AssignedTo = null,
                    SprintId = null,
                    CreatedAt = baseDate.AddDays(-7),
                    UpdatedAt = baseDate.AddDays(-7)
                },
                new TaskBoard.API.Models.TaskItem
                {
                    Title = "Agregar exportación de datos",
                    Description = "Permitir exportar tareas y reportes en formato Excel/PDF",
                    Status = TaskBoard.API.Models.TaskStatus.Backlog,
                    Priority = TaskBoard.API.Models.TaskPriority.Medium,
                    AssignedTo = null,
                    SprintId = null,
                    CreatedAt = baseDate.AddDays(-4),
                    UpdatedAt = baseDate.AddDays(-4)
                }
            };

            _context.Tasks.AddRange(backlogTasks);
        }
    }
}
