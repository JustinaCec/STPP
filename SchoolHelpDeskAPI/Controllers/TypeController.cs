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

        /// <summary>
        /// Gauna visų bilietų tipų sąrašą.
        /// </summary>
        /// <returns>Bilietų tipų sąrašą.</returns>
        /// <response code="200">Gražina bilietų tipų sąrašą.</response>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var types = await _context.TicketTypes.ToListAsync();
            return Ok(types);
        }

        /// <summary>
        /// Gauna bilieto tipo informaciją pagal ID.
        /// </summary>
        /// <param name="id">Bilieto tipo ID.</param>
        /// <returns>Bilieto tipo objektą.</returns>
        /// <response code="200">Gražina bilieto tipo informaciją.</response>
        /// <response code="404">Bilieto tipas nerastas.</response>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var type = await _context.TicketTypes.FindAsync(id);
            if (type == null) return NotFound();
            return Ok(type);
        }

        /// <summary>
        /// Sukuria naują bilieto tipą.
        /// </summary>
        /// <param name="type">Bilieto tipo objektas.</param>
        /// <returns>Sukurtą bilieto tipą.</returns>
        /// <response code="201">Bilieto tipas sėkmingai sukurtas.</response>
        /// <response code="400">Neteisingi įvesties duomenys.</response>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(TicketType type)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.TicketTypes.Add(type);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = type.Id }, type);
        }

        /// <summary>
        /// Atnaujina bilieto tipo informaciją pagal ID.
        /// </summary>
        /// <param name="id">Bilieto tipo ID.</param>
        /// <param name="type">Atnaujinti duomenys.</param>
        /// <returns>Atnaujintą bilieto tipą.</returns>
        /// <response code="200">Bilieto tipas sėkmingai atnaujintas.</response>
        /// <response code="400">Neteisingas bilieto tipo ID.</response>
        /// <response code="404">Bilieto tipas nerastas.</response>
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

        /// <summary>
        /// Ištrina bilieto tipą pagal ID.
        /// </summary>
        /// <param name="id">Bilieto tipo ID.</param>
        /// <returns>Nėra turinio.</returns>
        /// <response code="204">Bilieto tipas sėkmingai ištrintas.</response>
        /// <response code="404">Bilieto tipas nerastas.</response>
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
