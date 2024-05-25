using Cashlog.Core.Models.Main;
using Cashlog.Data.Entities;

namespace Cashlog.Core.Mappers;

public static class BillingPeriodMapper
{
    public static BillingPeriodDto ToData(this BillingPeriod obj)
    {
        return new BillingPeriodDto
        {
            GroupId = obj.GroupId,
            Id = obj.Id,
            PeriodBegin = obj.PeriodBegin,
            PeriodEnd = obj.PeriodEnd
        };
    }

    public static BillingPeriod ToCore(this BillingPeriodDto obj)
    {
        return new BillingPeriod
        {
            GroupId = obj.GroupId,
            Id = obj.Id,
            PeriodBegin = obj.PeriodBegin,
            PeriodEnd = obj.PeriodEnd
        };
    }
}