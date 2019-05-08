using System.Collections.Generic;
using Main.Domain.Concrete;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.Repository.EntityFramework.Config
{
    public class EventConfig : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            // Defining Primary Key -->
            builder.HasKey(s => s.Id);
            builder.Property(x => x.Id).HasColumnName("EventId");

            // Defining Version as RowVersion -->
            builder.Property(x => x.Version).IsRowVersion();

            builder.HasMany(x => (ICollection<RollbackData>)x.RollbackDatas).WithOne().IsRequired();
        }
    }
}