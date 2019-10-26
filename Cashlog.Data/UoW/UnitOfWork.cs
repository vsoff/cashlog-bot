using System;
using System.Threading.Tasks;
using Cashlog.Data.UoW.Repositories;

namespace Cashlog.Data.UoW
{
    public class UnitOfWork : IDisposable
    {
        private readonly ApplicationContext _context;

        public UnitOfWork(string connectionString)
        {
            _context = new ApplicationContext(connectionString);

            Groups = new GroupRepository(_context);
            Receipts = new ReceiptRepository(_context);
            Customers = new CustomerRepository(_context);
            BillingPeriods = new BillingPeriodRepository(_context);
            MoneyOperations = new MoneyOperationRepository(_context);
            ReceiptConsumerMaps = new ReceiptConsumerMapRepository(_context);
        }

        public GroupRepository Groups { get; }
        public ReceiptRepository Receipts { get; }
        public CustomerRepository Customers { get; }
        public BillingPeriodRepository BillingPeriods { get; }
        public MoneyOperationRepository MoneyOperations { get; }
        public ReceiptConsumerMapRepository ReceiptConsumerMaps { get; }

        public void Dispose() => _context.Dispose();
        public void SaveChanges() => _context.SaveChanges();
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}