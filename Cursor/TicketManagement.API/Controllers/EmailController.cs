using Microsoft.AspNetCore.Mvc;
using TicketManagement.Core.Interfaces;
using TicketManagement.Core.Models;

namespace TicketManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ITicketService _ticketService;
        private readonly ILogger<EmailController> _logger;

        public EmailController(
            IEmailService emailService,
            ITicketService ticketService,
            ILogger<EmailController> logger)
        {
            _emailService = emailService;
            _ticketService = ticketService;
            _logger = logger;
        }

        [HttpGet("unread")]
        public async Task<ActionResult<IEnumerable<EmailMessage>>> GetUnreadEmails()
        {
            try
            {
                var emails = await _emailService.GetUnreadEmailsAsync();
                return Ok(emails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread emails");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("filters")]
        public async Task<ActionResult<IEnumerable<EmailFilter>>> GetEmailFilters()
        {
            try
            {
                var filters = await _emailService.GetEmailFiltersAsync();
                return Ok(filters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting email filters");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("filters")]
        public async Task<ActionResult<EmailFilter>> CreateEmailFilter(EmailFilter filter)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdFilter = await _emailService.CreateEmailFilterAsync(filter);
                return CreatedAtAction(nameof(GetEmailFilters), createdFilter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating email filter");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("filters/{id}")]
        public async Task<ActionResult<EmailFilter>> UpdateEmailFilter(int id, EmailFilter filter)
        {
            try
            {
                if (id != filter.Id)
                {
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedFilter = await _emailService.UpdateEmailFilterAsync(filter);
                return Ok(updatedFilter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating email filter {FilterId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("filters/{id}")]
        public async Task<IActionResult> DeleteEmailFilter(int id)
        {
            try
            {
                await _emailService.DeleteEmailFilterAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting email filter {FilterId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("process")]
        public async Task<ActionResult<bool>> ProcessEmailFilters()
        {
            try
            {
                var result = await _ticketService.ProcessEmailFiltersAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing email filters");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("create-ticket")]
        public async Task<ActionResult<Ticket>> CreateTicketFromEmail([FromBody] CreateTicketFromEmailRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var emailMessage = new EmailMessage
                {
                    Id = request.EmailId,
                    Subject = request.Subject,
                    Body = request.Body,
                    From = request.From,
                    FromEmail = request.FromEmail,
                    ReceivedDateTime = request.ReceivedDateTime,
                    ThreadId = request.ThreadId ?? string.Empty
                };

                var ticket = await _ticketService.CreateTicketFromEmailAsync(emailMessage);
                return Ok(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket from email");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("reply")]
        public async Task<ActionResult<bool>> ReplyToEmail([FromBody] ReplyEmailRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _emailService.ReplyToEmailAsync(request.MessageId, request.ReplyContent);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error replying to email {MessageId}", request.MessageId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("forward")]
        public async Task<ActionResult<bool>> ForwardEmail([FromBody] ForwardEmailRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _emailService.ForwardEmailAsync(request.MessageId, request.ForwardTo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding email {MessageId}", request.MessageId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("mark-read")]
        public async Task<ActionResult<bool>> MarkEmailAsRead([FromBody] MarkEmailReadRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _emailService.MarkAsReadAsync(request.MessageId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking email as read {MessageId}", request.MessageId);
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class CreateTicketFromEmailRequest
    {
        public string EmailId { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public DateTime ReceivedDateTime { get; set; }
        public string? ThreadId { get; set; }
    }

    public class ReplyEmailRequest
    {
        public string MessageId { get; set; } = string.Empty;
        public string ReplyContent { get; set; } = string.Empty;
    }

    public class ForwardEmailRequest
    {
        public string MessageId { get; set; } = string.Empty;
        public string ForwardTo { get; set; } = string.Empty;
    }

    public class MarkEmailReadRequest
    {
        public string MessageId { get; set; } = string.Empty;
    }
} 