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

        /// <summary>
        /// Gauna visus komentarus pagal bilieto ID.
        /// </summary>
        /// <param name="ticketId">Bilieto ID.</param>
        /// <returns>Komentarų sąrašą</returns>
        /// <response code="200">Gražina komentarus.</response>
        /// <response code="403">Studentas neturi prieigos prie šio bilieto komentarų.</response>
        /// <response code="404">Bilietas nerastas.</response>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll(int ticketId)
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value ?? "0");

            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null) return NotFound();

            if (userRole == "Student" && ticket.UserId != userId)
                return Forbid();

            var comments = await _context.Comments
                                         .Where(c => c.TicketId == ticketId)
                                         .ToListAsync();
            return Ok(comments);
        }

        /// <summary>
        /// Gauna konkretų komentarą pagal jo ID ir bilieto ID.
        /// </summary>
        /// <param name="ticketId">Bilieto ID.</param>
        /// <param name="id">Komentaro ID.</param>
        /// <returns>Komentaro objektą.</returns>
        /// <response code="200">Gražina komentarą.</response>
        /// <response code="403">Studentas neturi prieigos prie šio komentaro.</response>
        /// <response code="404">Komentaras arba bilietas nerastas.</response>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int ticketId, int id)
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value ?? "0");

            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null) return NotFound();

            if (userRole == "Student" && ticket.UserId != userId)
                return Forbid();

            var comment = await _context.Comments
                                        .FirstOrDefaultAsync(c => c.Id == id && c.TicketId == ticketId);
            if (comment == null) return NotFound();

            return Ok(comment);
        }

        /// <summary>
        /// Sukuria naują komentarą prie konkretaus bilieto.
        /// </summary>
        /// <param name="ticketId">Bilieto ID.</param>
        /// <param name="comment">Komentaro objektas.</param>
        /// <returns>Sukurtą komentarą.</returns>
        /// <response code="201">Komentaras sėkmingai sukurtas.</response>
        /// <response code="403">Studentas neturi teisės kurti komentaro šiam bilietui.</response>
        /// <response code="404">Bilietas nerastas.</response>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(int ticketId, Comment comment)
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value ?? "0");

            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null) return NotFound();

            if (userRole == "Student" && ticket.UserId != userId)
                return Forbid();

            comment.TicketId = ticketId;
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { ticketId = ticketId, id = comment.Id }, comment);
        }

        /// <summary>
        /// Atnaujina komentarą pagal jo ID ir bilieto ID.
        /// </summary>
        /// <param name="ticketId">Bilieto ID.</param>
        /// <param name="id">Komentaro ID.</param>
        /// <param name="comment">Atnaujintas komentaras.</param>
        /// <returns>Atnaujintą komentarą.</returns>
        /// <response code="200">Komentaras atnaujintas.</response>
        /// <response code="400">Neteisingas komentarų ID.</response>
        /// <response code="403">Neturite prieigos atnaujinti šio komentaro.</response>
        /// <response code="404">Komentaras arba bilietas nerastas.</response>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int ticketId, int id, Comment comment)
        {
            if (id != comment.Id) return BadRequest();

            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value ?? "0");

            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null) return NotFound();

            var existing = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id && c.TicketId == ticketId);
            if (existing == null) return NotFound();

            if (existing.UserId != userId)
                return Forbid();

            existing.Body = comment.Body;
            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        /// <summary>
        /// Ištrina komentarą pagal jo ID ir bilieto ID.
        /// </summary>
        /// <param name="ticketId">Bilieto ID.</param>
        /// <param name="id">Komentaro ID.</param>
        /// <returns>Nėra turinio.</returns>
        /// <response code="204">Komentaras sėkmingai ištrintas.</response>
        /// <response code="403">Neturite prieigos ištrinti šio komentaro.</response>
        /// <response code="404">Komentaras arba bilietas nerastas.</response>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int ticketId, int id)
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value ?? "0");

            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null) return NotFound();

            if (userRole == "Student" && ticket.UserId != userId)
                return Forbid();

            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id && c.TicketId == ticketId);
            if (comment == null) return NotFound();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
