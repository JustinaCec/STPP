using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHelpDeskAPI.Data;
using SchoolHelpDeskAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace SchoolHelpDeskAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly SchoolHelpDeskContext _context;

        public TicketController(SchoolHelpDeskContext context)
        {
            _context = context;
        }

        // GET: api/ticket
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var tickets = await _context.Tickets.Include(t => t.Comments).ToListAsync();
            return Ok(tickets);
        }

        // GET: api/ticket/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var ticket = await _context.Tickets.Include(t => t.Comments)
                                               .FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound();

            // Student can only see own tickets
            var userRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value ?? "0");

            if (userRole == "Student" && ticket.UserId != userId)
                return Forbid();

            return Ok(ticket);
        }

        // POST: api/ticket
        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Create(Ticket ticket)
        {
            ticket.Status = "Open";
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
        }

        // PUT: api/ticket/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, Ticket ticket)
        {
            if (id != ticket.Id) return BadRequest();

            var existing = await _context.Tickets.FindAsync(id);
            if (existing == null) return NotFound();

            var userRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value ?? "0");

            if (userRole == "Student" && existing.UserId != userId)
                return Forbid();

            existing.Title = ticket.Title;
            existing.Description = ticket.Description;
            existing.TypeId = ticket.TypeId;
            if (userRole == "Admin") existing.Status = ticket.Status;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        // DELETE: api/ticket/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            var userRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value ?? "0");

            if (userRole == "Student" && ticket.UserId != userId)
                return Forbid();

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
