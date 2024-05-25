using Cashlog.Core.Mappers;
using Cashlog.Core.Models.Main;
using Cashlog.Core.Providers.Abstract;
using Cashlog.Core.Services.Abstract;
using Cashlog.Data.UoW;

namespace Cashlog.Core.Services.Main;

public class CustomerService : ICustomerService
{
    private readonly IDatabaseContextProvider _databaseContextProvider;

    public CustomerService(IDatabaseContextProvider databaseContextProvider)
    {
        _databaseContextProvider =
            databaseContextProvider ?? throw new ArgumentNullException(nameof(databaseContextProvider));
    }

    public async Task<Customer> AddAsync(Customer customer)
    {
        using var uow = new UnitOfWork(_databaseContextProvider.Create());
        var newCustomer = await uow.Customers.AddAsync(customer.ToData());
        await uow.SaveChangesAsync();
        return newCustomer.ToCore();
    }

    public async Task<Customer> GetAsync(long customerId)
    {
        using var uow = new UnitOfWork(_databaseContextProvider.Create());
        return (await uow.Customers.GetAsync(customerId))?.ToCore();
    }

    public async Task<Customer[]> GetListAsync(long[] customerIds)
    {
        using var uow = new UnitOfWork(_databaseContextProvider.Create());
        return (await uow.Customers.GetListAsync(customerIds))?.Select(x => x.ToCore()).ToArray();
    }

    public async Task<Customer[]> GetListAsync(long groupId)
    {
        using var uow = new UnitOfWork(_databaseContextProvider.Create());
        return (await uow.Customers.GetByGroupId(groupId))?.Select(x => x.ToCore()).ToArray();
    }
}