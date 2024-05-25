using System.Threading.Tasks;
using Cashlog.Core.Models.Main;

namespace Cashlog.Core.Modules.Calculator
{
    public interface IDebtsCalculator
    {
        Task<MoneyOperationShortInfo[]> Calculate(MoneyOperation[] operations, ReceiptCalculatorInfo[] receiptCalculatorInfos);
    }
}