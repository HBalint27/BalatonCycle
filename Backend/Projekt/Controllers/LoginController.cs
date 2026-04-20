using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt.Auth;
using Projekt.Model;

namespace Projekt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly Context _context;
        private readonly TokenManager _tokenManager;

        public LoginController(Context context, TokenManager tokenManager)
        {
            _context = context;
            _tokenManager = tokenManager;
        }

        public class LoginRequest {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Felhasznalok.FirstOrDefaultAsync(u => u.Email == request.Email);
            
            if (user == null || !PasswordHandler.VerifyPassword(request.Password, user.Jelszo)) 
                return Unauthorized("Hibás email vagy jelszó.");

            var token = _tokenManager.GenerateToken(user);
            
            return Ok(new { 
                token = token, 
                user = new { user.Fid, user.Email, user.Nev } 
            });
        }

        [Authorize] // Csak bejelentkezett felhasználó érheti el!
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            // A Tokenből kiszedjük a júzer email címét (vagy ID-ját)
            var email = User.Identity?.Name; 
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var user = await _context.Felhasznalok
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null) return NotFound();

            // Jelszót biztonsági okokból soha ne küldjünk vissza a frontendnek!
            user.Jelszo = null; 

            return Ok(user);
        }
    }
}