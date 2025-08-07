using Microsoft.AspNetCore.Mvc;
using TicketManagement.Core.Interfaces;
using TicketManagement.Core.Models;

namespace TicketManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(ITicketService ticketService, ILogger<TicketsController> logger)
        {
            _ticketService = ticketService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets()
        {
            try
            {
                var tickets = await _ticketService.GetAllTicketsAsync();
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tickets");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Ticket>> GetTicket(int id)
        {
            try
            {
                var ticket = await _ticketService.GetTicketByIdAsync(id);
                if (ticket == null)
                {
                    return NotFound();
                }

                return Ok(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket {TicketId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("number/{ticketNumber}")]
        public async Task<ActionResult<Ticket>> GetTicketByNumber(string ticketNumber)
        {
            try
            {
                var ticket = await _ticketService.GetTicketByNumberAsync(ticketNumber);
                if (ticket == null)
                {
                    return NotFound();
                }

                return Ok(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket by number {TicketNumber}", ticketNumber);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTicketsByStatus(TicketStatus status)
        {
            try
            {
                var tickets = await _ticketService.GetTicketsByStatusAsync(status);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tickets by status {Status}", status);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("assignee/{assignee}")]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTicketsByAssignee(string assignee)
        {
            try
            {
                var tickets = await _ticketService.GetTicketsByAssigneeAsync(assignee);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tickets by assignee {Assignee}", assignee);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("priority/{priority}")]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTicketsByPriority(TicketPriority priority)
        {
            try
            {
                var tickets = await _ticketService.GetTicketsByPriorityAsync(priority);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tickets by priority {Priority}", priority);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTicketsByCategory(TicketCategory category)
        {
            try
            {
                var tickets = await _ticketService.GetTicketsByCategoryAsync(category);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tickets by category {Category}", category);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Ticket>>> SearchTickets([FromQuery] string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                {
                    return BadRequest("Search term is required");
                }

                var tickets = await _ticketService.SearchTicketsAsync(q);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching tickets with term {SearchTerm}", q);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetOverdueTickets()
        {
            try
            {
                var tickets = await _ticketService.GetOverdueTicketsAsync();
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overdue tickets");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("counts")]
        public async Task<ActionResult<Dictionary<TicketStatus, int>>> GetTicketCounts()
        {
            try
            {
                var counts = await _ticketService.GetTicketCountsByStatusAsync();
                return Ok(counts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket counts");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Ticket>> CreateTicket(Ticket ticket)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdTicket = await _ticketService.CreateTicketAsync(ticket);
                return CreatedAtAction(nameof(GetTicket), new { id = createdTicket.Id }, createdTicket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicket(int id, Ticket ticket)
        {
            try
            {
                if (id != ticket.Id)
                {
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedTicket = await _ticketService.UpdateTicketAsync(ticket);
                return Ok(updatedTicket);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket {TicketId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult<Ticket>> UpdateTicketStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var updatedTicket = await _ticketService.UpdateTicketStatusAsync(id, request.Status, request.Reason);
                return Ok(updatedTicket);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket status {TicketId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}/assign")]
        public async Task<ActionResult<Ticket>> AssignTicket(int id, [FromBody] AssignTicketRequest request)
        {
            try
            {
                var updatedTicket = await _ticketService.AssignTicketAsync(id, request.Assignee, request.Reason);
                return Ok(updatedTicket);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning ticket {TicketId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{id}/comments")]
        public async Task<ActionResult<Ticket>> AddComment(int id, TicketComment comment)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedTicket = await _ticketService.AddCommentAsync(id, comment);
                return Ok(updatedTicket);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment to ticket {TicketId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            try
            {
                await _ticketService.DeleteTicketAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ticket {TicketId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class UpdateStatusRequest
    {
        public TicketStatus Status { get; set; }
        public string? Reason { get; set; }
    }

    public class AssignTicketRequest
    {
        public string Assignee { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }
} 