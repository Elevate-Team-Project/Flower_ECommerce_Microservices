using BuildingBlocks.Grpc;
using BuildingBlocks.Interfaces;
using FluentValidation;
using Grpc.Net.Client;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering_Service.Entities;
using Ordering_Service.Features.Orders.CreateOrder;
using Ordering_Service.Features.Orders.GetOrderDetails;
using Ordering_Service.Features.Orders.GetUserOrders;
using Ordering_Service.Features.Orders.UpdateOrderStatus;
using Ordering_Service.GrpcServices;
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

            // Database
            builder.Services.AddDbContext<OrderingDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Repositories & UoW
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IBaseRepository<Order>, BaseRepository<Order>>();
            builder.Services.AddScoped<IBaseRepository<OrderItem>, BaseRepository<OrderItem>>();
            builder.Services.AddScoped<IBaseRepository<DiscountUsage>, BaseRepository<DiscountUsage>>();

            // MediatR
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

            // FluentValidation
            builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderValidator>();

            // MassTransit
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

            // gRPC Server for Ordering Service
            builder.Services.AddGrpc();

            // gRPC Client for Catalog Service
            var catalogServiceUrl = builder.Configuration["GrpcServices:CatalogServiceUrl"] ?? "https://localhost:5001";
            builder.Services.AddGrpcClient<CatalogGrpc.CatalogGrpcClient>(options =>
            {
                options.Address = new Uri(catalogServiceUrl);
            })
            .ConfigureChannel(options =>
            {
                // For development - accept any certificate
                if (builder.Environment.IsDevelopment())
                {
                    options.HttpHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                }
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "Ordering Service API", Version = "v1" }));

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                using var scope = app.Services.CreateScope();
                //await DatabaseSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<OrderingDbContext>());
            }

            app.UseErrorHandling();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();
            app.UseAuthorization();

            // Map gRPC Service
            app.MapGrpcService<OrderingGrpcService>();

            // Map Order Endpoints
            app.MapCreateOrderEndpoints();
            app.MapGetUserOrdersEndpoints();
            app.MapGetOrderDetailsEndpoints();
            app.MapUpdateOrderStatusEndpoints();

            await app.RunAsync();
        }
    }
}
