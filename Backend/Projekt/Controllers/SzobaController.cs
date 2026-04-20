using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt.Model;

namespace Projekt.Controllers
{   
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SzobaController : ControllerBase
    {
        private readonly Context _context; 
        public SzobaController(Context context) { _context = context; }

        [Authorize(Policy ="Szoba.Read")]
        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _context.Szobak.ToListAsync());

        [Authorize(Policy = "Szoba.Read")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {           
            var szoba = await _context.Szobak
                .Include(s => s.Kepek) 
                .FirstOrDefaultAsync(s => s.Sid == id);
            return szoba == null ? NotFound() : Ok(szoba);
        }

        [Authorize(Policy = "Szoba.Create")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Szoba szoba)
        {
            _context.Szobak.Add(szoba);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = szoba.Sid }, szoba);
        }

        [Authorize(Policy = "Szoba.Update")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Szoba szoba)
        {
            var old = await _context.Szobak.FirstOrDefaultAsync(s => s.Sid == id);
            if (old == null) return NotFound();

            old.Statusz = szoba.Statusz;
            old.Szid = szoba.Szid; 

            await _context.SaveChangesAsync();
            return Ok(old);
        }

        [Authorize(Policy = "Szoba.Delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Szobak.FirstOrDefaultAsync(s => s.Sid == id);
            if (item == null) return NotFound();

            _context.Szobak.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Szoba törölve", item });
        }
    }
}