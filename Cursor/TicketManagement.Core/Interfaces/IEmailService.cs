using TicketManagement.Core.Models;

namespace TicketManagement.Core.Interfaces
{
    public interface IEmailService
    {
        Task<IEnumerable<EmailMessage>> GetUnreadEmailsAsync();
        Task<IEnumerable<EmailMessage>> GetEmailsByFilterAsync(EmailFilter filter);
        Task<EmailMessage?> GetEmailByIdAsync(string messageId);
        Task<IEnumerable<EmailMessage>> GetEmailsByThreadAsync(string threadId);
        Task<bool> MarkAsReadAsync(string messageId);
        Task<bool> ReplyToEmailAsync(string messageId, string replyContent);
        Task<bool> ForwardEmailAsync(string messageId, string forwardTo);
        Task<IEnumerable<EmailFilter>> GetEmailFiltersAsync();
        Task<EmailFilter> CreateEmailFilterAsync(EmailFilter filter);
        Task<EmailFilter> UpdateEmailFilterAsync(EmailFilter filter);
        Task DeleteEmailFilterAsync(int filterId);
    }

    public class EmailMessage
    {
        public string Id { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public DateTime ReceivedDateTime { get; set; }
        public bool IsRead { get; set; }
        public string ThreadId { get; set; } = string.Empty;
        public string? ConversationId { get; set; }
        public List<string> Attachments { get; set; } = new List<string>();
        public string? Cc { get; set; }
        public string? Bcc { get; set; }
    }
} 