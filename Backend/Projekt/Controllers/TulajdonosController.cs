using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt.Model;

namespace Projekt.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TulajdonosController : ControllerBase
    {
        private readonly Context _context;

        public TulajdonosController(Context context)
        {
            _context = context;
        }

        [Authorize(Policy = "Tulajdonos.Read")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _context.Tulajdonosok.ToListAsync());
        }

        [Authorize(Policy = "Tulajdonos.Read")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, [FromQuery] bool ext)
        {
            Tulajdonos? tulajdonos = null;
            if (ext)
            {
                tulajdonos = await _context.Tulajdonosok
                                .Include(t => t.Szallasok)
                                .Include(t => t.Felhasznalo)
                                .FirstOrDefaultAsync(p => p.Tid == id);
            }
            else
            {
                tulajdonos = await _context.Tulajdonosok.FirstOrDefaultAsync(p => p.Tid == id);
            }

            if (tulajdonos == null) return NotFound();
            return Ok(tulajdonos);
        }

        [Authorize(Policy = "Tulajdonos.Create")]
        [HttpPost]
        public async Task<IActionResult> Post(int id, Tulajdonos tulajdonos)
        {
            if (string.IsNullOrWhiteSpace(tulajdonos.Nev)) return BadRequest("Név megadása kötelező");

            _context.Tulajdonosok.Add(tulajdonos);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = tulajdonos.Tid }, tulajdonos);
        }

        [Authorize(Policy = "Tulajdonos.Update")]
        [HttpPut]
        public async Task<IActionResult> Put(int id, Tulajdonos tulajdonos)
        {
            var oldtulajdonos = await _context.Tulajdonosok.FirstOrDefaultAsync(p => p.Tid == id);

            if (oldtulajdonos == null) return NotFound();

            oldtulajdonos.Nev = tulajdonos.Nev;
            oldtulajdonos.Email = tulajdonos.Email;
            oldtulajdonos.Telefonszam = tulajdonos.Telefonszam;
            oldtulajdonos.Fid = tulajdonos.Fid;

            await _context.SaveChangesAsync();
            return Ok(oldtulajdonos);
        }

        [Authorize(Policy = "Tulajdonos.Delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tulajdonos = await _context.Tulajdonosok.FirstOrDefaultAsync(p => p.Tid == id);

            if (tulajdonos == null) return NotFound();

            _context.Tulajdonosok.Remove(tulajdonos);
            await _context.SaveChangesAsync();
            return Ok(tulajdonos);
        }
    }
}