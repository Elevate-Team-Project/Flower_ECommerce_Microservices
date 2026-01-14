using MassTransit;
using Notification_Service.Features.Inventory;
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

            // Services
            builder.Services.AddScoped<IEmailSender, MockEmailSender>();

            // MassTransit
            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<ProductLowStockConsumer>();

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

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();
            app.MapGet("/", () => "Notification Service is running...");
            app.UseAuthorization();

            app.Run();
        }
    }
}
