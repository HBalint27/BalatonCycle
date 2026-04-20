using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt.Model;

namespace Projekt.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ErtekelesController : ControllerBase
    {
        private readonly Context _context;
        public ErtekelesController(Context context) { _context = context; }

        [Authorize(Policy = "Ertekeles.Read")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _context.Ertekelesek.ToListAsync());
        }

        [Authorize(Policy = "Ertekeles.Read")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var ertekeles = await _context.Ertekelesek.FirstOrDefaultAsync(e => e.Eid == id);
            return ertekeles == null ? NotFound() : Ok(ertekeles);
        }

        [Authorize(Policy = "Ertekeles.Create")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Ertekeles ertekeles)
        {
            // Biztosítjuk, hogy legyen dátum
            if (ertekeles.Datum == DateTime.MinValue) ertekeles.Datum = DateTime.Now;

            // ELLENŐRZÉS: Létezik-e a szállás, amihez az értékelést küldik?
            var szallasLetezik = await _context.Szallasok.AnyAsync(s => s.Szid == ertekeles.Szid);
            if (!szallasLetezik)
            {
                return BadRequest("A megadott szállás azonosító nem létezik.");
            }

            // Mentjük az értékelést (A Szid már benne van az objektumban a MySQL idegen kulcs miatt)
            _context.Ertekelesek.Add(ertekeles);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = ertekeles.Eid }, ertekeles);
        }

        [Authorize(Policy = "Ertekeles.Update")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Ertekeles ertekeles)
        {
            var old = await _context.Ertekelesek.FirstOrDefaultAsync(e => e.Eid == id);
            if (old == null) return NotFound();

            old.Szoveg = ertekeles.Szoveg;
            old.Datum = ertekeles.Datum;
            old.Pont = ertekeles.Pont;
            // Opcionálisan a Szid is módosítható, ha rossz szálláshoz ment volna
            if (ertekeles.Szid != 0) old.Szid = ertekeles.Szid;

            await _context.SaveChangesAsync();
            return Ok(old);
        }

        [Authorize(Policy = "Ertekeles.Delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Ertekelesek.FirstOrDefaultAsync(e => e.Eid == id);
            if (item == null) return NotFound();

            _context.Ertekelesek.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(item);
        }
    }
}