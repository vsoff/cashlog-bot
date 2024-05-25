using Cashlog.Common;

namespace Cashlog.Core.Modules.Calculator;

public class MoneyOperationShortInfo
{
    public long FromId { get; set; }
    public long ToId { get; set; }
    public double Amount { get; set; }
    public MoneyOperationType Type { get; set; }
}