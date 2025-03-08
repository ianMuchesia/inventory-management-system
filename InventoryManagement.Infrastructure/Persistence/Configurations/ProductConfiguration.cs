using InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Persistence.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // Table configuration
            builder.ToTable("Products");
            
            // Primary Key
            builder.HasKey(p => p.Id);
            
            // Property Configurations
            builder.Property(p => p.Id)
                .HasColumnName("Id")
                .ValueGeneratedOnAdd();
                
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(255);
                
            builder.HasIndex(p => p.Name)
                .IsUnique();
                
            builder.Property(p => p.Description)
                .HasColumnType("NVARCHAR(MAX)");
                
            builder.Property(p => p.Category)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(p => p.UnitPrice)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasColumnType("DECIMAL(18,2)");
                
            builder.Property(p => p.QuantityInStock)
                .IsRequired();
                
            builder.Property(p => p.ReorderLevel)
                .IsRequired();
                
            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();
                
            builder.Property(p => p.UpdatedAt)
                .ValueGeneratedOnUpdate();
                
            // Relationships
            builder.HasMany(p => p.InventoryTransactions)
                .WithOne(t => t.Product)
                .HasForeignKey(t => t.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}