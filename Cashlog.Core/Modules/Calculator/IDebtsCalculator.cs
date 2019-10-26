using System.Threading.Tasks;
using Cashlog.Core.Core.Models;
using Cashlog.Core.Core.Services;

namespace Cashlog.Core.Modules.Calculator
{
    public interface IDebtsCalculator
    {
        Task<MoneyOperationShortInfo[]> Calculate(MoneyOperation[] operations, ReceiptCalculatorInfo[] receiptCalculatorInfos);
    }
}