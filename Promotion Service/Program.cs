using BuildingBlocks.Interfaces;
using BuildingBlocks.SharedEntities;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Promotion_Service.Entities;
using Promotion_Service.Infrastructure;
using Promotion_Service.MiddleWares;
using Promotion_Service.Features.RegistrationCodes.ValidateRegistrationCode;
using Promotion_Service.Features.RegistrationCodes.ApplyRegistrationCode;
using Promotion_Service.Features.RegistrationCodes.CreateRegistrationCode;
using Promotion_Service.Features.Loyalty;
using Promotion_Service.Features.Loyalty.RedeemPoints;
using Promotion_Service.Features.Loyalty.GetTransactions;
using Promotion_Service.Features.Loyalty.GetTiers;
using Promotion_Service.Features.Loyalty.GetBalance;
using Promotion_Service.Features.Offers.CreateOffer;
using Promotion_Service.Features.Offers.UpdateOffer;
using Promotion_Service.Features.Offers.DeleteOffer;
using Promotion_Service.Features.Offers.GetAllOffers;
using Promotion_Service.Features.Offers.GetActiveOffers;
using Promotion_Service.Features.Offers.GetOfferById;
using Promotion_Service.Features.Coupons.CreateCoupon;
using Promotion_Service.Features.Coupons.ValidateCoupon;
using Promotion_Service.Features.Coupons.ApplyCoupon;
using Promotion_Service.Features.Coupons.GetAllCoupons;
using Promotion_Service.Features.Coupons.CouponHistory;
using Promotion_Service.Features.Banners.CreateBanner;
using Promotion_Service.Features.Banners.UpdateBanner;
using Promotion_Service.Features.Banners.DeleteBanner;
using Promotion_Service.Features.Banners.GetAllBanners;
using Promotion_Service.Features.Banners.GetActiveBanners;

namespace Promotion_Service
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Database Configuration
            builder.Services.AddDbContext<PromotionDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Repositories & Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register repositories for all entities
            builder.Services.AddScoped<IBaseRepository<Offer>, BaseRepository<Offer>>();
            builder.Services.AddScoped<IBaseRepository<Coupon>, BaseRepository<Coupon>>();
            builder.Services.AddScoped<IBaseRepository<CouponUsage>, BaseRepository<CouponUsage>>();
            builder.Services.AddScoped<IBaseRepository<LoyaltyAccount>, BaseRepository<LoyaltyAccount>>();
            builder.Services.AddScoped<IBaseRepository<LoyaltyTier>, BaseRepository<LoyaltyTier>>();
            builder.Services.AddScoped<IBaseRepository<LoyaltyTransaction>, BaseRepository<LoyaltyTransaction>>();
            builder.Services.AddScoped<IBaseRepository<RegistrationCode>, BaseRepository<RegistrationCode>>();
            builder.Services.AddScoped<IBaseRepository<RegistrationCodeUsage>, BaseRepository<RegistrationCodeUsage>>();
            builder.Services.AddScoped<IBaseRepository<Banner>, BaseRepository<Banner>>();

            // Configuration
            builder.Services.Configure<LoyaltySettings>(builder.Configuration.GetSection("LoyaltySettings"));

            // MediatR
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

            // FluentValidation
            builder.Services.AddValidatorsFromAssemblyContaining<Program>();

            // MassTransit (RabbitMQ + Outbox)
            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<Promotion_Service.Features.Loyalty.EarnPoints.OrderDeliveredConsumer>();

                x.AddEntityFrameworkOutbox<PromotionDbContext>(o =>
                {
                    o.UseSqlServer();
                    o.UseBusOutbox();
                });

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
                    {
                        h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
                        h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
                    });
                    cfg.ConfigureEndpoints(context);
                });
            });

            // Authentication
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
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                };
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
            });

            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Promotion Service API", Version = "v1" });
            });

            var app = builder.Build();

            // Database Migration
            if (app.Environment.IsDevelopment())
            {
                using var scope = app.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<PromotionDbContext>();
                await context.Database.MigrateAsync();
            }

            app.UseErrorHandling();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();
            app.UseAuthorization();

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
            app.MapLoyaltyTransactionsEndpoints();
            app.MapRedeemPointsEndpoints();
            app.MapLoyaltyTiersEndpoints();

            // Map Endpoints - Banners
            app.MapCreateBannerEndpoints();
            app.MapGetAllBannersEndpoints();
            app.MapGetActiveBannersEndpoints();
            app.MapUpdateBannerEndpoints();
            app.MapDeleteBannerEndpoints();

            // Map Endpoints - Registration Codes
            app.MapCreateRegistrationCodeEndpoints();
            app.MapValidateRegistrationCodeEndpoints();
            app.MapApplyRegistrationCodeEndpoints();

            app.MapGet("/", () => "Promotion Service is running...");
            
            // Map gRPC Service
            app.MapGrpcService<Promotion_Service.Features.PriceResolution.PromotionGrpcService>();

            await app.RunAsync();
        }
    }
}
