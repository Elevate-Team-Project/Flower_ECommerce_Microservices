using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace API_Gateway
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add Ocelot configuration file
            builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

            // ============================================
            // JWT Authentication & Authorization Setup
            // ============================================
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer("Bearer", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "FlowerEcommerceAuthSystem",
                    ValidAudience = builder.Configuration["Jwt:Audience"] ?? "FlowerEcommerceAuthClient",
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "My$up3rS3cr3tKey_2025@ExamSystem!"))
                };
            });

            builder.Services.AddAuthorization();

            // ============================================
            // Ocelot Gateway Configuration
            // ============================================
            builder.Services.AddOcelot(builder.Configuration);

            // ============================================
            // CORS Configuration
            // ============================================
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("GatewayPolicy", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // ============================================
            // Health Checks
            // ============================================
            builder.Services.AddHealthChecks();

            // ============================================
            // Swagger/OpenAPI
            // ============================================
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Flower E-Commerce API Gateway (Ocelot)",
                    Version = "v1",
                    Description = "Unified Ocelot entry point for all Flower E-Commerce microservices"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Enter JWT Bearer token into field (e.g. 'Bearer {token}')",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

            var app = builder.Build();

            // ============================================
            // Middleware Pipeline
            // ============================================
            
            // CORS must be first
            app.UseCors("GatewayPolicy");

            // Health check endpoint
            app.MapHealthChecks("/health");

            // Gateway info endpoint
            app.MapGet("/", () => Results.Ok(new
            {
                Service = "Flower E-Commerce API Gateway (Ocelot)",
                Version = "1.0.0",
                Status = "Running",
                Timestamp = DateTime.UtcNow
            }));

            // Swagger (all environments in gateway for API documentation)
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway API v1");
                c.RoutePrefix = "swagger";
            });

            app.UseAuthentication();
            app.UseAuthorization();

            Console.WriteLine("🌸 [Gateway] Flower E-Commerce Ocelot API Gateway is starting...");
            Console.WriteLine("📡 [Gateway] Routing requests to downstream microservices via Ocelot");

            // Ocelot Reverse Proxy Middleware
            await app.UseOcelot();

            await app.RunAsync();
        }
    }
}
