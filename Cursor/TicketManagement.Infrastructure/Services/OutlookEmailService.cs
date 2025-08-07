using Microsoft.Graph;
using Microsoft.Extensions.Logging;
using TicketManagement.Core.Interfaces;
using TicketManagement.Core.Models;

namespace TicketManagement.Infrastructure.Services
{
    public class OutlookEmailService : IEmailService
    {
        private readonly GraphServiceClient _graphClient;
        private readonly ILogger<OutlookEmailService> _logger;

        public OutlookEmailService(GraphServiceClient graphClient, ILogger<OutlookEmailService> logger)
        {
            _graphClient = graphClient;
            _logger = logger;
        }

        public async Task<IEnumerable<EmailMessage>> GetUnreadEmailsAsync()
        {
            try
            {
                var messages = await _graphClient.Me.Messages
                    .Request()
                    .Filter("isRead eq false")
                    .OrderBy("receivedDateTime desc")
                    .Top(50)
                    .GetAsync();

                return messages.Select(MapToEmailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread emails");
                return Enumerable.Empty<EmailMessage>();
            }
        }

        public async Task<IEnumerable<EmailMessage>> GetEmailsByFilterAsync(EmailFilter filter)
        {
            try
            {
                var query = _graphClient.Me.Messages.Request();

                // Build filter based on EmailFilter criteria
                var filters = new List<string>();

                if (!string.IsNullOrEmpty(filter.FromEmail))
                {
                    filters.Add($"from/emailAddress/address eq '{filter.FromEmail}'");
                }

                if (!string.IsNullOrEmpty(filter.FromDomain))
                {
                    filters.Add($"from/emailAddress/address endswith '@{filter.FromDomain}'");
                }

                if (!string.IsNullOrEmpty(filter.SubjectContains))
                {
                    filters.Add($"contains(subject, '{filter.SubjectContains}')");
                }

                if (filters.Any())
                {
                    query.Filter(string.Join(" and ", filters));
                }

                var messages = await query
                    .OrderBy("receivedDateTime desc")
                    .Top(50)
                    .GetAsync();

                var emailMessages = messages.Select(MapToEmailMessage).ToList();

                // Apply body content filter if specified
                if (!string.IsNullOrEmpty(filter.BodyContains))
                {
                    emailMessages = emailMessages
                        .Where(m => m.Body.Contains(filter.BodyContains, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                return emailMessages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting emails by filter");
                return Enumerable.Empty<EmailMessage>();
            }
        }

        public async Task<EmailMessage?> GetEmailByIdAsync(string messageId)
        {
            try
            {
                var message = await _graphClient.Me.Messages[messageId].Request().GetAsync();
                return MapToEmailMessage(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting email by ID: {MessageId}", messageId);
                return null;
            }
        }

        public async Task<IEnumerable<EmailMessage>> GetEmailsByThreadAsync(string threadId)
        {
            try
            {
                var messages = await _graphClient.Me.Messages
                    .Request()
                    .Filter($"conversationId eq '{threadId}'")
                    .OrderBy("receivedDateTime desc")
                    .GetAsync();

                return messages.Select(MapToEmailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting emails by thread: {ThreadId}", threadId);
                return Enumerable.Empty<EmailMessage>();
            }
        }

        public async Task<bool> MarkAsReadAsync(string messageId)
        {
            try
            {
                var message = new Message
                {
                    IsRead = true
                };

                await _graphClient.Me.Messages[messageId].Request().UpdateAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking email as read: {MessageId}", messageId);
                return false;
            }
        }

        public async Task<bool> ReplyToEmailAsync(string messageId, string replyContent)
        {
            try
            {
                // 1. Create reply draft
                var replyDraft = await _graphClient.Me.Messages[messageId].CreateReply().Request().PostAsync();
                // 2. Update the body
                replyDraft.Body = new ItemBody
                {
                    Content = replyContent,
                    ContentType = BodyType.Text
                };
                await _graphClient.Me.Messages[replyDraft.Id].Request().UpdateAsync(replyDraft);
                // 3. Send the reply
                await _graphClient.Me.Messages[replyDraft.Id].Send().Request().PostAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error replying to email: {MessageId}", messageId);
                return false;
            }
        }

        public async Task<bool> ForwardEmailAsync(string messageId, string forwardTo)
        {
            try
            {
                // 1. Create forward draft
                var forwardDraft = await _graphClient.Me.Messages[messageId].CreateForward().Request().PostAsync();
                // 2. Update the body and recipients
                forwardDraft.Body = new ItemBody
                {
                    Content = string.Empty,
                    ContentType = BodyType.Text
                };
                forwardDraft.ToRecipients = new List<Recipient>
                {
                    new Recipient
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = forwardTo
                        }
                    }
                };
                await _graphClient.Me.Messages[forwardDraft.Id].Request().UpdateAsync(forwardDraft);
                // 3. Send the forward
                await _graphClient.Me.Messages[forwardDraft.Id].Send().Request().PostAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding email: {MessageId}", messageId);
                return false;
            }
        }

        public async Task<IEnumerable<EmailFilter>> GetEmailFiltersAsync()
        {
            // This would typically come from the database
            // For now, return empty list as filters are managed separately
            return Enumerable.Empty<EmailFilter>();
        }

        public async Task<EmailFilter> CreateEmailFilterAsync(EmailFilter filter)
        {
            // This would typically save to the database
            // For now, return the filter as-is
            return filter;
        }

        public async Task<EmailFilter> UpdateEmailFilterAsync(EmailFilter filter)
        {
            // This would typically update in the database
            // For now, return the filter as-is
            return filter;
        }

        public async Task DeleteEmailFilterAsync(int filterId)
        {
            // This would typically delete from the database
            // For now, do nothing
        }

        private EmailMessage MapToEmailMessage(Microsoft.Graph.Message message)
        {
            return new EmailMessage
            {
                Id = message.Id,
                Subject = message.Subject ?? string.Empty,
                Body = message.Body?.Content ?? string.Empty,
                From = message.From?.EmailAddress?.Name ?? string.Empty,
                FromEmail = message.From?.EmailAddress?.Address ?? string.Empty,
                To = string.Join(", ", message.ToRecipients?.Select(r => r.EmailAddress?.Address) ?? Enumerable.Empty<string>()),
                ReceivedDateTime = message.ReceivedDateTime?.DateTime ?? DateTime.UtcNow,
                IsRead = message.IsRead ?? false,
                ThreadId = message.ConversationId ?? string.Empty,
                ConversationId = message.ConversationId,
                Cc = string.Join(", ", message.CcRecipients?.Select(r => r.EmailAddress?.Address) ?? Enumerable.Empty<string>()),
                Bcc = string.Join(", ", message.BccRecipients?.Select(r => r.EmailAddress?.Address) ?? Enumerable.Empty<string>()),
                Attachments = message.Attachments?.Select(a => a.Name).ToList() ?? new List<string>()
            };
        }
    }
} 