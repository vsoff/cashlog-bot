using Cashlog.Core.Models.Main;

namespace Cashlog.Core.Modules.Calculator;

public interface IDebtsCalculator
{
    Task<MoneyOperationShortInfo[]> Calculate(MoneyOperationDto[] operations,
        ReceiptCalculatorInfo[] receiptCalculatorInfos);
}