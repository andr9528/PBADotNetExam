﻿using Microsoft.EntityFrameworkCore;
using System;
using Service.Banking.Domain.Concrete;
using Service.Banking.Repository.EntityFramework.Config;

namespace Service.Banking.Repository.EntityFramework
{
    public class EntityRepository : DbContext
    {
        private readonly DbContextOptions<EntityRepository> options;
        private readonly bool useLazyLoading;
        public EntityRepository(DbContextOptions<EntityRepository> options) : base(options)
        {
            this.options = options;
            useLazyLoading = true;

            //Database.Migrate();
        }

        // Underneath create as many DbSet' as you have domain classes you wish to persist.
        // Each DbSet should have a Config file applied in the method 'OnModelCreating'

        // e.g
        // public virtual DbSet<YourDomainClass> YourDomainClassInPlural { get; set; }
        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<Person> Persons { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // To Lazy Load properties they require the keyword Virtual.
            // Making use of Lazy load means that the property only loads as it is about to be used,
            // which improves performance of the program
            optionsBuilder.UseLazyLoadingProxies(useLazyLoading);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Create a new class under Config, with the name of the domain class you wish to persist,
            // ending it in Config, to differentiate it from the actual class

            // e.g
            // modelBuilder.ApplyConfiguration(new YourDomainClassConfig());
            modelBuilder.ApplyConfiguration(new AccountConfig());
            modelBuilder.ApplyConfiguration(new PersonConfig());
        }
    }
}
