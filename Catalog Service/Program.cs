using BuildingBlocks.Interfaces;
using BuildingBlocks.MiddleWares; // Ensure you have this namespace/folder
using BuildingBlocks.SharedEntities;
using Catalog_Service.Entities;
using Catalog_Service.Features.CategoriesFeature.GetAllCategories;
using Catalog_Service.Features.CategoriesFeature.UpdateCategory;
using Catalog_Service.Features.CategoriesFeature.UpdateCategoryStatus;
using Catalog_Service.Features.OccasionsFeature.CreateOccasion;
using Catalog_Service.Features.OccasionsFeature.GetAllOccasions;
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
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using Catalog_Service.Features.CategoriesFeature.CreateCategory;
using Catalog_Service.Infrastructure.UnitOfWork;
using Catalog_Service.Features.OccasionsFeature.UpdateOccasion;
using Catalog_Service.Features.OffersFeature.CreateOffer;
using Catalog_Service.Features.OffersFeature.UpdateOffer;
using Catalog_Service.Features.OffersFeature.DeleteOffer;
using Catalog_Service.Features.OffersFeature.GetAllOffers;
using Catalog_Service.Features.OffersFeature.GetActiveOffers;
using Catalog_Service.Features.OffersFeature.GetOfferById;
using Catalog_Service.Features.CouponsFeature.CreateCoupon;
using Catalog_Service.Features.CouponsFeature.GetAllCoupons;
using Catalog_Service.Features.CouponsFeature.ValidateCoupon;
using Catalog_Service.Features.CouponsFeature.ApplyCoupon;
using Catalog_Service.Features.CouponsFeature.CouponHistory;
using Catalog_Service.Features.LoyaltyFeature.GetBalance;
using Catalog_Service.Features.LoyaltyFeature.GetTiers;
using Catalog_Service.Features.LoyaltyFeature.GetTransactions;
using Catalog_Service.Features.LoyaltyFeature.RedeemPoints;
using Catalog_Service.Features.RegistrationCodesFeature.CreateRegistrationCode;
using Catalog_Service.Features.RegistrationCodesFeature.ValidateRegistrationCode;
using Catalog_Service.Features.RegistrationCodesFeature.ApplyRegistrationCode;
using Catalog_Service.Features.BannersFeature.CreateBanner;
using Catalog_Service.Features.BannersFeature.DeleteBanner;
using Catalog_Service.Features.BannersFeature.GetActiveBanners;
using Catalog_Service.Features.BannersFeature.GetAllBanners;
using Catalog_Service.Features.BannersFeature.UpdateBanner;
using Catalog_Service.Features.CategoriesFeature.GetActiveCategoryFeature;
using Catalog_Service.GrpcServices;
// PromotionGrpcService should be in Catalog_Service.GrpcServices namespace
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
                // In your CatalogService's Program.cs or Startup.cs
                // Register all repositories from BuildingBlocks
                





                // Ensure you have these classes created in your project or referencing BuildingBlocks
                // builder.Services.AddScoped<ICurrentUserService, CurrentUserService>(); 
                // builder.Services.AddScoped<TransactionMiddleware>(); 
                 builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

                builder.Services.AddStackExchangeRedisCache(options =>
                {
                    var redisUrl = builder.Configuration["Redis:Url"];
                    var redisPassword = builder.Configuration["Redis:Password"];

                    if (string.IsNullOrEmpty(redisUrl))
                        throw new ArgumentException("Redis URL is missing!");

                    options.Configuration = $"{redisUrl},password={redisPassword}";
                    options.InstanceName = "catalog_"; // prefix ?????
                });

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
                builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

                builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

                Log.Information("Registered {Count} generic repositories successfully", entityTypes.Count);

                builder.Services.AddMediatR(typeof(Program).Assembly); 

                // -------------------------------------------------------------------------------------
                // MassTransit Configuration (RabbitMQ + Outbox Pattern)
                // -------------------------------------------------------------------------------------
                builder.Services.AddMassTransit(x =>
                {
                    // Add Consumers here
                    x.AddConsumer<Catalog_Service.Features.ProductsFeature.StockManagement.OrderDeliveredConsumer>();
                    x.AddConsumer<Catalog_Service.Features.ProductsFeature.StockManagement.PaymentFailedConsumer>();
                    x.AddConsumer<Catalog_Service.Features.ProductsFeature.StockManagement.OrderCancelledConsumer>();
                    x.AddConsumer<Catalog_Service.Features.ProductsFeature.OfferExpiredConsumer>();
                    // Loyalty Consumer
                    x.AddConsumer<Catalog_Service.Features.LoyaltyFeature.EarnPoints.OrderDeliveredConsumer>();

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

                // Add gRPC service
                builder.Services.AddGrpc();

                // Add gRPC client for Ordering Service (US-A11: Check product in active orders)
                builder.Services.AddGrpcClient<BuildingBlocks.Grpc.OrderingGrpc.OrderingGrpcClient>(options =>
                {
                    var orderingServiceUrl = config["GrpcServices:OrderingService"] ?? "https://localhost:5199";
                    options.Address = new Uri(orderingServiceUrl);
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    // Allow self-signed certificates in development
                    var handler = new HttpClientHandler();
                    handler.ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                    return handler;
                });

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

                ////app.UseHttpsRedirection();

                app.UseCors("AllowAll");

                app.UseAuthentication();
                app.UseAuthorization();
                app.MapAllEndpoints();
                app.MapGet("/", () => "Catalog Service is running...");
                // Map Endpoints - Offers
                app.MapCreateOfferEndpoints();
                app.MapGetAllOffersEndpoints();
                app.MapGetOfferByIdEndpoints();
                app.MapUpdateOfferEndpoints();
                app.MapDeleteOfferEndpoints();
                app.MapGetActiveOffersEndpoints();

                // Map Endpoints - Coupons
                app.MapCreateCouponEndpoints();
                app.MapGetAllCouponsEndpoints();
                app.MapValidateCouponEndpoints();
                app.MapApplyCouponEndpoints();
                app.MapCouponHistoryEndpoints();

                // Map Endpoints - Loyalty
                app.MapLoyaltyBalanceEndpoints();
                app.MapLoyaltyTiersEndpoints();
                app.MapLoyaltyTransactionsEndpoints();
                app.MapRedeemPointsEndpoints();

                // Map Endpoints - Registration Codes
                app.MapCreateRegistrationCodeEndpoints();
                app.MapValidateRegistrationCodeEndpoints();
                app.MapApplyRegistrationCodeEndpoints();

                // Map Endpoints - Banners
                app.MapCreateBannerEndpoints();
                app.MapDeleteBannerEndpoints();
                app.MapGetActiveBannersEndpoints();
                app.MapGetAllBannersEndpoints();
                app.MapUpdateBannerEndpoints();

                // Map gRPC service
                app.MapGrpcService<CatalogGrpcService>();
                app.MapGrpcService<PromotionGrpcService>(); // ✅ NEW

                // Uncomment once you have the Middleware class
                // app.UseMiddleware<TransactionMiddleware>();

              

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