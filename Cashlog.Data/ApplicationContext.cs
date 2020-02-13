using System;
using System.IO;
using Cashlog.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Cashlog.Data
{
    public sealed class ApplicationContext : DbContext
    {
        private readonly string _connectionString;
        private readonly DataProviderType _providerType;

        public ApplicationContext(string connectionString, DataProviderType providerType)
        {
            _connectionString = connectionString;
            _providerType = providerType;

            if (Database.EnsureCreated())
                Database.Migrate();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            switch (_providerType)
            {
                case DataProviderType.MsSql:
                    optionsBuilder.UseSqlServer(_connectionString);
                    break;

                case DataProviderType.MySql:
                    optionsBuilder.UseMySql(_connectionString);
                    break;

                default: throw new InvalidOperationException($"Неизвестный провайдер данных {_providerType}");
            }
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReceiptConsumerMapDto>().ToTable(nameof(ReceiptConsumerMaps));
            modelBuilder.Entity<ReceiptConsumerMapDto>().HasOne(x => x.Consumer).WithMany(x => x.ConsumerReceiptMaps).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ReceiptConsumerMapDto>().HasOne(x => x.Receipt).WithMany(x => x.ConsumerMaps).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GroupDto>().ToTable(nameof(Groups));
            modelBuilder.Entity<GroupDto>().HasMany(x => x.Customers).WithOne(x => x.Group);
            modelBuilder.Entity<GroupDto>().HasMany(x => x.Receipts).WithOne(x => x.Group);
            modelBuilder.Entity<GroupDto>().HasMany(x => x.BillingPeriods).WithOne(x => x.Group);

            modelBuilder.Entity<ReceiptDto>().ToTable(nameof(Receipts));
            modelBuilder.Entity<ReceiptDto>().HasMany(x => x.ConsumerMaps).WithOne(x => x.Receipt);
            modelBuilder.Entity<ReceiptDto>().HasOne(x => x.Customer).WithMany(x => x.CustomerReceipts);
            modelBuilder.Entity<ReceiptDto>().HasOne(x => x.BillingPeriod).WithMany(x => x.Receipts);

            modelBuilder.Entity<CustomerDto>().ToTable(nameof(Customers));
            modelBuilder.Entity<CustomerDto>().HasMany(x => x.ConsumerReceiptMaps).WithOne(x => x.Consumer);
            modelBuilder.Entity<CustomerDto>().HasMany(x => x.CustomerReceipts).WithOne(x => x.Customer);
            modelBuilder.Entity<CustomerDto>().HasOne(x => x.Group).WithMany(x => x.Customers);

            modelBuilder.Entity<BillingPeriodDto>().ToTable(nameof(BillingPeriods));
            modelBuilder.Entity<BillingPeriodDto>().HasOne(x => x.Group).WithMany(x => x.BillingPeriods);
            modelBuilder.Entity<BillingPeriodDto>().HasMany(x => x.Receipts).WithOne(x => x.BillingPeriod);
            modelBuilder.Entity<BillingPeriodDto>().HasMany(x => x.MoneyOperations).WithOne(x => x.BillingPeriod);

            modelBuilder.Entity<MoneyOperationDto>().ToTable(nameof(MoneyOperations));
            modelBuilder.Entity<MoneyOperationDto>().HasOne(x => x.BillingPeriod).WithMany(x => x.MoneyOperations);
        }

        public DbSet<GroupDto> Groups { get; set; }
        public DbSet<ReceiptDto> Receipts { get; set; }
        public DbSet<CustomerDto> Customers { get; set; }
        public DbSet<BillingPeriodDto> BillingPeriods { get; set; }
        public DbSet<MoneyOperationDto> MoneyOperations { get; set; }
        public DbSet<ReceiptConsumerMapDto> ReceiptConsumerMaps { get; set; }
    }
}