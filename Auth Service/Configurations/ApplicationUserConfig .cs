namespace Auth.Data.Configurations
{
    using global::Auth.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    namespace Auth.Data.Configurations
    {
        public class ApplicationUserConfig : IEntityTypeConfiguration<ApplicationUser>
        {
            public void Configure(EntityTypeBuilder<ApplicationUser> builder)
            {
                builder.Property(u => u.FullName)
                    .IsRequired()
                    .HasMaxLength(150);
                
                builder.Property(u => u.FullName)
                        .HasMaxLength(150)
                        .HasComputedColumnSql("[FirstName] + ' ' + [LastName]");


              
            }
        }
    }

}
