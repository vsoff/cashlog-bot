using Cashlog.Common.Models.Main;
using Cashlog.Data.Entities;
using Customer = Cashlog.Data.Entities.Customer;

namespace Cashlog.Core.Mappers;

public static class CustomerMapper
{
    public static Customer ToData(this CustomerDto obj)
    {
        return new Customer
        {
            Caption = obj.Caption,
            GroupId = obj.GroupId,
            Id = obj.Id,
            IsDeleted = obj.IsDeleted
        };
    }

    public static CustomerDto ToCore(this Customer obj)
    {
        return new CustomerDto
        {
            Caption = obj.Caption,
            GroupId = obj.GroupId,
            Id = obj.Id,
            IsDeleted = obj.IsDeleted
        };
    }
}