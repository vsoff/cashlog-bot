using Cashlog.Core.Models.Main;

namespace Cashlog.Core.Modules.Calculator;

public class ClosingPeriodResult
{
    public required BillingPeriodDto PreviousPeriod { get; init; }
    public required BillingPeriodDto NewPeriod { get; init; }
    public required IReadOnlyCollection<MoneyOperationDto> Debts { get; init; }
}