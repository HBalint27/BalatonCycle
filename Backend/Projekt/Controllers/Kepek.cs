using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt.Model;

namespace Projekt.Controllers
{
    [Authorize] 
    [Route("api/[controller]")]
    [ApiController]
    public class KepekController : ControllerBase
    {
        private readonly Context _context;

        public KepekController(Context context)
        {
            _context = context;
        }

        [AllowAnonymous] 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Kepek>>> Get()
        {
            return Ok(await _context.Kepek.ToListAsync());
        }

        [AllowAnonymous] 
        [HttpGet("szoba/{sid}")]
        public async Task<ActionResult<IEnumerable<Kepek>>> GetBySzoba(int sid)
        {
            var kepek = await _context.Kepek
                                      .Where(k => k.Sid == sid)
                                      .ToListAsync();
            return Ok(kepek);
        }

        [Authorize(Policy = "Kepek.Create")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Kepek kep)
        {
            if (kep == null) return BadRequest();

            kep.FeltoltveEkkor = DateTime.Now;
            _context.Kepek.Add(kep);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = kep.Kid }, kep);
        }

        [Authorize(Policy = "Kepek.Delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var kep = await _context.Kepek.FindAsync(id);
            if (kep == null) return NotFound();

            _context.Kepek.Remove(kep);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Kép rekord törölve", id });
        }
    }
}