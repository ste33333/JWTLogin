
using System.Text;
using JWTLogin.Data;
using JWTLogin.Models;
using JWTLogin.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace JWTLogin
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Configurazione JwtSettings
            var jwtSettingsSection = builder.Configuration.GetSection("Jwt");
            builder.Services.Configure<JwtSettings>(jwtSettingsSection);
            var jwtSettings = jwtSettingsSection.Get<JwtSettings>();

            // 2. Configurazione Entity Framework Core DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection")));

            // 3. Registrazione servizi custom
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddControllers();

            // 4. Configura l'autenticazione JWT
            if (jwtSettings != null && !string.IsNullOrEmpty(jwtSettings.Key))
            {
                var keyBytes = Encoding.ASCII.GetBytes(jwtSettings.Key);
                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = builder.Environment.IsProduction(); 
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,
                        ValidateLifetime = true, // Controlla la scadenza del token
                        ClockSkew = TimeSpan.Zero // Nessuna tolleranza sulla scadenza per impostazione predefinita
                    };
                });
            }
            else
            {
                // Logga un errore se le impostazioni JWT non sono configurate
                throw new InvalidOperationException("La configurazione JWT (Key, Issuer, Audience) è mancante o incompleta in appsettings.json");
            }


            // 5. Configura CORS (Cross-Origin Resource Sharing)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularApp", 
                    policyBuilder =>
                    {
                        policyBuilder.WithOrigins("http://localhost:4200") 
                                     .AllowAnyHeader()
                                     .AllowAnyMethod();
                    });
            });


            // 6. Configura Swagger/OpenAPI 
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "TuoProgetto API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Inserisci 'Bearer ' seguito dal tuo token JWT",
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
            {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
            }
            });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TuoProgetto API V1"));
                app.UseDeveloperExceptionPage(); 
            }
            else
            {
                app.UseExceptionHandler("/Error"); 
                app.UseHsts(); 
            }

            app.UseHttpsRedirection(); 

            app.UseRouting();

            app.UseCors("AllowAngularApp"); 

            app.UseAuthentication(); 
            app.UseAuthorization();  

            app.MapControllers(); 

            app.Run();
        }
    }
}
