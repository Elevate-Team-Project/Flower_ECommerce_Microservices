using BuildingBlocks.Interfaces;
using BuildingBlocks.SharedEntities; // Ensure this contains BaseEntity if needed
using Cart_Service.Features.Cart.AddToCart;
using Cart_Service.Features.Cart.Checkout;
using Cart_Service.Features.Cart.RemoveCartItem;
using Cart_Service.Features.Cart.UpdateProductQuantityInShoppingCart;
using Cart_Service.Features.Cart.ViewShoppingCart;
using Cart_Service.Infrastructure;
using Cart_Service.Infrastructure.Data; // Update to match your actual namespace
using Cart_Service.Infrastructure.UnitOfWork;
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

namespace Cart_Service
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
                .Enrich.WithProperty("Application", "CartService")
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} | {CorrelationId} | {Message:lj}{NewLine}{Exception}", theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Literate)
                .WriteTo.File("logs/CartService-.log", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext} | {CorrelationId} | {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            try
            {
                Log.Information("Starting CartService Application");

                var builder = WebApplication.CreateBuilder(args);

                builder.Host.UseSerilog();

                var config = builder.Configuration;

                // -------------------------------------------------------------------------------------
                // 2. Service Registration (Dependency Injection)
                // -------------------------------------------------------------------------------------

                builder.Services.AddMemoryCache();
                builder.Services.AddHttpContextAccessor();

                // Common Services
                // builder.Services.AddScoped<ICurrentUserService, CurrentUserService>(); 
                builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

                // -------------------------------------------------------------------------------------
                // Redis Configuration (Robust Logic)
                // -------------------------------------------------------------------------------------
                var redisUrl = config["Redis:Url"];
                var redisPassword = config["Redis:Password"];

                if (string.IsNullOrEmpty(redisUrl))
                {
                    Log.Warning("Redis URL is missing from configuration. Caching may not work.");
                }
                else
                {
                    builder.Services.AddStackExchangeRedisCache(options =>
                    {
                        options.InstanceName = "flower_cart_";

                        var configOptions = StackExchange.Redis.ConfigurationOptions.Parse(redisUrl);
                        if (!string.IsNullOrEmpty(redisPassword))
                        {
                            configOptions.Password = redisPassword;
                        }
                        configOptions.AbortOnConnectFail = false;
                        options.ConfigurationOptions = configOptions;
                    });
                    Log.Information($"Registered Redis Cache at {redisUrl}");
                }

                // -------------------------------------------------------------------------------------
                // Entity Framework Core (SQL Server)
                // -------------------------------------------------------------------------------------
                builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
                {
                    options.UseSqlServer(config.GetConnectionString("DefaultConnection"));

                    // NoTracking is good for read-heavy, but be careful if you rely on tracking for updates
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

                    if (builder.Environment.IsDevelopment())
                    {
                        options.EnableSensitiveDataLogging(true);
                        options.EnableDetailedErrors(true);
                    }
                });

                // Generic Repository Registration
                // Only registers entities that inherit from BaseEntity
                var entityTypes = Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseEntity)))
                    .ToList();

                foreach (var entityType in entityTypes)
                {
                    var interfaceType = typeof(IBaseRepository<>).MakeGenericType(entityType);
                    var implementationType = typeof(BaseRepository<>).MakeGenericType(entityType); // Ensure you have BaseRepository in this project
                    builder.Services.AddScoped(interfaceType, implementationType);
                }

                Log.Information("Registered {Count} generic repositories successfully", entityTypes.Count);

                builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

                // -------------------------------------------------------------------------------------
                // gRPC Client Configuration (Catalog Service)
                // -------------------------------------------------------------------------------------
                var catalogServiceUrl = config["GrpcServices:Catalog"] ?? "http://localhost:5001";
                builder.Services.AddGrpcClient<BuildingBlocks.Grpc.CatalogGrpc.CatalogGrpcClient>(options =>
                {
                    options.Address = new Uri(catalogServiceUrl);
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var handler = new HttpClientHandler();
                    handler.ServerCertificateCustomValidationCallback = 
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                    return handler;
                });

                // -------------------------------------------------------------------------------------
                // MassTransit Configuration (RabbitMQ + Outbox Pattern)
                // -------------------------------------------------------------------------------------
                builder.Services.AddMassTransit(x =>
                {
                    // Add Consumers here
                    // x.AddConsumer<BasketCheckoutConsumer>(); 

                    // Transactional Outbox
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
                    options.SwaggerDoc("v1", new OpenApiInfo { Title = "CartService API", Version = "v1" });

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
                        Log.Error(ex, "Error occurred during database migration");
                        if (app.Environment.IsDevelopment()) throw;
                    }
                }

                // -------------------------------------------------------------------------------------
                // 4. HTTP Request Pipeline (Middleware Order)
                // -------------------------------------------------------------------------------------

                // app.UseMiddleware<ErrorHandlingMiddleware>();

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                ////app.UseHttpsRedirection();

                app.UseCors("AllowAll");

                app.UseAuthentication();
                app.UseAuthorization();

                // Ensure you have the EndpointExtensions class in your Cart project as well!
                app.MapAllEndpoints(); 
                app.MapGet("/health", () => Results.Ok("Cart Service is healthy"));
                // get all carts 
                app.MapGet("/carts", async (IBaseRepository<Entities.Cart> cartRepository) =>
                {
                    var carts = await cartRepository.GetAll().ToListAsync();
                    return Results.Ok(carts);
                });
                app.MapViewCartEndpoints();
                app.MapAddToCartEndpoints();
                app.MapRemoveCartItemEndpoints();
                app.MapCheckoutEndpoints();
                app.MapUpdateItemQuantityEndpoints();
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