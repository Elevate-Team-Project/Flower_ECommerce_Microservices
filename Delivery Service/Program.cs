using System;
using System.Reflection;
using System.Text;
using BuildingBlocks.Interfaces;
using BuildingBlocks.SharedEntities;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Delivery_Service.Entities;
using Delivery_Service.Features.Addresses;
using Delivery_Service.Features.Shipments;
using Delivery_Service.Infrastructure;
using Delivery_Service.Infrastructure.Data;
using Delivery_Service.Infrastructure.UnitOfWork;

namespace Delivery_Service
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Database
            builder.Services.AddDbContext<DeliveryDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Repositories & Unit of Work
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

            // MediatR
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

            // MassTransit
            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<PaymentSucceededConsumer>();
                x.AddConsumer<CodOrderApprovedConsumer>();

                x.AddEntityFrameworkOutbox<DeliveryDbContext>(o =>
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

            // JWT Authentication
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                };
            });

            builder.Services.AddAuthorization();

            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "Delivery Service API", Version = "v1" }));

            var app = builder.Build();

            // Database Migration on startup (Development only)
            if (app.Environment.IsDevelopment())
            {
                using var scope = app.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DeliveryDbContext>();
                await context.Database.MigrateAsync();
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            // Map Endpoints
            app.MapAddressEndpoints();
            app.MapShipmentEndpoints();

            app.MapGet("/", () => "Delivery Service is running...");

            await app.RunAsync();
        }
    }
}
