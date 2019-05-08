using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Service.Ordering.Domain.Concrete;

namespace Service.Ordering.Repository.EntityFramework.Config
{
    public class ItemConfig : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            // Defining Primary Key -->
            builder.HasKey(s => s.Id);
            builder.Property(x => x.Id).HasColumnName("ItemId");

            // Defining Version as RowVersion -->
            builder.Property(x => x.Version).IsRowVersion();

            builder.Property(x => x.ItemNumber).IsRequired();
            builder.HasIndex(x => x.ItemNumber).IsUnique();
        }
    }
}