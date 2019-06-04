using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Service.Banking.Domain.Concrete;

namespace Service.Banking.Repository.EntityFramework.Config
{
    public class AccountConfig : AbstractValidator<Account>, IEntityTypeConfiguration<Account> 
    {
        public AccountConfig()
        {
            RuleFor(x => x.Balance).GreaterThanOrEqualTo(0).WithMessage("Account Balance cannot be below zero!");
        }

        public void Configure(EntityTypeBuilder<Account> builder)
        {
            // Defining Primary Key -->
            builder.HasKey(s => s.Id);
            builder.Property(x => x.Id).HasColumnName("AccountId");

            // Defining Version as RowVersion -->
            builder.Property(x => x.Version).IsRowVersion();

            builder.Property(x => x.AccountNumber).IsRequired();
            builder.HasIndex(x => x.AccountNumber).IsUnique();
        }
    }
}