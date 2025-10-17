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

        /// <summary>
        /// Gauna visus bilietus. Studentai mato tik savo bilietus.
        /// </summary>
        /// <returns>Bilietų sąrašą.</returns>
        /// <response code="200">Gražina bilietų sąrašą.</response>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value ?? "0");

            IQueryable<Ticket> query = _context.Tickets.Include(t => t.Comments);

            if (userRole == "Student")
            {
                query = query.Where(t => t.UserId == userId);
            }

            var tickets = await query.ToListAsync();
            return Ok(tickets);
        }

        /// <summary>
        /// Gauna konkretų bilietą pagal ID.
        /// </summary>
        /// <param name="id">Bilieto ID.</param>
        /// <returns>Bilieto objektą.</returns>
        /// <response code="200">Gražina bilietą su komentarais.</response>
        /// <response code="403">Studentas neturi teisės matyti šio bilieto.</response>
        /// <response code="404">Bilietas nerastas.</response>
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

        /// <summary>
        /// Sukuria naują bilietą.
        /// </summary>
        /// <param name="ticket">Bilieto objektas.</param>
        /// <returns>Sukurtą bilietą.</returns>
        /// <response code="201">Bilietas sėkmingai sukurtas.</response>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(Ticket ticket)
        {
            ticket.Status = "Open";
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, ticket);
        }

        /// <summary>
        /// Atnaujina bilieto duomenis pagal ID.
        /// </summary>
        /// <param name="id">Bilieto ID.</param>
        /// <param name="ticket">Atnaujinti bilieto duomenys.</param>
        /// <returns>Atnaujintą bilietą.</returns>
        /// <response code="200">Bilietas sėkmingai atnaujintas.</response>
        /// <response code="400">Neteisingas bilieto ID.</response>
        /// <response code="403">Neturite teisės atnaujinti šio bilieto.</response>
        /// <response code="404">Bilietas nerastas.</response>
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

            if (userRole == "Admin")
            {
                existing.TypeId = ticket.TypeId;
                existing.Status = ticket.Status;
            }

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        /// <summary>
        /// Ištrina bilietą pagal ID.
        /// </summary>
        /// <param name="id">Bilieto ID.</param>
        /// <returns>Nėra turinio.</returns>
        /// <response code="204">Bilietas sėkmingai ištrintas.</response>
        /// <response code="403">Neturite teisės ištrinti šio bilieto.</response>
        /// <response code="404">Bilietas nerastas.</response>
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
        [HttpGet("byType/{typeId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] int? typeId)
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value ?? "0");

            IQueryable<Ticket> query = _context.Tickets.Include(t => t.Comments);

            if (userRole == "Student")
                query = query.Where(t => t.UserId == userId);

            if (typeId.HasValue)
                query = query.Where(t => t.TypeId == typeId.Value);

            var tickets = await query.ToListAsync();
            return Ok(tickets);
        }

    }
}
