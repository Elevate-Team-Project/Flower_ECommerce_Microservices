using BuildingBlocks.Interfaces;
using BuildingBlocks.MiddleWares; // Ensure you have this namespace/folder
using BuildingBlocks.SharedEntities;
using Catalog_Service.Entities;
using Catalog_Service.Features.OccasionsFeature.CreateOccasion;
using Catalog_Service.Infrastructure;
using Catalog_Service.Infrastructure.Data;
using Catalog_Service.Infrastructure.UnitOfWork;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Reflection;
using System.Text;

namespace Catalog_Service
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // -------------------------------------------------------------------------------------
            // 1. Serilog Configuration
            // -------------------------------------------------------------------------------------
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "CatalogService")
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} | {CorrelationId} | {Message:lj}{NewLine}{Exception}", theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Literate)
                .WriteTo.File("logs/CatalogService-.log", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext} | {CorrelationId} | {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            try
            {
                Log.Information("Starting CatalogService Application");

                var builder = WebApplication.CreateBuilder(args);

                builder.Host.UseSerilog();

                var config = builder.Configuration;

                // -------------------------------------------------------------------------------------
                // 2. Service Registration (Dependency Injection)
                // -------------------------------------------------------------------------------------

                builder.Services.AddMemoryCache();
                builder.Services.AddHttpContextAccessor();

                // Ensure you have these classes created in your project or referencing BuildingBlocks
                // builder.Services.AddScoped<ICurrentUserService, CurrentUserService>(); 
                // builder.Services.AddScoped<TransactionMiddleware>(); 
                 builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

                // Configure Entity Framework Core with SQL Server
                builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
                {
                    options.UseSqlServer(config.GetConnectionString("DefaultConnection"));

                    // Recommended for Read-Heavy services like Catalog
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

                    if (builder.Environment.IsDevelopment())
                    {
                        options.EnableSensitiveDataLogging(true);
                        options.EnableDetailedErrors(true);
                    }
                });

                // Generic Repository Registration
                var entityTypes = Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseEntity)))
                    .ToList();

                foreach (var entityType in entityTypes)
                {
                    var interfaceType = typeof(IBaseRepository<>).MakeGenericType(entityType);
                    var implementationType = typeof(BaseRepository<>).MakeGenericType(entityType);
                    builder.Services.AddScoped(interfaceType, implementationType);
                }

                Log.Information("Registered {Count} generic repositories successfully", entityTypes.Count);

                builder.Services.AddMediatR(typeof(Program).Assembly);;

                // -------------------------------------------------------------------------------------
                // MassTransit Configuration (RabbitMQ + Outbox Pattern)
                // -------------------------------------------------------------------------------------
                builder.Services.AddMassTransit(x =>
                {
                    // Add Consumers here when you create them
                    // x.AddConsumer<ProductCreatedConsumer>();

                    // CRITICAL: Configure Transactional Outbox
                    x.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
                    {
                        o.UseSqlServer();
                        o.UseBusOutbox();
                    });

                    x.SetKebabCaseEndpointNameFormatter();

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        var rabbitMqHost = config["RabbitMq:Host"] ?? "localhost";

                        cfg.Host(rabbitMqHost, "/", h =>
                        {
                            h.Username("guest");
                            h.Password("guest");
                        });

                        cfg.ConfigureEndpoints(context);
                    });
                });

                // -------------------------------------------------------------------------------------
                // API Security & Configuration
                // -------------------------------------------------------------------------------------

                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAll",
                        b => b.AllowAnyMethod()
                        .AllowAnyHeader()
                        .SetIsOriginAllowed(origin => true)
                        .AllowCredentials());
                });

                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = config["Jwt:Issuer"],
                        ValidAudience = config["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
                    };
                });

                builder.Services.AddAuthorization();

                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo { Title = "CatalogService API", Version = "v1" });

                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Please enter JWT with Bearer into field (e.g., 'Bearer {token}')",
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                    });
                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                            },
                            new string[] {}
                        }
                    });
                });

                var app = builder.Build();

                // -------------------------------------------------------------------------------------
                // 3. Database Migration & Seeding (Startup Scope)
                // -------------------------------------------------------------------------------------
                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    try
                    {
                        var context = services.GetRequiredService<ApplicationDbContext>();
                        await context.Database.MigrateAsync();
                        await DatabaseSeeder.SeedAsync(services);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error occurred during database migration or seeding");
                        if (app.Environment.IsDevelopment()) throw;
                    }
                }

                // -------------------------------------------------------------------------------------
                // 4. HTTP Request Pipeline (Middleware Order)
                // -------------------------------------------------------------------------------------

                // Uncomment once you have the Middleware class
                // app.UseMiddleware<ErrorHandlingMiddleware>();

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();

                app.UseCors("AllowAll");

                app.UseAuthentication();
                app.UseAuthorization();

                // Uncomment once you have the Middleware class
                // app.UseMiddleware<TransactionMiddleware>();

                // Ensure you have an Extension method for MapAllEndpoints or use Controllers
                // app.MapAllEndpoints(); 

                // Temporary endpoints until you add Features/Endpoints
                app.MapGet("/", () => "Catalog Service is running.");
                app.MapGet("/Brands", async (IBaseRepository<Brand> BrandRepo) => Results.Ok(await BrandRepo.GetAll().ToListAsync()));
                app.MapUpdateCategoryEndpoints();

                app.MapCategoryStatusEndpoints();



                app.MapCreateOccasionEndpoints();

                await app.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failed to start due to an unhandled exception");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}