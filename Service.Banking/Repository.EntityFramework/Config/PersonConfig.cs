using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Service.Banking.Domain.Concrete;

namespace Service.Banking.Repository.EntityFramework.Config
{
    public class PersonConfig : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            // Defining Primary Key -->
            builder.HasKey(s => s.Id);
            builder.Property(x => x.Id).HasColumnName("PersonId");

            // Defining Version as RowVersion -->
            builder.Property(x => x.Version).IsRowVersion();

            builder.HasMany(x => (ICollection<Account>) x.Accounts).WithOne(x => (Person) x.Owner).IsRequired();
            builder.HasIndex(x => x.PersonNumber).IsUnique();
        }
    }
}