using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt.Model;

namespace Projekt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Szallas_ErtekelesController : ControllerBase
    {
        private readonly Context _context;

        public Szallas_ErtekelesController(Context context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _context.Szallas_Ertekeles
                .Include("Szallas")
                .Include("Ertekeles")
                .ToListAsync());
        }

        [HttpGet("{szid}/{eid}")]
        public async Task<IActionResult> Get(int szid, int eid)
        {
            var kapcsolat = await _context.Szallas_Ertekeles
                .Include("Szallas")
                .Include("Ertekeles")
                .FirstOrDefaultAsync(x => x.Szid == szid && x.Eid == eid);

            if (kapcsolat == null) return NotFound();

            return Ok(kapcsolat);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Szallas_ertekeles szallasErtekeles)
        {
            _context.Szallas_Ertekeles.Add(szallasErtekeles);
            await _context.SaveChangesAsync();
            return StatusCode(201, szallasErtekeles);
        }

        [HttpDelete("{szid}/{eid}")]
        public async Task<IActionResult> Delete(int szid, int eid)
        {
            var kapcsolat = await _context.Szallas_Ertekeles
                .FirstOrDefaultAsync(x => x.Szid == szid && x.Eid == eid);

            if (kapcsolat == null) return NotFound();

            _context.Szallas_Ertekeles.Remove(kapcsolat);
            await _context.SaveChangesAsync();

            return Ok(kapcsolat);
        }
    }
}
