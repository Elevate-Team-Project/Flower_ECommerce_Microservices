namespace API_Gateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ============================================
            // YARP Reverse Proxy Configuration
            // ============================================
            builder.Services.AddReverseProxy()
                .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

            // ============================================
            // CORS Configuration
            // ============================================
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("GatewayPolicy", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // ============================================
            // Health Checks
            // ============================================
            builder.Services.AddHealthChecks();

            // ============================================
            // Swagger/OpenAPI
            // ============================================
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Flower E-Commerce API Gateway",
                    Version = "v1",
                    Description = "Unified entry point for all Flower E-Commerce microservices"
                });
            });

            var app = builder.Build();

            // ============================================
            // Middleware Pipeline
            // ============================================
            
            // CORS must be first
            app.UseCors("GatewayPolicy");

            // Health check endpoint
            app.MapHealthChecks("/health");

            // Gateway info endpoint
            app.MapGet("/", () => Results.Ok(new
            {
                Service = "Flower E-Commerce API Gateway",
                Version = "1.0.0",
                Status = "Running",
                Timestamp = DateTime.UtcNow
            }));

            // Swagger (all environments in gateway for API documentation)
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway API v1");
                c.RoutePrefix = "swagger";
            });

            // YARP Reverse Proxy - routes all /api/* requests to downstream services
            app.MapReverseProxy();

            Console.WriteLine("🌸 [Gateway] Flower E-Commerce API Gateway is starting...");
            Console.WriteLine("📡 [Gateway] Routing requests to downstream microservices");

            app.Run();
        }
    }
}
