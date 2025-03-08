using InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
           
            
            // Primary Key
            builder.HasKey(u => u.Id);
            
            // Property Configurations
            builder.Property(u => u.Id)
                .HasColumnName("Id")
                .ValueGeneratedOnAdd();
                
            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.HasIndex(u => u.Username)
                .IsUnique();
                
            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);
                
            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);
                
            builder.HasIndex(u => u.Email)
                .IsUnique();
                
            builder.Property(u => u.Role)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(u => u.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
                
            builder.Property(u => u.LastLoginDate);
        }
    }
}