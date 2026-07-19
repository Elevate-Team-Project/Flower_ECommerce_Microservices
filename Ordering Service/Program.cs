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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ordering_Service.Consumers;
using System.Text;

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
                x.AddConsumer<CartCheckoutEventConsumer>();
                x.AddConsumer<PaymentSucceededConsumer>();
                x.AddConsumer<CodOrderApprovedConsumer>();

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

            builder.Services.AddHttpClient("CatalogService", client =>
            {
                var catalogUrl = builder.Configuration["ServiceClients:CatalogServiceUrl"] ?? "http://catalogservice:8080/";
                client.BaseAddress = new Uri(catalogUrl);
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
                await DatabaseSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<OrderingDbContext>());
            }

            app.UseErrorHandling();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();
            app.UseAuthentication();
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

            // Check if product exists in active orders (for Catalog Service deletion validation)
            app.MapGet("/api/orders/check-product/{productId:int}", async (
                int productId,
                IBaseRepository<Order> orderRepository) =>
            {
                var activeStatuses = new[] { "Delivered", "Cancelled" };
                var activeOrderCount = await orderRepository.GetAll()
                    .Include(o => o.Items)
                    .Where(o => !activeStatuses.Contains(o.Status))
                    .Where(o => o.Items.Any(i => i.ProductId == productId))
                    .CountAsync();

                return Results.Ok(new
                {
                    Success = true,
                    HasActiveOrders = activeOrderCount > 0,
                    ActiveOrderCount = activeOrderCount,
                    Message = activeOrderCount > 0
                        ? $"Product is in {activeOrderCount} active order(s)"
                        : "Product is not in any active orders"
                });
            });

            // ✅ MERGED CART ENDPOINTS
            app.MapAddToCartEndpoints();
            app.MapCheckoutEndpoints();
            app.MapRemoveCartItemEndpoints();
            app.MapViewCartEndpoints();
            app.MapDecreaseItemEndpoints();
            app.MapUpdateItemQuantityEndpoints();
            app.MapUpdateCartItemEndpoints();
            // ...



            app.MapGet("/", () => "Ordering Service is running...");
            await app.RunAsync();
        }
    }
}
