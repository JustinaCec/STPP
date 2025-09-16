using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHelpDeskAPI.Data;
using SchoolHelpDeskAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace SchoolHelpDeskAPI.Controllers
{
    [Route("api/tickets/{ticketId}/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly SchoolHelpDeskContext _context;

        public CommentController(SchoolHelpDeskContext context)
        {
            _context = context;
        }

        // GET: api/tickets/{ticketId}/comment
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll(int ticketId)
        {
            var comments = await _context.Comments
                                         .Where(c => c.TicketId == ticketId)
                                         .ToListAsync();
            return Ok(comments);
        }

        // GET: api/tickets/{ticketId}/comment/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int ticketId, int id)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id && c.TicketId == ticketId);
            if (comment == null) return NotFound();
            return Ok(comment);
        }

        // POST: api/tickets/{ticketId}/comment
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(int ticketId, Comment comment)
        {
            comment.TicketId = ticketId;
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { ticketId = ticketId, id = comment.Id }, comment);
        }

        // PUT: api/tickets/{ticketId}/comment/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int ticketId, int id, Comment comment)
        {
            if (id != comment.Id) return BadRequest();

            var existing = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id && c.TicketId == ticketId);
            if (existing == null) return NotFound();

            existing.Body = comment.Body;
            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        // DELETE: api/tickets/{ticketId}/comment/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int ticketId, int id)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id && c.TicketId == ticketId);
            if (comment == null) return NotFound();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
