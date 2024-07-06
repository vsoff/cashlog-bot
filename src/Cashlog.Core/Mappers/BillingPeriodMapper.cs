using Cashlog.Common.Models.Main;
using Cashlog.Data.Entities;
using BillingPeriod = Cashlog.Data.Entities.BillingPeriod;

namespace Cashlog.Core.Mappers;

public static class BillingPeriodMapper
{
    public static BillingPeriod ToData(this BillingPeriodDto obj)
    {
        return new BillingPeriod
        {
            GroupId = obj.GroupId,
            Id = obj.Id,
            PeriodBegin = obj.PeriodBegin,
            PeriodEnd = obj.PeriodEnd
        };
    }

    public static BillingPeriodDto ToCore(this BillingPeriod obj)
    {
        return new BillingPeriodDto
        {
            GroupId = obj.GroupId,
            Id = obj.Id,
            PeriodBegin = obj.PeriodBegin,
            PeriodEnd = obj.PeriodEnd
        };
    }
}