using Cashlog.Core.Models.Main;
using Cashlog.Data.Entities;
using BillingPeriod = Cashlog.Data.Entities.BillingPeriod;

namespace Cashlog.Core.Mappers;

public static class BillingPeriodMapper
{
    public static BillingPeriod ToData(this Models.Main.BillingPeriodDto obj)
    {
        return new BillingPeriod
        {
            GroupId = obj.GroupId,
            Id = obj.Id,
            PeriodBegin = obj.PeriodBegin,
            PeriodEnd = obj.PeriodEnd
        };
    }

    public static Models.Main.BillingPeriodDto ToCore(this BillingPeriod obj)
    {
        return new Models.Main.BillingPeriodDto
        {
            GroupId = obj.GroupId,
            Id = obj.Id,
            PeriodBegin = obj.PeriodBegin,
            PeriodEnd = obj.PeriodEnd
        };
    }
}