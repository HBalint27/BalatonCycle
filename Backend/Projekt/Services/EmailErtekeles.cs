using Microsoft.EntityFrameworkCore;
using Projekt.Model;
using System.Text;
using System.Text.Json;

namespace Projekt.Services
{
    public class EmailErtekeles : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public EmailErtekeles(IServiceProvider serviceProvider, IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _serviceProvider = serviceProvider;
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Meghatározzuk a következő futás időpontját (pl. minden reggel 9:00)
                var now = DateTime.Now;
                var nextRunTime = new DateTime(now.Year, now.Month, now.Day, 9, 0, 0);
                if (now > nextRunTime) nextRunTime = nextRunTime.AddDays(1);

                var delay = nextRunTime - now;
                await Task.Delay(delay, stoppingToken);

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<Context>();
                        
                        // A tegnapi nap dátuma
                        var tegnap = DateTime.Now.AddDays(-1).Date;

                        // Lekérjük a tegnapi foglalásokat (amik ma jártak le)
                        var tegnapiFoglalasok = await context.Foglalasok
                            .Include(f => f.Szallas)
                            .Include(f => f.Felhasznalo)
                            .Where(f => f.ErkezesNap.Date == tegnap)
                            .ToListAsync();

                        foreach (var foglalas in tegnapiFoglalasok)
                        {
                            if (foglalas.Felhasznalo != null && !string.IsNullOrEmpty(foglalas.Felhasznalo.Email))
                            {
                                await SendReviewRequestAsync(
                                    foglalas.Felhasznalo.Email,
                                    foglalas.Felhasznalo.Nev ?? "Vendégünk",
                                    foglalas.Szallas.Nev,
                                    foglalas.ErkezesNap.ToString("yyyy.MM.dd"),
                                    foglalas.Szid
                                );
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Hiba az automatikus értékeléskérő futtatásakor: {ex.Message}");
                }
            }
        }

        private async Task SendReviewRequestAsync(string email, string name, string hotel, string date, int szid)
        {
            var payload = new
            {
                service_id = _config["EmailJS:ServiceId"],
                template_id = "template_95qxy5s",
                user_id = _config["EmailJS:PublicKey"],
                accessToken = _config["EmailJS:PrivateKey"],
                template_params = new
                {
                    user_email = email,
                    user_name = name,
                    hotel_name = hotel,
                    booking_date = date,
                    // ÉLES DOMAIN HASZNÁLATA (Rackhoston foglaltad le)
                    review_link = $"https://localhost:5173/szallas/{szid}#review-section"
                }
            };

            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.emailjs.com/api/v1.0/email/send", content);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Értékeléskérő sikeresen kiküldve: {email}");
            }
        }
    }
}