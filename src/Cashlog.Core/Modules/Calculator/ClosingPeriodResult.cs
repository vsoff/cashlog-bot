using Cashlog.Core.Models.Main;

namespace Cashlog.Core.Modules.Calculator;

public class ClosingPeriodResult
{
    public required BillingPeriod PreviousPeriod { get; init; }
    public required BillingPeriod NewPeriod { get; init; }
    public required IReadOnlyCollection<MoneyOperation> Debts { get; init; }
}