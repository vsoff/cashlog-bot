using Cashlog.Common.Models.Main;
using Cashlog.Data.Entities;
using MoneyOperation = Cashlog.Data.Entities.MoneyOperation;

namespace Cashlog.Core.Mappers;

public static class MoneyOperationMapper
{
    public static MoneyOperation ToData(this MoneyOperationDto obj)
    {
        return new MoneyOperation
        {
            Amount = obj.Amount,
            BillingPeriodId = obj.BillingPeriodId,
            Comment = obj.Comment,
            Id = obj.Id,
            OperationType = obj.OperationType,
            CustomerFromId = obj.CustomerFromId,
            CustomerToId = obj.CustomerToId
        };
    }

    public static MoneyOperationDto ToCore(this MoneyOperation obj)
    {
        return new MoneyOperationDto
        {
            Amount = obj.Amount,
            BillingPeriodId = obj.BillingPeriodId,
            Comment = obj.Comment,
            Id = obj.Id,
            OperationType = obj.OperationType,
            CustomerFromId = obj.CustomerFromId,
            CustomerToId = obj.CustomerToId
        };
    }
}