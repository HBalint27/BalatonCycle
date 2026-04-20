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
    public class SzolgaltatasokController : ControllerBase
    {
        private readonly Context _context;
        public SzolgaltatasokController(Context context) { _context = context; }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _context.Szolgaltatasok.ToListAsync());

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await _context.Szolgaltatasok.FirstOrDefaultAsync(s => s.Szoid == id);
            return item == null ? NotFound() : Ok(item);
        }

        [Authorize(Policy = "Szolgaltatasok.Create")]
        [HttpPost]
        public async Task<IActionResult> Post(Szolgaltatasok sz)
        {
            _context.Szolgaltatasok.Add(sz);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = sz.Szoid }, sz);
        }

        [Authorize(Policy = "Szolgaltatasok.Update")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Szolgaltatasok sz)
        {
            var old = await _context.Szolgaltatasok.FirstOrDefaultAsync(s => s.Szoid == id);
            if (old == null) return NotFound();

            old.Nev = sz.Nev;
            await _context.SaveChangesAsync();
            return Ok(old);
        }

        [Authorize(Policy = "Szolgaltatasok.Delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Szolgaltatasok.FirstOrDefaultAsync(s => s.Szoid == id);
            if (item == null) return NotFound();
            _context.Szolgaltatasok.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(item);
        }
    }
}
