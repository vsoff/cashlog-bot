using System.Threading.Tasks;
using Cashlog.Core.Core.Models;
using Cashlog.Core.Modules.Calculator;
using Cashlog.Core.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cashlog.Tests
{
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
                DebtsCalculatorTestHelper.NewOperation(2, 1, 200, MoneyOperationType.Debt),
                DebtsCalculatorTestHelper.NewOperation(3, 1, 500, MoneyOperationType.Debt),
                DebtsCalculatorTestHelper.NewOperation(2, 1, 200, MoneyOperationType.Transfer),
                DebtsCalculatorTestHelper.NewOperation(3, 1, 300, MoneyOperationType.Transfer),
            };
            ReceiptCalculatorInfo[] receipts =
            {
                DebtsCalculatorTestHelper.NewReceipt(1, new long[] {1, 2, 3}, 300),
                DebtsCalculatorTestHelper.NewReceipt(1, new long[] {1, 2, 3}, 600),
                DebtsCalculatorTestHelper.NewReceipt(3, new long[] {1, 3}, 3000),
                DebtsCalculatorTestHelper.NewReceipt(2, new long[] {1, 2}, 900),
                DebtsCalculatorTestHelper.NewReceipt(2, new long[] {3, 2}, 900),
            };

            MoneyOperationShortInfo[] debts = await _calculator.Calculate(operations, receipts);

            Assert.IsNotNull(debts);
            Assert.IsTrue(debts.Length > 0);
        }

        [TestMethod]
        public async Task Test2()
        {
            MoneyOperation[] operations =
            {
                //DebtsCalculatorTestHelper.NewOperation(1, 2, 400, MoneyOperationType.Debt),
                //DebtsCalculatorTestHelper.NewOperation(3, 1, 200, MoneyOperationType.Debt),
                //DebtsCalculatorTestHelper.NewOperation(1, 2, 100, MoneyOperationType.Transfer),
                //DebtsCalculatorTestHelper.NewOperation(1, 3, 300, MoneyOperationType.Transfer),
                //DebtsCalculatorTestHelper.NewOperation(2, 3, 400, MoneyOperationType.Transfer),
            };
            ReceiptCalculatorInfo[] receipts =
            {
                DebtsCalculatorTestHelper.NewReceipt(1, new long[] {1, 3}, 300),
                DebtsCalculatorTestHelper.NewReceipt(2, new long[] {1, 2, 3}, 600),
                DebtsCalculatorTestHelper.NewReceipt(3, new long[] {1, 2, 3}, 600),
                DebtsCalculatorTestHelper.NewReceipt(1, new long[] {1, 2, 3}, 1000),
            };

            MoneyOperationShortInfo[] debts = await _calculator.Calculate(operations, receipts);

            Assert.IsNotNull(debts);
            Assert.IsTrue(debts.Length > 0);
        }
    }
}