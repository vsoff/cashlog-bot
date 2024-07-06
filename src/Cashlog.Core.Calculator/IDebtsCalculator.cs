using Cashlog.Common.Models.Main;

namespace Cashlog.Core.Calculator;

public interface IDebtsCalculator
{
    Task<MoneyOperationShortInfo[]> Calculate(MoneyOperationDto[] operations,
        ReceiptCalculatorInfo[] receiptCalculatorInfos);
}