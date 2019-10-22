using System;
using System.Linq;
using System.Threading.Tasks;
using Cashlog.Core.Core.Models;
using Cashlog.Core.Data.Mappers;
using Cashlog.Data.Entities;
using Cashlog.Data.UoW;

namespace Cashlog.Core.Core.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICashogSettings _cashogSettings;

        public CustomerService(ICashogSettings cashogSettings)
        {
            _cashogSettings = cashogSettings ?? throw new ArgumentNullException(nameof(cashogSettings));
        }

        public async Task<Customer> Add(Customer customer)
        {
            using (var uow = new UnitOfWork(_cashogSettings.DataBaseConnectionString))
            {
                CustomerDto newCustomer = await uow.Customers.AddAsync(customer.ToData());
                await uow.SaveChangesAsync();
                return newCustomer.ToCore();
            }
        }

        public async Task<Customer> Get(long customerId)
        {
            using (var uow = new UnitOfWork(_cashogSettings.DataBaseConnectionString))
            {
                return (await uow.Customers.GetAsync(customerId))?.ToCore();
            }
        }

        public async Task<Customer[]> GetByGroupId(long groupId)
        {
            using (var uow = new UnitOfWork(_cashogSettings.DataBaseConnectionString))
            {
                return (await uow.Customers.GetByGroupId(groupId))?.Select(x => x.ToCore()).ToArray();
            }
        }
    }
}