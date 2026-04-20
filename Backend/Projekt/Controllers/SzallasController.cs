using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt.Model;
using System.IO;
using System.Text.Json;
using System.Text;
using System.Security.Claims;

namespace Projekt.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SzallasController : ControllerBase
    {
        private readonly Context _context;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public SzallasController(Context context, IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var szallasok = await _context.Szallasok
                                          .Include(s => s.Ertekelesek)
                                          .Include(s => s.Szobak)
                                          .Include(s => s.SzallasSzolgaltatasok)
                                          .ToListAsync();
            return Ok(szallasok);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, [FromQuery] bool ext)
        {
            IQueryable<Szallas> query = _context.Szallasok;

            if (ext)
            {
                query = query.Include(s => s.Tulajdonos)
                             .Include(s => s.Ertekelesek)
                             .Include(s => s.Szobak)
                                .ThenInclude(sz => sz.Kepek)
                             .Include(s => s.SzallasSzolgaltatasok)
                                .ThenInclude(ss => ss.Szolgaltatasok);
            }
            else
            {
                query = query.Include(s => s.Ertekelesek);
            }

            var szallas = await query.FirstOrDefaultAsync(s => s.Szid == id);

            if (szallas == null) return NotFound();
            return Ok(szallas);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post([FromForm] Szallas szallas, IFormFile? kepadat, [FromForm] List<IFormFile>? galeriaKepek, [FromForm] List<int>? SzolgaltatasIds)
        {
            try
            {
                var userIdClaim = User.FindFirst("Fid")?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { message = "A hirdetés feladásához be kell jelentkeznie." });
                }
                int fid = int.Parse(userIdClaim);

                var tulajdonos = await _context.Tulajdonosok.FirstOrDefaultAsync(t => t.Fid == fid);

                if (tulajdonos == null)
                {
                    var felhasznalo = await _context.Felhasznalok.FirstOrDefaultAsync(u => u.Fid == fid);
                    if (felhasznalo == null) return Unauthorized(new { message = "Felhasználó nem található." });

                    tulajdonos = new Tulajdonos
                    {
                        Fid = fid,
                        Nev = felhasznalo.Nev,
                        Email = felhasznalo.Email,
                        Telefonszam = felhasznalo.Telefonszam
                    };

                    _context.Tulajdonosok.Add(tulajdonos);
                    await _context.SaveChangesAsync();
                }

                szallas.Tid = tulajdonos.Tid;

                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                if (kepadat != null && kepadat.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(kepadat.FileName);
                    var filePath = Path.Combine(folderPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create)) { await kepadat.CopyToAsync(stream); }
                    szallas.Szallaskep = "uploads/" + fileName;
                }

                _context.Szallasok.Add(szallas);
                await _context.SaveChangesAsync();

                var defaultSzoba = new Szoba
                {
                    Statusz = "szabad",
                    Szid = szallas.Szid
                };
                _context.Szobak.Add(defaultSzoba);
                await _context.SaveChangesAsync();

                if (galeriaKepek != null && galeriaKepek.Count > 0)
                {
                    foreach (var galeriaKep in galeriaKepek)
                    {
                        if (galeriaKep.Length > 0)
                        {
                            var galFileName = Guid.NewGuid().ToString() + Path.GetExtension(galeriaKep.FileName);
                            var galFilePath = Path.Combine(folderPath, galFileName);
                            using (var stream = new FileStream(galFilePath, FileMode.Create)) { await galeriaKep.CopyToAsync(stream); }

                            var ujKep = new Kepek
                            {
                                Sid = defaultSzoba.Sid,
                                Fajlnev = "uploads/" + galFileName,
                                FeltoltveEkkor = DateTime.Now
                            };
                            _context.Kepek.Add(ujKep);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                if (SzolgaltatasIds != null && SzolgaltatasIds.Count > 0)
                {
                    foreach (var szolgId in SzolgaltatasIds)
                    {
                        var kapcso = new Szallas_szolgaltatas
                        {
                            Szid = szallas.Szid,
                            Szoid = szolgId
                        };
                        _context.Szallas_Szolgaltatas.Add(kapcso);
                    }
                    await _context.SaveChangesAsync();
                }

                var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                                ?? User.Claims.FirstOrDefault(c => c.Type.ToLower() == "email")?.Value;

                if (!string.IsNullOrEmpty(userEmail))
                {
                    await SendAdConfirmationEmailAsync(userEmail, szallas.Nev);
                }

                return CreatedAtAction(nameof(Get), new { id = szallas.Szid }, szallas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Hiba a mentés során", error = ex.Message });
            }
        }

        [Authorize(Policy = "Szallas.Update")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromForm] Szallas szallas, IFormFile? kepadat)
        {
            var oldSzallas = await _context.Szallasok.FirstOrDefaultAsync(s => s.Szid == id);
            if (oldSzallas == null) return NotFound();

            if (kepadat != null && kepadat.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(kepadat.FileName);
                var filePath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create)) { await kepadat.CopyToAsync(stream); }
                oldSzallas.Szallaskep = "uploads/" + fileName;
            }

            oldSzallas.Nev = szallas.Nev;
            oldSzallas.Iranyitoszam = szallas.Iranyitoszam;
            oldSzallas.Telepules = szallas.Telepules;
            oldSzallas.Utca = szallas.Utca;
            oldSzallas.Hazszam = szallas.Hazszam;
            oldSzallas.Ar = szallas.Ar;
            oldSzallas.Tid = szallas.Tid;
            oldSzallas.Leiras = szallas.Leiras;
            oldSzallas.Lat = szallas.Lat;
            oldSzallas.Lon = szallas.Lon;

            await _context.SaveChangesAsync();
            return Ok(oldSzallas);
        }

        // --- NEW: Secure Delete Check ---
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // 1. Get the logged-in user's Fid
                var userIdClaim = User.FindFirst("Fid")?.Value;
                if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
                int fid = int.Parse(userIdClaim);

                // 2. Find the accommodation
                var szallas = await _context.Szallasok.FirstOrDefaultAsync(s => s.Szid == id);
                if (szallas == null) return NotFound(new { message = "Szállás nem található." });

                // 3. Find the owner profile of the logged-in user
                var tulajdonos = await _context.Tulajdonosok.FirstOrDefaultAsync(t => t.Fid == fid);

                // 4. Security Check: Does this user actually own this specific accommodation?
                if (tulajdonos == null || szallas.Tid != tulajdonos.Tid)
                {
                    return Forbid(); // 403 Forbidden - "You don't own this!"
                }

                // 5. Delete it
                _context.Szallasok.Remove(szallas);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Szállás sikeresen törölve" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Hiba a törlés során", error = ex.Message });
            }
        }

        [HttpGet("my-accommodations")]
        [Authorize]
        public async Task<IActionResult> GetMyAccommodations()
        {
            var userIdClaim = User.FindFirst("Fid")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            int fid = int.Parse(userIdClaim);

            var tulajdonos = await _context.Tulajdonosok.FirstOrDefaultAsync(t => t.Fid == fid);
            if (tulajdonos == null) return Ok(new List<Szallas>());

            var mySzallasok = await _context.Szallasok
                .Where(s => s.Tid == tulajdonos.Tid)
                .ToListAsync();

            return Ok(mySzallasok);
        }

        // --- NEW: BOOKING ENDPOINT ---
        [Authorize]
        [HttpPost("{id}/book")]
        public async Task<IActionResult> BookAccommodation(int id, [FromBody] FoglalasRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst("Fid")?.Value;
                if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
                int fid = int.Parse(userIdClaim);

                var szallas = await _context.Szallasok.FindAsync(id);
                if (szallas == null) return NotFound(new { message = "Szállás nem található." });

                // Since we don't know your exact Model name, we use raw SQL to insert into the junction table
                var sql = "INSERT INTO szallas_felhasznalo (Fid, Szid, ErkezesNap) VALUES ({0}, {1}, {2})";
                await _context.Database.ExecuteSqlRawAsync(sql, fid, id, request.ErkezesNap);

                return Ok(new { message = "Sikeres foglalás!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Már van foglalása erre a napra, vagy hiba történt.", error = ex.Message });
            }
        }

        // --- 1. FETCH MY BOOKINGS ---
        [Authorize]
        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userIdClaim = User.FindFirst("Fid")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            int fid = int.Parse(userIdClaim);

            var bookings = new List<object>();
            var connection = _context.Database.GetDbConnection();

            await connection.OpenAsync();
            using (var command = connection.CreateCommand())
            {
                // We use raw SQL to securely join the junction table with the accommodation details
                command.CommandText = @"
                    SELECT sf.ErkezesNap, s.Szid, s.Nev, s.Telepules, s.Szallaskep, s.Ar 
                    FROM szallas_felhasznalo sf 
                    JOIN szallas s ON sf.Szid = s.Szid 
                    WHERE sf.Fid = @fid 
                    ORDER BY sf.ErkezesNap DESC";

                var param = command.CreateParameter();
                param.ParameterName = "@fid";
                param.Value = fid;
                command.Parameters.Add(param);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        bookings.Add(new
                        {
                            ErkezesNap = reader.GetDateTime(0),
                            Szid = reader.GetInt32(1),
                            Nev = reader.GetString(2),
                            Telepules = reader.GetString(3),
                            Szallaskep = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Ar = reader.GetInt32(5)
                        });
                    }
                }
            }
            await connection.CloseAsync();
            return Ok(bookings);
        }

        // --- 2. SUBMIT A REVIEW ---
        [Authorize]
        [HttpPost("{id}/review")]
        public async Task<IActionResult> PostReview(int id, [FromBody] ErtekelesRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst("Fid")?.Value;
                if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
                int fid = int.Parse(userIdClaim);

                // SECURITY CHECK 1: Did they actually book this place?
                // SECURITY CHECK 2: Has the date passed?
                var connection = _context.Database.GetDbConnection();
                bool canReview = false;

                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT COUNT(*) FROM szallas_felhasznalo 
                        WHERE Fid = @fid AND Szid = @szid AND ErkezesNap <= @today";

                    var pFid = command.CreateParameter(); pFid.ParameterName = "@fid"; pFid.Value = fid;
                    var pSzid = command.CreateParameter(); pSzid.ParameterName = "@szid"; pSzid.Value = id;
                    var pToday = command.CreateParameter(); pToday.ParameterName = "@today"; pToday.Value = DateTime.Now.Date;

                    command.Parameters.Add(pFid);
                    command.Parameters.Add(pSzid);
                    command.Parameters.Add(pToday);

                    var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    if (count > 0) canReview = true;
                }
                await connection.CloseAsync();

                if (!canReview)
                {
                    return BadRequest(new { message = "Csak olyan szállást értékelhet, ahol már megszállt (és a dátum elmúlt)!" });
                }

                // If secure, save the review!
                var ujErtekeles = new Ertekeles
                {
                    Szid = id,
                    Szoveg = request.Szoveg,
                    Pont = request.Pont,
                    Datum = DateTime.Now.Date
                };

                _context.Ertekelesek.Add(ujErtekeles);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Értékelés sikeresen elmentve!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Hiba az értékelés mentésekor.", error = ex.Message });
            }
        }

        // Helper class for the request
        public class ErtekelesRequest
        {
            public int Pont { get; set; }
            public string Szoveg { get; set; }
        }

        // Helper class for the request
        public class FoglalasRequest
        {
            public DateTime ErkezesNap { get; set; }
        }

        private async Task SendAdConfirmationEmailAsync(string userEmail, string accommodationName)
        {
            var payload = new
            {
                service_id = _config["EmailJS:ServiceId"],
                template_id = "template_w4x6jep",
                user_id = _config["EmailJS:PublicKey"],
                accessToken = _config["EmailJS:PrivateKey"],
                template_params = new
                {
                    user_email = userEmail,
                    hotel_name = accommodationName,
                    my_ads_link = "http://localhost:5173/profile?tab=hirdeteseim"
                }
            };

            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("https://api.emailjs.com/api/v1.0/email/send", content);
                if (response.IsSuccessStatusCode)
                    Console.WriteLine($"Sikeres hirdetés visszaigazolás elküldve: {userEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba az e-mail küldésekor: {ex.Message}");
            }
        }
    }
}