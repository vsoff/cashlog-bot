using Cashlog.Core.Core.Models;
using Cashlog.Core.Core.Services;
using Cashlog.Core.Modules.Calculator;

namespace Cashlog.Core.Tests
{
    internal static class DebtsCalculatorTestHelper
    {
        public static MoneyOperation NewOperation(long fromId, long toId, int amount, MoneyOperationType type)
        {
            return new MoneyOperation
            {
                Amount = amount,
                CustomerFromId = fromId,
                CustomerToId = toId,
                OperationType = type
            };
        }

        public static ReceiptCalculatorInfo NewReceipt(long customerId, long[] consumerIds, int amount)
        {
            return new ReceiptCalculatorInfo
            {
                Amount = amount,
                CustomerId = customerId,
                ConsumerIds = consumerIds,
            };
        }
    }
}