using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagement.Infrastructure.Persistence.Configurations
{
    public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
    {
        public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
        {
            
            
            // Primary key
            builder.HasKey(t => t.Id);
            
            // Properties
            builder.Property(t => t.Id)
                .HasColumnName("Id")
                .ValueGeneratedOnAdd();  // IDENTITY column
                
            builder.Property(t => t.ProductId)
                .HasColumnName("ProductId")
                .IsRequired();
                
            builder.Property(t => t.Type)
                .HasColumnName("Type")
                .HasConversion<int>()  // Store enum as int
                .IsRequired();
                
            builder.Property(t => t.Quantity)
                .HasColumnName("Quantity")
                .IsRequired();
                
            builder.Property(t => t.TransactionDate)
                .HasColumnName("TransactionDate")
                .HasDefaultValueSql("GETUTCDATE()");
                
            builder.Property(t => t.Notes)
                .HasColumnName("Notes")
                .HasMaxLength(500);
                
            // Relationships 
            builder.HasOne<Product>()
                .WithMany()
                .HasForeignKey(t => t.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Ignore private setters in EF Core
            builder.Property(t => t.TransactionDate).Metadata.SetBeforeSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
            builder.Property(t => t.Notes).Metadata.SetBeforeSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
        }
    }
}