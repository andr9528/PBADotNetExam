using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Service.Ordering.Domain.Concrete;

namespace Service.Ordering.Repository.EntityFramework.Config
{
    public class OrderConfig : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            // Defining Primary Key -->
            builder.HasKey(s => s.Id);
            builder.Property(x => x.Id).HasColumnName("OrderId");

            // Defining Version as RowVersion -->
            builder.Property(x => x.Version).IsRowVersion();

            builder.Property(x => x.OrderNumber).IsRequired();
            builder.HasIndex(x => x.OrderNumber).IsUnique();

            builder.HasMany(x => (ICollection<Item>) x.Items).WithOne(x => (Order)x.Order).HasForeignKey(x => x.FK_Order).IsRequired(false);
        }
    }
}