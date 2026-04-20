using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Projekt.Auth;
using Projekt.Model;
using Projekt.Services;
using System.Text;
using System.Text.Json.Serialization;

namespace Projekt
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("projekt");
            if (connectionString == null)
            {
                Console.WriteLine("Adatbázis kapcsolat string nem található");
                return;
            }

            builder.Services.AddDbContext<Context>(options =>
            {
                options.UseMySQL(connectionString);
            });

            builder.Services.AddHttpClient();
            // UPDATED CORS POLICY
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("ReactPolicy", policy =>
                {
                    policy.AllowAnyOrigin()   // Allows access from any port (5173, 3000, etc.)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            builder.Services.AddControllers()
                .AddJsonOptions(o =>
                {
                    // This is important because your model has circular references (Szallas -> Szoba -> Szallas)
                    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    // This ensures SzallasCime becomes szallasCime in JSON
                    o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                });

            builder.Services.AddSingleton<TokenManager>();
            builder.Services.AddHostedService<EmailErtekeles>();

            AddJwtAuthentication(builder);

            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Írd be így: Bearer {token}"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            builder.Services.AddEndpointsApiExplorer();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // ORDER MATTERS HERE
            app.UseHttpsRedirection();

            app.UseStaticFiles();

            // Apply CORS before Authentication and Authorization
            app.UseCors("ReactPolicy");

            app.UseAuthentication();
            
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static void AddJwtAuthentication(WebApplicationBuilder builder)
        {
            var secretKey = builder.Configuration["Auth:JWT:Key"];
            var issuer = builder.Configuration["Auth:JWT:Issuer"];
            var audience = builder.Configuration["Auth:JWT:Audience"];

            if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
            {
                throw new ApplicationException("Authentication konfiguráció hiányzik");
            }

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                var sp = builder.Services.BuildServiceProvider();
                var tokenManager = sp.GetRequiredService<TokenManager>();

                foreach (var permission in tokenManager.Permissions.Distinct())
                {
                    options.AddPolicy(permission, policy =>
                        policy.RequireClaim("permission", permission));
                }
            });
        }
    }
}