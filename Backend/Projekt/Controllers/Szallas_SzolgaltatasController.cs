using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt.Model;

namespace Projekt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Szallas_SzolgaltatasController : ControllerBase
    {
        private readonly Context _context;

        public Szallas_SzolgaltatasController(Context context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _context.Szallas_Szolgaltatas
                .Include("Szallas")
                .Include("Szolgaltatasok")
                .ToListAsync());
        }

        [HttpGet("{szid}/{szoid}")]
        public async Task<IActionResult> Get(int szid, int szoid)
        {
            var kapcsolat = await _context.Szallas_Szolgaltatas
                .Include("Szallas")
                .Include("Szolgaltatasok")
                .FirstOrDefaultAsync(x => x.Szid == szid && x.Szoid == szoid);

            if (kapcsolat == null) return NotFound();

            return Ok(kapcsolat);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Szallas_szolgaltatas szallasSzolgaltatas)
        {
            _context.Szallas_Szolgaltatas.Add(szallasSzolgaltatas);
            await _context.SaveChangesAsync();
            return StatusCode(201, szallasSzolgaltatas);
        }

        [HttpDelete("{szid}/{szoid}")]
        public async Task<IActionResult> Delete(int szid, int szoid)
        {
            var kapcsolat = await _context.Szallas_Szolgaltatas
                .FirstOrDefaultAsync(x => x.Szid == szid && x.Szoid == szoid);

            if (kapcsolat == null) return NotFound();

            _context.Szallas_Szolgaltatas.Remove(kapcsolat);
            await _context.SaveChangesAsync();

            return Ok(kapcsolat);
        }
    }
}
