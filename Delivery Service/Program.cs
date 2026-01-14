using BuildingBlocks.Grpc;
using BuildingBlocks.Interfaces;
using Delivery_Service.Entities;
using Delivery_Service.Features.Addresses.CreateAddress;
using Delivery_Service.Features.Addresses.GetUserAddresses;
using Delivery_Service.Features.Addresses.UpdateAddress;
using Delivery_Service.Features.Addresses.DeleteAddress;
using Delivery_Service.Features.Addresses.SetDefaultAddress;
using Delivery_Service.Features.Shipments.CreateShipment;
using Delivery_Service.Features.Shipments.UpdateShipmentStatus;
using Delivery_Service.Features.Shipments.GetShipmentDetails;
using Delivery_Service.Features.Shipments.UpdateDriverLocation;
using Delivery_Service.Features.Shipments.GetDeliveryTracking;
using Delivery_Service.Infrastructure;
using Delivery_Service.Infrastructure.Data;
using Delivery_Service.Infrastructure.UnitOfWork;
using Delivery_Service.MiddleWares;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using MassTransit;

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

            // Repositories & UoW
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IBaseRepository<UserAddress>, BaseRepository<UserAddress>>();
            builder.Services.AddScoped<IBaseRepository<Shipment>, BaseRepository<Shipment>>();
            builder.Services.AddScoped<IBaseRepository<DeliveryZone>, BaseRepository<DeliveryZone>>();

            // MediatR
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

            // FluentValidation
            builder.Services.AddValidatorsFromAssemblyContaining<CreateAddressValidator>();

            // MassTransit
            builder.Services.AddMassTransit(x =>
            {
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

            // gRPC Client for Ordering Service
            var orderingServiceUrl = builder.Configuration["GrpcServices:OrderingServiceUrl"] ?? "https://localhost:5003";
            builder.Services.AddGrpcClient<OrderingGrpc.OrderingGrpcClient>(options =>
            {
                options.Address = new Uri(orderingServiceUrl);
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

            builder.Services.AddAuthorization();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "Delivery Service API", Version = "v1" }));

            var app = builder.Build();

            // Seed database
            if (app.Environment.IsDevelopment())
            {
                using var scope = app.Services.CreateScope();
                await DatabaseSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<DeliveryDbContext>());
            }

            app.UseErrorHandling();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();
            app.UseAuthorization();

            // Map Address Endpoints
            app.MapCreateAddressEndpoints();
            app.MapGetUserAddressesEndpoints();
            app.MapUpdateAddressEndpoints();
            app.MapDeleteAddressEndpoints();
            app.MapSetDefaultAddressEndpoints();

            // Map Shipment Endpoints
            app.MapCreateShipmentEndpoints();
            app.MapUpdateShipmentStatusEndpoints();
            app.MapGetShipmentDetailsEndpoints();
            
            // Map Tracking Endpoints (US-E02)
            app.MapUpdateDriverLocationEndpoints();
            app.MapGetDeliveryTrackingEndpoints();
            
            app.MapGet("/", () => "Delivery Service is running...");

            await app.RunAsync();
        }
    }
}

