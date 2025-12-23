using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Auth.Data
{
    public class FlowerEcommerceAuthContextFactory : IDesignTimeDbContextFactory<FlowerEcommerceAuthContext>
    {
        public FlowerEcommerceAuthContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // path للـ project root
                .AddJsonFile("appsettings.json")             // جلب الـ connection string
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<FlowerEcommerceAuthContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);

            return new FlowerEcommerceAuthContext(optionsBuilder.Options);
        }
    }
}
