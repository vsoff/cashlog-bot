using Cashlog.Common.Models.Main;

namespace Cashlog.Core.Calculator;

public class ClosingPeriodResult
{
    public required BillingPeriodDto PreviousPeriod { get; init; }
    public required BillingPeriodDto NewPeriod { get; init; }
    public required IReadOnlyCollection<MoneyOperationDto> Debts { get; init; }
}