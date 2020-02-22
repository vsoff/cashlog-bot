using Cashlog.Core.Models;
using Cashlog.Core.Models.Main;
using Cashlog.Data.Entities;

namespace Cashlog.Core.Mappers
{
    public static class MoneyOperationMapper
    {
        public static MoneyOperationDto ToData(this MoneyOperation obj)
        {
            return new MoneyOperationDto
            {
                Amount = obj.Amount,
                BillingPeriodId = obj.BillingPeriodId,
                Comment = obj.Comment,
                Id = obj.Id,
                OperationType = (MoneyOperationTypeDto) obj.OperationType,
                CustomerFromId = obj.CustomerFromId,
                CustomerToId = obj.CustomerToId
            };
        }

        public static MoneyOperation ToCore(this MoneyOperationDto obj)
        {
            return new MoneyOperation
            {
                Amount = obj.Amount,
                BillingPeriodId = obj.BillingPeriodId,
                Comment = obj.Comment,
                Id = obj.Id,
                OperationType = (MoneyOperationType) obj.OperationType,
                CustomerFromId = obj.CustomerFromId,
                CustomerToId = obj.CustomerToId
            };
        }
    }
}