
using BuildingBlocks.Interfaces;
using BuildingBlocks.SharedEntities;
using Catalog_Service.Entities;
using Catalog_Service.Infrastructure;
using Catalog_Service.Infrastructure.Data;
using FluentAssertions.Common;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;

namespace Catalog_Service
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Generic Repository Registration
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

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<Infrastructure.Data.ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            var app = builder.Build();


            // -------------------------------------------------------------------------------------
            // 3. Database Migration & Seeding (Startup Scope)
            // -------------------------------------------------------------------------------------
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    await context.Database.MigrateAsync();
                    await DatabaseSeeder.SeedAsync(services);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error occurred during database migration or seeding");
                    if (app.Environment.IsDevelopment()) throw;
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.MapGet("/", () => "Catalog Service is running.");
            app.MapGet("/Brands", async (IBaseRepository<Brand> BrandRepo) => Results.Ok(await BrandRepo.GetAll().ToListAsync()));

            app.Run();
        }
    }
}
