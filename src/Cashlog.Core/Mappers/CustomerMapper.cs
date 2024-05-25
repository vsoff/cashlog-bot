using Cashlog.Core.Models.Main;
using Cashlog.Data.Entities;

namespace Cashlog.Core.Mappers
{
    public static class CustomerMapper
    {
        public static CustomerDto ToData(this Customer obj)
        {
            return new CustomerDto
            {
                Caption = obj.Caption,
                GroupId = obj.GroupId,
                Id = obj.Id,
                IsDeleted = obj.IsDeleted
            };
        }

        public static Customer ToCore(this CustomerDto obj)
        {
            return new Customer
            {
                Caption = obj.Caption,
                GroupId = obj.GroupId,
                Id = obj.Id,
                IsDeleted = obj.IsDeleted
            };
        }
    }
}