using Cashlog.Core.Models.Main;
using Cashlog.Web.Shared.Contracts;

namespace Cashlog.Web.Server.Core.Mappers
{
    public static class CustomerMapper
    {
        public static CustomerWebModel ToModel(this Customer source)
        {
            if (source == null) return null;
            return new CustomerWebModel
            {
                Caption = source.Caption,
                GroupId = source.GroupId,
                Id = source.Id,
                IsDeleted = source.IsDeleted
            };
        }
    }
}