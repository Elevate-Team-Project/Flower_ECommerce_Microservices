using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Notification_Service.Features.Inventory;
using Notification_Service.Features.Notifications.GetNotifications;
using Notification_Service.Features.Notifications.MarkRead;
using Notification_Service.Features.Notifications.Consumers;
using Notification_Service.Infrastructure;
using Notification_Service.Services;

namespace Notification_Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Database
            builder.Services.AddDbContext<NotificationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // MediatR
            builder.Services.AddMediatR(cfg => 
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

            // Services
            builder.Services.AddScoped<IEmailSender, MockEmailSender>();

            // MassTransit
            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<ProductLowStockConsumer>();
                x.AddConsumer<OrderStatusChangedConsumer>();
                x.AddConsumer<OfferCreatedConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
                    cfg.Host(rabbitMqHost, "/", h => {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            var app = builder.Build();

            // Apply migrations
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
                context.Database.Migrate();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();
            app.MapGet("/", () => "Notification Service is running...");
            app.UseAuthorization();

            // Map notification endpoints
            app.MapGetNotificationsEndpoints();
            app.MapMarkReadEndpoints();

            app.Run();
        }
    }
}
