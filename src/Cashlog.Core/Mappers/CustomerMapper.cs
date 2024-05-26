using Cashlog.Core.Models.Main;
using Cashlog.Data.Entities;
using Customer = Cashlog.Data.Entities.Customer;

namespace Cashlog.Core.Mappers;

public static class CustomerMapper
{
    public static Customer ToData(this Models.Main.CustomerDto obj)
    {
        return new Customer
        {
            Caption = obj.Caption,
            GroupId = obj.GroupId,
            Id = obj.Id,
            IsDeleted = obj.IsDeleted
        };
    }

    public static Models.Main.CustomerDto ToCore(this Customer obj)
    {
        return new Models.Main.CustomerDto
        {
            Caption = obj.Caption,
            GroupId = obj.GroupId,
            Id = obj.Id,
            IsDeleted = obj.IsDeleted
        };
    }
}