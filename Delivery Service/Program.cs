using BuildingBlocks.Interfaces;
using Delivery_Service.Entities;
using Delivery_Service.Features.Addresses.CreateAddress;
using Delivery_Service.Features.Addresses.CreateAddress.Create_Address_automatically;
using Delivery_Service.Features.Addresses.DeleteAddress;
using Delivery_Service.Features.Addresses.GetUserAddresses;
using Delivery_Service.Features.Addresses.SetDefaultAddress;
using Delivery_Service.Features.Addresses.UpdateAddress;
using Delivery_Service.Features.Shipments.CreateShipment;
using Delivery_Service.Features.Shipments.GetShipmentDetails;
using Delivery_Service.Features.Shipments.UpdateShipmentStatus;
using Delivery_Service.Infrastructure           ;
using Delivery_Service.Infrastructure.Data;
using Delivery_Service.Infrastructure.UnitOfWork;
using Delivery_Service.MiddleWares;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

            builder.Services.AddMemoryCache();

            // Repositories & UoW
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IBaseRepository<UserAddress>, BaseRepository<UserAddress>>();
            builder.Services.AddScoped<IBaseRepository<Shipment>, BaseRepository<Shipment>>();
            builder.Services.AddScoped<IBaseRepository<DeliveryZone>, BaseRepository<DeliveryZone>>();
            builder.Services.AddScoped<IBaseRepository<DeliveryAddress>, BaseRepository<DeliveryAddress>>();

            // Geocoding (Nominatim, free, with in-memory caching)
            // Infrastructure.Services.IGeocodingService for reverse geocoding (coordinates -> address)
            builder.Services.AddHttpClient<Infrastructure.Services.IGeocodingService, Infrastructure.Services.CachedOsmGeocodingService>();
            builder.Services.AddScoped<Delivery_Service.Infrastructure.Services.CachedOsmGeocodingService>();
            builder.Services.AddScoped<Delivery_Service.Infrastructure.Services.IGeocodingService>(sp =>
                (Delivery_Service.Infrastructure.Services.IGeocodingService)sp.GetRequiredService<Delivery_Service.Infrastructure.Services.CachedOsmGeocodingService>());

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

            app.UseHttpsRedirection();
            app.UseAuthorization();

            // Map Address Endpoints
            app.MapCreateAddressEndpoints();
            app.MapCreateAddressAutomaticallyEndpoints();
            app.MapGetUserAddressesEndpoints();
            app.MapUpdateAddressEndpoints();
            app.MapDeleteAddressEndpoints();
            app.MapSetDefaultAddressEndpoints();

            // Map Shipment Endpoints
            app.MapCreateShipmentEndpoints();
            app.MapUpdateShipmentStatusEndpoints();
            app.MapGetShipmentDetailsEndpoints();

            await app.RunAsync();
        }
    }
}
