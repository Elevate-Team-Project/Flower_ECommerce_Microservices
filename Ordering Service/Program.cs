using BuildingBlocks.Grpc;
using BuildingBlocks.Interfaces;
using BuildingBlocks.SharedEntities;
using FluentValidation;
using Grpc.Net.Client;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering_Service.Entities;
using Ordering_Service.Features.Cart.AddToCart; // ✅ NEW
using Ordering_Service.Features.Cart.Checkout;  // ✅ NEW
using Ordering_Service.Features.Cart.RemoveCartItem; // ✅ NEW
using Ordering_Service.Features.Cart.RemoveProductQuantityInShoppingCart; // ✅ NEW
using Ordering_Service.Features.Cart.UpdateCartItem; // ✅ NEW
using Ordering_Service.Features.Cart.UpdateProductQuantityInShoppingCart; // ✅ NEW
using Ordering_Service.Features.Cart.ViewShoppingCart; // ✅ NEW
using Ordering_Service.Features.Delivery.Addresses.CreateAddress;
using Ordering_Service.Features.Delivery.Addresses.DeleteAddress;
using Ordering_Service.Features.Delivery.Addresses.GetUserAddresses;
using Ordering_Service.Features.Delivery.Addresses.SetDefaultAddress;
using Ordering_Service.Features.Delivery.Addresses.UpdateAddress;
using Ordering_Service.Features.Delivery.Shipments.CreateShipment;
using Ordering_Service.Features.Delivery.Shipments.GetDeliveryTracking;
using Ordering_Service.Features.Delivery.Shipments.GetShipmentDetails;
using Ordering_Service.Features.Delivery.Shipments.UpdateDriverLocation;
using Ordering_Service.Features.Delivery.Shipments.UpdateShipmentStatus;
using Ordering_Service.Features.Orders;
using Ordering_Service.Features.Orders.ConfirmOrder;
using Ordering_Service.Features.Orders.CreateOrder;
using Ordering_Service.Features.Orders.GetOrderDetails;
using Ordering_Service.Features.Orders.GetUserOrders;
using Ordering_Service.Features.Orders.ReOrder;
using Ordering_Service.Features.Orders.UpdateOrderStatus;
using Ordering_Service.Features.Orders.ViewMyOrders;
using Ordering_Service.GrpcServices;
using Ordering_Service.Infrastructure;
using Ordering_Service.Infrastructure.Data;
using Ordering_Service.Infrastructure.UnitOfWork;
using Ordering_Service.MiddleWares;
using Serilog;
using System.Reflection;

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

            // Repositories & UoW - single registrations (avoid duplicates)
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

            // Register open-generic once
            builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            Log.Information("Registered {Count} generic repositories successfully", entityTypes.Count);

            // MediatR - single registration using recommended API
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

            // ✅ NEW: gRPC Client for Promotion Service (Merged into Catalog Service, so same URL)
            builder.Services.AddGrpcClient<PromotionGrpc.PromotionGrpcClient>(options =>
            {
                options.Address = new Uri(catalogServiceUrl);
            })
            .ConfigureChannel(options =>
            {
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
            app.MapGetOrdersStatusEndpoints();
            app.MapReOrderEndpoints();
            app.MapViewMyOrdersEndpoints();

            // ✅ MERGED CART ENDPOINTS
            app.MapAddToCartEndpoints();
            app.MapCheckoutEndpoints();
            app.MapRemoveCartItemEndpoints();
            app.MapViewCartEndpoints();
            app.MapDecreaseItemEndpoints();
            app.MapUpdateItemQuantityEndpoints();
            app.MapUpdateCartItemEndpoints();
            // ...

            // ✅ MERGED DELIVERY ENDPOINTS
            app.MapCreateAddressEndpoints();
            app.MapDeleteAddressEndpoints();
            app.MapGetUserAddressesEndpoints();
            app.MapSetDefaultAddressEndpoints();
            app.MapUpdateAddressEndpoints();

            app.MapCreateShipmentEndpoints();
            app.MapGetDeliveryTrackingEndpoints();
            app.MapGetShipmentDetailsEndpoints();
            app.MapUpdateDriverLocationEndpoints();
            app.MapUpdateShipmentStatusEndpoints();

            app.MapGet("/", () => "Ordering Service is running...");
            await app.RunAsync();
        }
    }
}
