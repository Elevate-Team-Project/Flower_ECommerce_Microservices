using BuildingBlocks.Interfaces;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering_Service.Entities;
using Ordering_Service.Features.Orders.CreateOrder;
using Ordering_Service.Features.Orders.GetOrderDetails;
using Ordering_Service.Features.Orders.GetUserOrders;
using Ordering_Service.Features.Orders.UpdateOrderStatus;
using Ordering_Service.Features.Shipments.CreateShipment;
using Ordering_Service.Features.Shipments.GetShipmentDetails;
using Ordering_Service.Features.Shipments.UpdateShipmentStatus;
using Ordering_Service.Infrastructure;
using Ordering_Service.Infrastructure.Data;
using Ordering_Service.Infrastructure.UnitOfWork;
using Ordering_Service.MiddleWares;

namespace Ordering_Service
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =========================================================
            // 📦 Database Configuration
            // =========================================================
            builder.Services.AddDbContext<OrderingDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // =========================================================
            // 🔧 Dependency Injection - Repositories & UoW
            // =========================================================
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IBaseRepository<Order>, BaseRepository<Order>>();
            builder.Services.AddScoped<IBaseRepository<OrderItem>, BaseRepository<OrderItem>>();
            builder.Services.AddScoped<IBaseRepository<Shipment>, BaseRepository<Shipment>>();
            builder.Services.AddScoped<IBaseRepository<DiscountUsage>, BaseRepository<DiscountUsage>>();

            // =========================================================
            // 🎯 MediatR Configuration
            // =========================================================
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

            // =========================================================
            // ✅ FluentValidation Configuration
            // =========================================================
            builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderValidator>();

            // =========================================================
            // 🚌 MassTransit Configuration (Message Bus)
            // =========================================================
            builder.Services.AddMassTransit(x =>
            {
                x.AddEntityFrameworkOutbox<OrderingDbContext>(o =>
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

            // =========================================================
            // 🔐 Authorization Configuration
            // =========================================================
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminPolicy", policy =>
                    policy.RequireRole("Admin"));
            });

            // =========================================================
            // 📚 Swagger/OpenAPI Configuration
            // =========================================================
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Ordering Service API", Version = "v1" });
            });

            var app = builder.Build();

            // =========================================================
            // 🌱 Database Seeding (Development Only)
            // =========================================================
            if (app.Environment.IsDevelopment())
            {
                using var scope = app.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
                await DatabaseSeeder.SeedAsync(context);
            }

            // =========================================================
            // 🔧 Middleware Pipeline
            // =========================================================
            app.UseErrorHandling();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            // =========================================================
            // 🛣️ Map Endpoints
            // =========================================================
            // Orders
            app.MapCreateOrderEndpoints();
            app.MapGetUserOrdersEndpoints();
            app.MapGetOrderDetailsEndpoints();
            app.MapUpdateOrderStatusEndpoints();

            // Shipments
            app.MapCreateShipmentEndpoints();
            app.MapUpdateShipmentStatusEndpoints();
            app.MapGetShipmentDetailsEndpoints();

            await app.RunAsync();
        }
    }
}
