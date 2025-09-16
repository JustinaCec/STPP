using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHelpDeskAPI.Data;
using SchoolHelpDeskAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace SchoolHelpDeskAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketTypeController : ControllerBase
    {
        private readonly SchoolHelpDeskContext _context;

        public TicketTypeController(SchoolHelpDeskContext context)
        {
            _context = context;
        }

        // GET: api/tickettype
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var types = await _context.TicketTypes.ToListAsync();
            return Ok(types);
        }

        // GET: api/tickettype/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var type = await _context.TicketTypes.FindAsync(id);
            if (type == null) return NotFound();
            return Ok(type);
        }

        // POST: api/tickettype
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(TicketType type)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.TicketTypes.Add(type);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = type.Id }, type);
        }

        // PUT: api/tickettype/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, TicketType type)
        {
            if (id != type.Id) return BadRequest();
            _context.Entry(type).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.TicketTypes.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            return Ok(type);
        }

        // DELETE: api/tickettype/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var type = await _context.TicketTypes.FindAsync(id);
            if (type == null) return NotFound();

            _context.TicketTypes.Remove(type);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
