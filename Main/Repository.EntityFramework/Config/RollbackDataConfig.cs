using Main.Domain.Concrete;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Repository.EntityFramework.Config
{
    public class RollbackDataConfig : IEntityTypeConfiguration<RollbackData>
    {
        public void Configure(EntityTypeBuilder<RollbackData> builder)
        {
            // Defining Primary Key -->
            builder.HasKey(s => s.Id);
            builder.Property(x => x.Id).HasColumnName("RollbackDataId");

            // Defining Version as RowVersion -->
            builder.Property(x => x.Version).IsRowVersion();
        }
    }
}