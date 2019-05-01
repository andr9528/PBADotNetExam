using Domain.Concrete;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.EntityFramework.Config
{

    // For each class you want to persist, you need to create a config class,
    // which tells Entity Framework, how to save the entities to the database.

    // Domain classes will most likely have navigational properties as interface types,
    // during the config thess interface types need to be cast to the concrete versions.

    // e.g
    /*
    internal class YourDomainClassConfig : IEntityTypeConfiguration<YourDomainClass>
    {
        public YourDomainClassConfig()
        {
        }

        public void Configure(EntityTypeBuilder<YourDomainClass> builder)
        {
            // As all domain classes should implements a interface, that inherits from IEntity, in Repository.Core,
                // a property with the name of Id should exist
            
            // Defining Primary Key -->
            builder.HasKey(s => s.Id);
            builder.Property(x => x.Id).HasColumnName("YourDomainClassId");
			
			// Defining Version as RowVersion -->
            builder.Property(x => x.Version).IsRowVersion();

            // Defining whether or not a property is required 
                // Takes in a false in the 'IsRequred' if one wish to make it not required
                // Properteis that are nullable default to be not required -->
            builder.Property(x => x.PropertyA).IsRequired();

            // Defining a One-To-Many relation -->
            builder.HasOne(x => (YourSecoundDomainClass)x.YourSecoundDomainClass)
                .WithMany(x => (ICollection<YourDomainClass>)x.YourDomainClassInPlural).HasForeignKey(x => x.FK_YourDomainClassId);

            // Defining a Many-To-One relation -->
            builder.HasMany(x => (ICollection<YourThirdDomainClass>)x.YourThirdDomainClassInPlural)
                .WithOne(x => (YourDomainClass)x.YourDomainClass).HasForeignKey(x => x.FK_YourDomainClassId).HasPrincipalKey(x => x.Id);

            // Defining a One-To-One relation -->
            builder.HasOne(x => (YourFourthDomainClass)x.YourFourthDomainClass)
                .WithOne(x => (YourDomainClass)x.YourDomainClass).HasForeignKey<YourFourthDomainClass>(x => x.Id);
            
                // Use the following line in the Configure on the foreign site, meaning YourFourthDomainClass in this example.     
                builder.Property(x => x.Id).ValueGeneratedNever();

            // Defining that a property need to be unique in the database -->
            builder.HasIndex(x => x.PropertyA).IsUnique();

            // Defining that a combination of several properties need to be unique in the database -->
            builder.HasIndex(x => new { x.PropertyA, x.PropertyB, x.PropertyC, x.PropertyD }).HasName("SomeName").IsUnique();
        }
    }
    */
}