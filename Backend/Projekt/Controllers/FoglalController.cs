using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt.Model;
using System.Security.Claims;
using System.Text.Json;
using System.Text;

namespace Projekt.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FoglalController : ControllerBase
    {
        private readonly Context _context;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public FoglalController(Context context, IConfiguration config, IHttpClientFactory httpClientFactory) 
        { 
            _context = context; 
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        [Authorize(Policy = "Foglal.Read")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _context.Foglalasok
                .Include(f => f.Felhasznalo)
                .Include(f => f.Szallas)
                .ToListAsync());
        }

        [Authorize(Policy = "Foglal.Read")]
        [HttpGet("{fid}/{szid}/{erkezes}")]
        public async Task<IActionResult> Get(int fid, int szid, DateTime erkezes)
        {
            var foglal = await _context.Foglalasok
                .FirstOrDefaultAsync(f => f.Fid == fid && f.Szid == szid && f.ErkezesNap == erkezes);

            if (foglal == null) return NotFound();
            return Ok(foglal);
        }

        [Authorize(Policy = "Foglal.Create")]
        [HttpPost]
        public async Task<IActionResult> Post(Foglal foglal)
        {
            try
            {
                _context.Foglalasok.Add(foglal);
                await _context.SaveChangesAsync();

                // Azonnali visszaigazolás küldése
                var szallas = await _context.Szallasok.FindAsync(foglal.Szid);
                var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value 
                                ?? User.Claims.FirstOrDefault(c => c.Type.ToLower() == "email")?.Value;
                var userName = User.Claims.FirstOrDefault(c => c.Type == "Nev")?.Value ?? "Kedves Vendégünk";

                if (!string.IsNullOrEmpty(userEmail) && szallas != null)
                {
                    await SendConfirmationEmail(userEmail, userName, szallas.Nev, foglal.ErkezesNap);
                }

                return StatusCode(201, foglal);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Hiba a foglalás mentésekor", error = ex.Message });
            }
        }

        private async Task SendConfirmationEmail(string userEmail, string userName, string hotelName, DateTime date)
        {
            var payload = new
            {
                service_id = _config["EmailJS:ServiceId"],
                template_id = "template_q7g1y2p", 
                user_id = _config["EmailJS:PublicKey"],
                accessToken = _config["EmailJS:PrivateKey"],
                template_params = new
                {
                    user_email = userEmail,
                    user_name = userName,
                    hotel_name = hotelName,
                    check_in = date.ToString("yyyy.MM.dd"),
                    check_out = date.AddDays(1).ToString("yyyy.MM.dd"),
                    my_bookings_link = "https://localhost:5173/profile?tab=foglalasok"
                }
            };

            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try 
            {
                await client.PostAsync("https://api.emailjs.com/api/v1.0/email/send", content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba az azonnali visszaigazoló e-mail küldésekor: {ex.Message}");
            }
        }

        [Authorize(Policy = "Foglal.Delete")]
        [HttpDelete("{fid}/{szid}/{erkezes}")]
        public async Task<IActionResult> Delete(int fid, int szid, DateTime erkezes)
        {
            var foglal = await _context.Foglalasok
                .FirstOrDefaultAsync(f => f.Fid == fid && f.Szid == szid && f.ErkezesNap == erkezes);

            if (foglal == null) return NotFound();

            _context.Foglalasok.Remove(foglal);
            await _context.SaveChangesAsync();
            return Ok(foglal);
        }
    }
}