using Cashlog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cashlog.Data;

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

    public DbSet<Group> Groups { get; set; }
    public DbSet<Receipt> Receipts { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<BillingPeriod> BillingPeriods { get; set; }
    public DbSet<MoneyOperation> MoneyOperations { get; set; }
    public DbSet<ReceiptConsumerMap> ReceiptConsumerMaps { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        switch (_providerType)
        {
            case DataProviderType.MsSql:
                optionsBuilder.UseSqlServer(_connectionString);
                break;

            case DataProviderType.MySql:
                optionsBuilder.UseMySql(_connectionString,
                    builder => { builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null); });
                break;
            
            case DataProviderType.PostgreSql:
                optionsBuilder.UseNpgsql(_connectionString,
                    builder => { builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null); });
                break;

            default: throw new InvalidOperationException($"Неизвестный провайдер данных {_providerType}");
        }
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReceiptConsumerMap>().ToTable(nameof(ReceiptConsumerMaps));
        modelBuilder.Entity<ReceiptConsumerMap>()
            .HasOne(x => x.Consumer)
            .WithMany(x => x.ConsumerReceiptMaps)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ReceiptConsumerMap>()
            .HasOne(x => x.Receipt)
            .WithMany(x => x.ConsumerMaps)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Group>().ToTable(nameof(Groups));
        modelBuilder.Entity<Group>()
            .HasMany(x => x.Customers)
            .WithOne(x => x.Group);
        modelBuilder.Entity<Group>()
            .HasMany(x => x.Receipts)
            .WithOne(x => x.Group);
        modelBuilder.Entity<Group>()
            .HasMany(x => x.BillingPeriods)
            .WithOne(x => x.Group);

        modelBuilder.Entity<Receipt>().ToTable(nameof(Receipts));
        modelBuilder.Entity<Receipt>()
            .HasMany(x => x.ConsumerMaps)
            .WithOne(x => x.Receipt);
        modelBuilder.Entity<Receipt>()
            .HasOne(x => x.Customer)
            .WithMany(x => x.CustomerReceipts);
        modelBuilder.Entity<Receipt>()
            .HasOne(x => x.BillingPeriod)
            .WithMany(x => x.Receipts);

        modelBuilder.Entity<Customer>().ToTable(nameof(Customers));
        modelBuilder.Entity<Customer>()
            .HasMany(x => x.ConsumerReceiptMaps)
            .WithOne(x => x.Consumer);
        modelBuilder.Entity<Customer>()
            .HasMany(x => x.CustomerReceipts)
            .WithOne(x => x.Customer);
        modelBuilder.Entity<Customer>()
            .HasOne(x => x.Group)
            .WithMany(x => x.Customers);

        modelBuilder.Entity<BillingPeriod>().ToTable(nameof(BillingPeriods));
        modelBuilder.Entity<BillingPeriod>()
            .HasOne(x => x.Group)
            .WithMany(x => x.BillingPeriods);
        modelBuilder.Entity<BillingPeriod>()
            .HasMany(x => x.Receipts)
            .WithOne(x => x.BillingPeriod);
        modelBuilder.Entity<BillingPeriod>()
            .HasMany(x => x.MoneyOperations)
            .WithOne(x => x.BillingPeriod);

        modelBuilder.Entity<MoneyOperation>().ToTable(nameof(MoneyOperations));
        modelBuilder.Entity<MoneyOperation>()
            .HasOne(x => x.BillingPeriod)
            .WithMany(x => x.MoneyOperations);
    }
}