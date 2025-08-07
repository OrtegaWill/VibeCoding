using Microsoft.Graph;
using OutlookTicketManager.Models;

namespace OutlookTicketManager.Services
{
    /// <summary>
    /// Servicio original de Outlook - reemplazado temporalmente por OutlookServiceSimplified
    /// debido a incompatibilidades con Microsoft Graph SDK v5.42.0
    /// </summary>
    public class OutlookServiceDeprecated
    {
        private readonly ILogger<OutlookServiceDeprecated> _logger;

        public OutlookServiceDeprecated(ILogger<OutlookServiceDeprecated> logger)
        {
            _logger = logger;
        }

        // Placeholder methods to resolve compilation errors
        public Task<bool> InitializeGraphClientAsync()
        {
            _logger.LogWarning("OutlookService is deprecated - use OutlookServiceSimplified instead");
            return Task.FromResult(false);
        }

        public Task<List<object>> GetUnreadEmailsAsync(string userEmail, int maxEmails)
        {
            _logger.LogWarning("OutlookService is deprecated - use OutlookServiceSimplified instead");
            return Task.FromResult(new List<object>());
        }

        public Task<List<object>> GetEmailsAsync(string userEmail, int maxEmails)
        {
            _logger.LogWarning("OutlookService is deprecated - use OutlookServiceSimplified instead");
            return Task.FromResult(new List<object>());
        }

        public Task<List<object>> GetMessagesAsync(string userEmail, bool onlyUnread = true, int maxResults = 50, DateTime? since = null)
        {
            _logger.LogWarning("OutlookService is deprecated - use OutlookServiceSimplified instead");
            return Task.FromResult(new List<object>());
        }

        public Task MarkEmailAsReadAsync(string userEmail, string messageId)
        {
            _logger.LogWarning("OutlookService is deprecated - use OutlookServiceSimplified instead");
            return Task.CompletedTask;
        }

        public Task ReplyToEmailAsync(string userEmail, string messageId, string replyContent)
        {
            _logger.LogWarning("OutlookService is deprecated - use OutlookServiceSimplified instead");
            return Task.CompletedTask;
        }
    }
}
