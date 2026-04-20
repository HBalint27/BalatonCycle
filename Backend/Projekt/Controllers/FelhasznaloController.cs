using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt.Auth;
using Projekt.Model;
using System.Text;
using System.Text.Json;
using System.Net.Http;

namespace Projekt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FelhasznaloController : ControllerBase
    {
        private readonly Context _context;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public FelhasznaloController(Context context, IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        [Authorize(Policy = "Felhasznalo.Read")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var felhasznalok = await _context.Felhasznalok.ToListAsync();
            return Ok(felhasznalok);
        }

        [Authorize(Policy = "Felhasznalo.Read")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, [FromQuery] bool ext)
        {
            Felhasznalo? felhasznalo;

            if (ext)
            {
                felhasznalo = await _context.Felhasznalok
                    .Include("Foglalas")
                    .FirstOrDefaultAsync(f => f.Fid == id);
            }
            else
            {
                felhasznalo = await _context.Felhasznalok
                    .FirstOrDefaultAsync(f => f.Fid == id);
            }

            if (felhasznalo == null)
                return NotFound();

            return Ok(felhasznalo);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Felhasznalo felhasznalo)
        {
            // 1. Validációk
            if (string.IsNullOrWhiteSpace(felhasznalo.Jelszo))
                return BadRequest("A jelszó kötelező");

            if (await _context.Felhasznalok.AnyAsync(u => u.Email == felhasznalo.Email))
            {
                return BadRequest("Ez az email cím már foglalt.");
            }

            // Admin jogosultság ellenőrzése
            if (felhasznalo.Statusz != null && felhasznalo.Statusz.ToLower() == "admin")
            {
                var currentUserStatus = User.FindFirst("Statusz")?.Value;
                if (currentUserStatus == null || currentUserStatus.ToLower() != "Admin")
                {
                    return Forbid();
                }
            }
            else
            {
                felhasznalo.Statusz = "User";
            }

            // 2. Jelszó hashelése és mentés
            felhasznalo.Jelszo = PasswordHandler.HashPassword(felhasznalo.Jelszo);

            await _context.Felhasznalok.AddAsync(felhasznalo);
            await _context.SaveChangesAsync();

            // 3. EMAIL KÜLDÉS - Itt hívjuk meg az EmailJS-t
            await SendEmailJSAsync(felhasznalo.Nev, felhasznalo.Email);

            felhasznalo.Jelszo = null;
            return CreatedAtAction(nameof(Get), new { id = felhasznalo.Fid }, felhasznalo);
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] NewsletterRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Érvénytelen e-mail cím.");

            // Meghívjuk a küldő függvényt
            await SendNewsletterEmailAsync(request.Email);

            return Ok(new { message = "Sikeres feliratkozás!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            var felhasznalo = await _context.Felhasznalok
                .FirstOrDefaultAsync(u => u.Email == login.Email);

            if (felhasznalo == null)
                return BadRequest("Hibás email cím vagy jelszó.");

            bool isPasswordValid = PasswordHandler.VerifyPassword(login.Jelszo, felhasznalo.Jelszo);

            if (!isPasswordValid)
                return BadRequest("Hibás email cím vagy jelszó.");

            felhasznalo.Jelszo = null;
            return Ok(new { message = "Sikeres bejelentkezés!", user = felhasznalo });
        }

        [Authorize(Policy = "Felhasznalo.Update")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Felhasznalo felhasznalo)
        {
            var oldFelhasznalo = await _context.Felhasznalok
                .FirstOrDefaultAsync(f => f.Fid == id);

            if (oldFelhasznalo == null)
                return NotFound();

            oldFelhasznalo.Nev = felhasznalo.Nev;
            oldFelhasznalo.Email = felhasznalo.Email;
            oldFelhasznalo.Lakcim = felhasznalo.Lakcim;
            oldFelhasznalo.Telefonszam = felhasznalo.Telefonszam;
            oldFelhasznalo.Statusz = felhasznalo.Statusz;
                        
            if (!string.IsNullOrWhiteSpace(felhasznalo.Jelszo))
            {
                oldFelhasznalo.Jelszo = PasswordHandler.HashPassword(felhasznalo.Jelszo);
            }

            await _context.SaveChangesAsync();
            return Ok(oldFelhasznalo);
        }

        [Authorize(Policy = "Felhasznalo.Delete")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var felhasznalo = await _context.Felhasznalok
                .FirstOrDefaultAsync(f => f.Fid == id);

            if (felhasznalo == null)
                return NotFound();

            _context.Felhasznalok.Remove(felhasznalo);
            await _context.SaveChangesAsync();

            return Ok(felhasznalo);
        }

        // --- PRIVÁT SEGÉDFÜGGVÉNY AZ EMAIL KÜLDÉSHEZ ---

        //Regisztarico utani email
        private async Task SendEmailJSAsync(string userName, string userEmail)
        {
            // KIBUKTATJUK A HIBÁT: Kiíratjuk a terminálba, mit olvasott be
            var pubKey = _config["EmailJS:PublicKey"];
            Console.WriteLine($"--- DEBUG: A beolvasott Public Key: [{pubKey}] ---");

            var payload = new
            {
                service_id = _config["EmailJS:ServiceId"],
                template_id = "template_gl4ylqb", 
                user_id = _config["EmailJS:PublicKey"],
                accessToken = _config["EmailJS:PrivateKey"],
                template_params = new
                {
                    user_name = userName,
                    user_email = userEmail,
                    reply_to = "balatoncycle@gmail.com" 
                }
            };

            var client = _httpClientFactory.CreateClient();
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try 
            {
                var response = await client.PostAsync("https://api.emailjs.com/api/v1.0/email/send", content);
                var responseText = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                    Console.WriteLine("BalatonCycle: Sikeres küldés!");
                else
                    Console.WriteLine($"EmailJS Hiba: {response.StatusCode} - {responseText}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba: {ex.Message}");
            }
        }

        // Hírlevél küldése 

        private async Task SendNewsletterEmailAsync(string userEmail)
        {
            var payload = new
            {
                service_id = _config["EmailJS:ServiceId"],
                template_id = "template_i733t3o", // IDE AZ ÚJ SABLON ID-T ÍRD!
                user_id = _config["EmailJS:PublicKey"],
                accessToken = _config["EmailJS:PrivateKey"],
                template_params = new
                {
                    user_email = userEmail,
                    popular_link = "http://localhost:5173/szallasok?sort=popular"
                }
            };

            var client = _httpClientFactory.CreateClient();
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try 
            {
                var response = await client.PostAsync("https://api.emailjs.com/api/v1.0/email/send", content);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"EmailJS Hírlevél Hiba: {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hírlevél küldési hiba: {ex.Message}");
            }
        }

        public class LoginModel
        {
            public string Email { get; set; } = string.Empty;
            public string Jelszo { get; set; } = string.Empty;
        }

        public class NewsletterRequest
        {
            public string Email { get; set; } = string.Empty;
        }
    }
}