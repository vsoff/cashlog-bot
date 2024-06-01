using Cashlog.Common;
using Cashlog.Core.Models.Main;
using Cashlog.Core.Modules.Calculator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cashlog.Tests;

[TestClass]
public class DebtsCalculatorTests
{
    private IDebtsCalculator _calculator;

    [TestInitialize]
    public void TestMethod1()
    {
        _calculator = new DebtsCalculator();
    }

    [TestMethod]
    public async Task Test1()
    {
        MoneyOperationDto[] operations =
        {
            CreateOperation(2, 1, 200, MoneyOperationType.Debt),
            CreateOperation(3, 1, 500, MoneyOperationType.Debt),
            CreateOperation(2, 1, 200, MoneyOperationType.Transfer),
            CreateOperation(3, 1, 300, MoneyOperationType.Transfer)
        };
        ReceiptCalculatorInfo[] receipts =
        {
            CreateReceipt(1, new long[] { 1, 2, 3 }, 300),
            CreateReceipt(1, new long[] { 1, 2, 3 }, 600),
            CreateReceipt(3, new long[] { 1, 3 }, 3000),
            CreateReceipt(2, new long[] { 1, 2 }, 900),
            CreateReceipt(2, new long[] { 3, 2 }, 900)
        };

        var debts = await _calculator.Calculate(operations, receipts);

        Assert.IsNotNull(debts);
        Assert.IsTrue(debts.Length > 0);
    }

    [TestMethod]
    public async Task Test2()
    {
        MoneyOperationDto[] operations =
        {
            //CreateOperation(1, 2, 400, MoneyOperationType.Debt),
            //CreateOperation(3, 1, 200, MoneyOperationType.Debt),
            //CreateOperation(1, 2, 100, MoneyOperationType.Transfer),
            //CreateOperation(1, 3, 300, MoneyOperationType.Transfer),
            //CreateOperation(2, 3, 400, MoneyOperationType.Transfer),
        };
        ReceiptCalculatorInfo[] receipts =
        {
            CreateReceipt(1, new long[] { 1, 3 }, 300),
            CreateReceipt(2, new long[] { 1, 2, 3 }, 600),
            CreateReceipt(3, new long[] { 1, 2, 3 }, 600),
            CreateReceipt(1, new long[] { 1, 2, 3 }, 1000)
        };

        var debts = await _calculator.Calculate(operations, receipts);

        Assert.IsNotNull(debts);
        Assert.IsTrue(debts.Length > 0);
    }

    [TestMethod]
    public async Task Test3()
    {
        MoneyOperationDto[] operations =
        {
            //CreateOperation(1, 2, 400, MoneyOperationType.Debt),
            //CreateOperation(3, 1, 200, MoneyOperationType.Debt),
            //CreateOperation(1, 2, 100, MoneyOperationType.Transfer),
            //CreateOperation(1, 3, 300, MoneyOperationType.Transfer),
            //CreateOperation(2, 3, 400, MoneyOperationType.Transfer),
        };
        ReceiptCalculatorInfo[] receipts =
        {
            CreateReceipt(1, new long[] { 1, 2 }, 300),
            CreateReceipt(2, new long[] { 2, 3 }, 300)
        };

        var debts = await _calculator.Calculate(operations, receipts);

        Assert.IsNotNull(debts);
        Assert.IsTrue(debts.Length > 0);
    }

    private static MoneyOperationDto CreateOperation(long fromId, long toId, int amount, MoneyOperationType type)
    {
        return new MoneyOperationDto()
        {
            Amount = amount,
            CustomerFromId = fromId,
            CustomerToId = toId,
            OperationType = type
        };
    }

    private static ReceiptCalculatorInfo CreateReceipt(long customerId, long[] consumerIds, int amount)
    {
        return new ReceiptCalculatorInfo()
        {
            Amount = amount,
            CustomerId = customerId,
            ConsumerIds = consumerIds
        };
    }
}