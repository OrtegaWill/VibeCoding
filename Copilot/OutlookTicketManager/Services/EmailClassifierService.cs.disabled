using Microsoft.Graph;
using Microsoft.Graph.Models;
using OutlookTicketManager.Models;
using System.Text.RegularExpressions;

namespace OutlookTicketManager.Services
{
    /// <summary>
    /// Servicio para clasificar automáticamente emails y extraer información relevante para tickets
    /// </summary>
    public class EmailClassifierService
    {
        private readonly ILogger<EmailClassifierService> _logger;

        public EmailClassifierService(ILogger<EmailClassifierService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Clasifica un email y extrae información para crear un ticket
        /// </summary>
        /// <param name="message">Mensaje de correo electrónico</param>
        /// <param name="filters">Filtros configurados para clasificación</param>
        /// <returns>Información del ticket extraída</returns>
        public TicketInfo ClassifyEmail(Message message, List<EmailFilter> filters)
        {
            try
            {
                var ticketInfo = new TicketInfo
                {
                    EmailId = message.Id ?? Guid.NewGuid().ToString(),
                    Subject = message.Subject ?? "Sin asunto",
                    Description = ExtractTextFromBody(message.Body),
                    FromEmail = message.From?.EmailAddress?.Address ?? "unknown@unknown.com",
                    FromName = message.From?.EmailAddress?.Name ?? "Desconocido",
                    ReceivedDate = message.ReceivedDateTime?.DateTime ?? DateTime.UtcNow
                };

                // Aplicar filtros para clasificación automática
                ApplyFilters(ticketInfo, filters);

                // Analizar contenido para extraer información adicional
                AnalyzeContent(ticketInfo);

                _logger.LogInformation("Email classified: {Subject} from {FromEmail}", ticketInfo.Subject, ticketInfo.FromEmail);

                return ticketInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error classifying email: {MessageId}", message.Id);
                throw;
            }
        }

        /// <summary>
        /// Aplica los filtros configurados para clasificación automática
        /// </summary>
        private void ApplyFilters(TicketInfo ticketInfo, List<EmailFilter> filters)
        {
            foreach (var filter in filters.Where(f => f.IsActive))
            {
                bool matches = false;

                // Verificar filtro por email específico
                if (!string.IsNullOrEmpty(filter.FromEmail) && 
                    ticketInfo.FromEmail.Equals(filter.FromEmail, StringComparison.OrdinalIgnoreCase))
                {
                    matches = true;
                }

                // Verificar filtro por dominio
                if (!string.IsNullOrEmpty(filter.FromDomain) && 
                    ticketInfo.FromEmail.EndsWith(filter.FromDomain, StringComparison.OrdinalIgnoreCase))
                {
                    matches = true;
                }

                // Verificar palabras clave en el asunto
                if (!string.IsNullOrEmpty(filter.SubjectKeywords))
                {
                    var keywords = filter.SubjectKeywords.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (keywords.Any(keyword => ticketInfo.Subject.Contains(keyword.Trim(), StringComparison.OrdinalIgnoreCase)))
                    {
                        matches = true;
                    }
                }

                // Verificar palabras clave en el cuerpo
                if (!string.IsNullOrEmpty(filter.BodyKeywords))
                {
                    var keywords = filter.BodyKeywords.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (keywords.Any(keyword => ticketInfo.Description.Contains(keyword.Trim(), StringComparison.OrdinalIgnoreCase)))
                    {
                        matches = true;
                    }
                }

                // Aplicar configuración automática si coincide
                if (matches)
                {
                    if (!string.IsNullOrEmpty(filter.AutoCategory))
                        ticketInfo.Category = filter.AutoCategory;

                    if (filter.AutoPriority.HasValue)
                        ticketInfo.Priority = filter.AutoPriority.Value;

                    if (!string.IsNullOrEmpty(filter.AutoAssignTo))
                        ticketInfo.AssignedTo = filter.AutoAssignTo;

                    _logger.LogInformation("Applied filter '{FilterName}' to email from {FromEmail}", filter.Name, ticketInfo.FromEmail);
                    break; // Aplicar solo el primer filtro que coincida
                }
            }
        }

        /// <summary>
        /// Analiza el contenido del email para extraer información adicional
        /// </summary>
        private void AnalyzeContent(TicketInfo ticketInfo)
        {
            // Determinar prioridad basada en palabras clave si no se asignó automáticamente
            if (ticketInfo.Priority == Priority.Medium)
            {
                ticketInfo.Priority = DeterminePriorityFromContent(ticketInfo.Subject, ticketInfo.Description);
            }

            // Extraer tags/etiquetas
            ticketInfo.Tags = ExtractTags(ticketInfo.Subject, ticketInfo.Description);

            // Determinar categoría si no se asignó automáticamente
            if (string.IsNullOrEmpty(ticketInfo.Category))
            {
                ticketInfo.Category = DetermineCategoryFromContent(ticketInfo.Subject, ticketInfo.Description);
            }
        }

        /// <summary>
        /// Determina la prioridad basándose en el contenido del email
        /// </summary>
        private Priority DeterminePriorityFromContent(string subject, string description)
        {
            var combinedText = $"{subject} {description}".ToLower();

            // Palabras clave para prioridad crítica
            var criticalKeywords = new[] { "crítico", "critical", "urgente", "urgent", "bloqueador", "blocking", "caído", "down", "no funciona" };
            if (criticalKeywords.Any(keyword => combinedText.Contains(keyword)))
                return Priority.Critical;

            // Palabras clave para prioridad alta
            var highKeywords = new[] { "importante", "important", "alto", "high", "error", "fallo", "bug", "problema grave" };
            if (highKeywords.Any(keyword => combinedText.Contains(keyword)))
                return Priority.High;

            // Palabras clave para prioridad baja
            var lowKeywords = new[] { "consulta", "question", "duda", "pregunta", "información", "mejora", "enhancement" };
            if (lowKeywords.Any(keyword => combinedText.Contains(keyword)))
                return Priority.Low;

            return Priority.Medium; // Por defecto
        }

        /// <summary>
        /// Determina la categoría basándose en el contenido
        /// </summary>
        private string DetermineCategoryFromContent(string subject, string description)
        {
            var combinedText = $"{subject} {description}".ToLower();

            // Patrones para diferentes categorías
            var categoryPatterns = new Dictionary<string, string[]>
            {
                { "Bug", new[] { "bug", "error", "fallo", "excepción", "exception", "crash" } },
                { "Soporte", new[] { "ayuda", "help", "soporte", "support", "problema", "issue" } },
                { "Consulta", new[] { "pregunta", "question", "duda", "consulta", "cómo", "how" } },
                { "Mejora", new[] { "mejora", "enhancement", "feature", "funcionalidad", "nuevo" } },
                { "Configuración", new[] { "configuración", "config", "setup", "instalación", "install" } },
                { "Acceso", new[] { "acceso", "access", "login", "password", "contraseña", "permiso" } }
            };

            foreach (var category in categoryPatterns)
            {
                if (category.Value.Any(keyword => combinedText.Contains(keyword)))
                {
                    return category.Key;
                }
            }

            return "General"; // Categoría por defecto
        }

        /// <summary>
        /// Extrae tags relevantes del contenido
        /// </summary>
        private string ExtractTags(string subject, string description)
        {
            var tags = new List<string>();
            var combinedText = $"{subject} {description}".ToLower();

            // Tags comunes basados en tecnologías
            var techTags = new Dictionary<string, string[]>
            {
                { "web", new[] { "web", "html", "css", "javascript", "browser" } },
                { "database", new[] { "database", "sql", "mysql", "postgresql", "oracle" } },
                { "network", new[] { "network", "red", "conexión", "internet", "vpn" } },
                { "security", new[] { "security", "seguridad", "virus", "malware", "firewall" } },
                { "mobile", new[] { "mobile", "móvil", "android", "ios", "app" } },
                { "email", new[] { "email", "correo", "outlook", "exchange" } }
            };

            foreach (var tagGroup in techTags)
            {
                if (tagGroup.Value.Any(keyword => combinedText.Contains(keyword)))
                {
                    tags.Add(tagGroup.Key);
                }
            }

            return string.Join(", ", tags);
        }

        /// <summary>
        /// Extrae texto plano del cuerpo HTML/texto del email
        /// </summary>
        private string ExtractTextFromBody(ItemBody? body)
        {
            if (body == null || string.IsNullOrEmpty(body.Content))
                return string.Empty;

            var content = body.Content;

            // Si es HTML, eliminar tags
            if (body.ContentType == BodyType.Html)
            {
                // Eliminar tags HTML de forma simple
                content = Regex.Replace(content, "<[^>]*>", " ");
                // Decodificar entidades HTML comunes
                content = content.Replace("&nbsp;", " ")
                                .Replace("&amp;", "&")
                                .Replace("&lt;", "<")
                                .Replace("&gt;", ">")
                                .Replace("&quot;", "\"")
                                .Replace("&#39;", "'");
            }

            // Limpiar espacios extra y saltos de línea
            content = Regex.Replace(content, @"\s+", " ").Trim();

            // Limitar longitud para evitar descripciones muy largas
            if (content.Length > 2000)
            {
                content = content.Substring(0, 2000) + "...";
            }

            return content;
        }
    }

    /// <summary>
    /// Clase para almacenar información extraída de un email para crear un ticket
    /// </summary>
    public class TicketInfo
    {
        public string EmailId { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public Priority Priority { get; set; } = Priority.Medium;
        public string? AssignedTo { get; set; }
        public string? Tags { get; set; }
        public DateTime ReceivedDate { get; set; }
    }
}
