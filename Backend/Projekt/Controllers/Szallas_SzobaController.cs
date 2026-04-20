using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt.Model;

namespace Projekt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SzallasSzobaController : ControllerBase
    {
        private readonly Context _context;
        public SzallasSzobaController(Context context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _context.Szallas_Szoba.ToListAsync());

        // Egyedi lekérdezés összetett kulccsal
        [HttpGet("{szid}/{sid}")]
        public async Task<IActionResult> Get(int szid, int sid)
        {
            var kapcsolat = await _context.Szallas_Szoba
                .FirstOrDefaultAsync(x => x.Szid == szid && x.Sid == sid);
            return kapcsolat == null ? NotFound() : Ok(kapcsolat);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Szallas_szoba szallas_szoba)
        {
            _context.Szallas_Szoba.Add(szallas_szoba);
            await _context.SaveChangesAsync();
            return StatusCode(201, szallas_szoba);
        }

        [HttpDelete("{szid}/{sid}")]
        public async Task<IActionResult> Delete(int szid, int sid)
        {
            var kapcsolat = await _context.Szallas_Szoba
                .FirstOrDefaultAsync(x => x.Szid == szid && x.Sid == sid);

            if (kapcsolat == null) return NotFound();

            _context.Szallas_Szoba.Remove(kapcsolat);
            await _context.SaveChangesAsync();
            return Ok(kapcsolat);
        }
    }
}
