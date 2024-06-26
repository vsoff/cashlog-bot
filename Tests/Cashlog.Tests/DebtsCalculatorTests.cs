using Cashlog.Common;
using Cashlog.Core.Models.Main;
using Cashlog.Core.Modules.Calculator;
using Cashlog.Core.Providers;
using Cashlog.Core.Services;
using Cashlog.Core.Services.Main;
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
        MoneyOperation[] operations =
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
        MoneyOperation[] operations =
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

    // TODO: Вынести в integration tests.
    [Ignore]
    [TestMethod]
    public async Task CalculateTusaMrazeiTest()
    {
        var settingsService = new CashlogSettingsService();
        var provider = new BotDatabaseContextProvider(settingsService);
        var service = new BillingPeriodService(provider);
        var receiptService = new ReceiptService(provider);
        var customerService = new CustomerService(provider);
        var period = await service.GetLastByGroupIdAsync(5);
        var periodReceipts = await receiptService.GetByBillingPeriodIdAsync(period.Id);

        var consumerMap =
            await receiptService.GetConsumerIdsByReceiptIdsMapAsync(periodReceipts.Select(x => x.Id).ToArray());
        var customerNamesMap = (await customerService.GetListAsync(5)).ToDictionary(x => x.Id, x => x.Caption);

        var gg = periodReceipts.Select(x => $"Id{x.Id}: `{x.Comment}`; сумма: {x.TotalAmount}р.; купил: {customerNamesMap[x.CustomerId.Value]}");
        var gg2 = string.Join("\n", gg);
        var all = periodReceipts.Sum(x => x.TotalAmount);
    }

    [TestMethod]
    public async Task Test3()
    {
        MoneyOperation[] operations =
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

    private static MoneyOperation CreateOperation(long fromId, long toId, int amount, MoneyOperationType type)
    {
        return new MoneyOperation()
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