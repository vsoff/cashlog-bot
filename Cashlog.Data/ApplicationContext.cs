using System.IO;
using Cashlog.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Cashlog.Data
{
    public sealed class ApplicationContext : DbContext
    {
        private readonly string _connectionString;

        public ApplicationContext(string connectionString)
        {
            _connectionString = connectionString;

            if (Database.EnsureCreated())
                Database.Migrate();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(_connectionString);


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ReceiptConsumerMapDto>().ToTable(nameof(ReceiptConsumerMaps));
            modelBuilder.Entity<ReceiptConsumerMapDto>().HasOne(x => x.Consumer).WithMany(x => x.ConsumerReceiptMaps).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ReceiptConsumerMapDto>().HasOne(x => x.Receipt).WithMany(x => x.ConsumerMaps).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GroupDto>().ToTable(nameof(Groups));
            modelBuilder.Entity<GroupDto>().HasMany(x => x.Customers).WithOne(x => x.Group);
            modelBuilder.Entity<GroupDto>().HasMany(x => x.Receipts).WithOne(x => x.Group);
            modelBuilder.Entity<GroupDto>().HasMany(x => x.BillingTimes).WithOne(x => x.Group);

            modelBuilder.Entity<ReceiptDto>().ToTable(nameof(Receipts));
            modelBuilder.Entity<ReceiptDto>().HasMany(x => x.ConsumerMaps).WithOne(x => x.Receipt);
            modelBuilder.Entity<ReceiptDto>().HasOne(x => x.Customer).WithMany(x => x.CustomerReceipts);
            modelBuilder.Entity<ReceiptDto>().HasOne(x => x.Group).WithMany(x => x.Receipts);
            modelBuilder.Entity<ReceiptDto>().HasOne(x => x.BillingTime).WithMany(x => x.Receipts);

            modelBuilder.Entity<CustomerDto>().ToTable(nameof(Customers));
            modelBuilder.Entity<CustomerDto>().HasMany(x => x.ConsumerReceiptMaps).WithOne(x => x.Consumer);
            modelBuilder.Entity<CustomerDto>().HasMany(x => x.CustomerReceipts).WithOne(x => x.Customer);
            modelBuilder.Entity<CustomerDto>().HasOne(x => x.Group).WithMany(x => x.Customers);

            modelBuilder.Entity<BillingTimeDto>().ToTable(nameof(BillingTimes));
            modelBuilder.Entity<BillingTimeDto>().HasOne(x => x.Group).WithMany(x => x.BillingTimes);
            modelBuilder.Entity<BillingTimeDto>().HasMany(x => x.Receipts).WithOne(x => x.BillingTime);
            modelBuilder.Entity<BillingTimeDto>().HasMany(x => x.MoneyOperations).WithOne(x => x.BillingTime);

            modelBuilder.Entity<MoneyOperationDto>().ToTable(nameof(MoneyOperations));
            modelBuilder.Entity<MoneyOperationDto>().HasOne(x => x.BillingTime).WithMany(x => x.MoneyOperations);
        }

        public DbSet<GroupDto> Groups { get; set; }
        public DbSet<ReceiptDto> Receipts { get; set; }
        public DbSet<CustomerDto> Customers { get; set; }
        public DbSet<BillingTimeDto> BillingTimes { get; set; }
        public DbSet<MoneyOperationDto> MoneyOperations { get; set; }
        public DbSet<ReceiptConsumerMapDto> ReceiptConsumerMaps { get; set; }
    }
}