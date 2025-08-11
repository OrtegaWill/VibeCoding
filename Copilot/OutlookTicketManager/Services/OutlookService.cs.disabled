using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using OutlookTicketManager.Models;

namespace OutlookTicketManager.Services
{
    /// <summary>
    /// Servicio para interactuar con Microsoft Graph API y Outlook 365
    /// </summary>
    public class OutlookService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OutlookService> _logger;
        private GraphServiceClient? _graphServiceClient;

        public OutlookService(IConfiguration configuration, ILogger<OutlookService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Inicializa el cliente de Microsoft Graph
        /// </summary>
        public Task<bool> InitializeGraphClientAsync()
        {
            try
            {
                // Por ahora simular la inicialización
                // En una implementación real, aquí se configuraria Microsoft Graph
                _logger.LogWarning("Microsoft Graph client initialization is not implemented yet. This is a placeholder.");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inicializar cliente de Microsoft Graph");
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Obtiene los correos electrónicos de la bandeja de entrada
        /// </summary>
        /// <param name="userEmail">Email del usuario (ej: 2422236@cognizant.com)</param>
        /// <param name="maxResults">Número máximo de correos a obtener</param>
        /// <returns>Lista de correos electrónicos</returns>
        public async Task<List<Message>> GetEmailsAsync(string userEmail, int maxResults = 50)
        {
            try
            {
                if (_graphServiceClient == null)
                {
                    var initialized = await InitializeGraphClientAsync();
                    if (!initialized)
                    {
                        throw new InvalidOperationException("Failed to initialize Graph client.");
                    }
                }

                // Obtener mensajes de la bandeja de entrada
                var messages = await _graphServiceClient!.Users[userEmail]
                    .MailFolders.Inbox
                    .Messages
                    .Request()
                    .Top(maxResults)
                    .OrderBy("receivedDateTime desc")
                    .GetAsync();

                return messages.CurrentPage.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving emails for user {UserEmail}", userEmail);
                return new List<Message>();
            }
        }

        /// <summary>
        /// Obtiene correos filtrados por remitente específico
        /// </summary>
        /// <param name="userEmail">Email del usuario que recibe</param>
        /// <param name="fromEmail">Email del remitente a filtrar</param>
        /// <param name="maxResults">Número máximo de resultados</param>
        /// <returns>Lista de correos del remitente específico</returns>
        public async Task<List<Message>> GetEmailsByFromAddressAsync(string userEmail, string fromEmail, int maxResults = 50)
        {
            try
            {
                if (_graphServiceClient == null)
                {
                    var initialized = await InitializeGraphClientAsync();
                    if (!initialized)
                    {
                        throw new InvalidOperationException("Failed to initialize Graph client.");
                    }
                }

                // Crear filtro para el remitente específico
                var filterQuery = $"from/emailAddress/address eq '{fromEmail}'";

                var messages = await _graphServiceClient!.Users[userEmail]
                    .MailFolders.Inbox
                    .Messages
                    .Request()
                    .Filter(filterQuery)
                    .Top(maxResults)
                    .OrderBy("receivedDateTime desc")
                    .GetAsync();

                return messages.CurrentPage.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving emails from {FromEmail} for user {UserEmail}", fromEmail, userEmail);
                return new List<Message>();
            }
        }

        /// <summary>
        /// Obtiene correos no leídos de la bandeja de entrada
        /// </summary>
        /// <param name="userEmail">Email del usuario</param>
        /// <param name="maxResults">Número máximo de resultados</param>
        /// <returns>Lista de correos no leídos</returns>
        public async Task<List<Message>> GetUnreadEmailsAsync(string userEmail, int maxResults = 50)
        {
            try
            {
                if (_graphServiceClient == null)
                {
                    var initialized = await InitializeGraphClientAsync();
                    if (!initialized)
                    {
                        throw new InvalidOperationException("Failed to initialize Graph client.");
                    }
                }

                // Filtrar solo correos no leídos
                var filterQuery = "isRead eq false";

                var messages = await _graphServiceClient!.Users[userEmail]
                    .MailFolders.Inbox
                    .Messages
                    .Request()
                    .Filter(filterQuery)
                    .Top(maxResults)
                    .OrderBy("receivedDateTime desc")
                    .GetAsync();

                return messages.CurrentPage.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread emails for user {UserEmail}", userEmail);
                return new List<Message>();
            }
        }

        /// <summary>
        /// Marca un correo como leído
        /// </summary>
        /// <param name="userEmail">Email del usuario</param>
        /// <param name="messageId">ID del mensaje</param>
        /// <returns>True si se marcó correctamente</returns>
        public async Task<bool> MarkEmailAsReadAsync(string userEmail, string messageId)
        {
            try
            {
                if (_graphServiceClient == null)
                {
                    var initialized = await InitializeGraphClientAsync();
                    if (!initialized)
                    {
                        return false;
                    }
                }

                var message = new Message
                {
                    IsRead = true
                };

                await _graphServiceClient!.Users[userEmail]
                    .Messages[messageId]
                    .Request()
                    .UpdateAsync(message);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking email as read for user {UserEmail}, message {MessageId}", userEmail, messageId);
                return false;
            }
        }

        /// <summary>
        /// Envía una respuesta a un correo electrónico
        /// </summary>
        /// <param name="userEmail">Email del usuario que envía</param>
        /// <param name="messageId">ID del mensaje original</param>
        /// <param name="replyContent">Contenido de la respuesta</param>
        /// <returns>True si se envió correctamente</returns>
        public async Task<bool> ReplyToEmailAsync(string userEmail, string messageId, string replyContent)
        {
            try
            {
                if (_graphServiceClient == null)
                {
                    var initialized = await InitializeGraphClientAsync();
                    if (!initialized)
                    {
                        return false;
                    }
                }

                var reply = new Message
                {
                    Body = new ItemBody
                    {
                        ContentType = BodyType.Html,
                        Content = replyContent
                    }
                };

                await _graphServiceClient!.Users[userEmail]
                    .Messages[messageId]
                    .Reply(reply)
                    .Request()
                    .PostAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error replying to email for user {UserEmail}, message {MessageId}", userEmail, messageId);
                return false;
            }
        }
    }
}
