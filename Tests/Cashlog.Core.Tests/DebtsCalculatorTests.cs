using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cashlog.Core.Core.Models;
using Cashlog.Core.Core.Services;
using Cashlog.Core.Messengers.Menu;
using Cashlog.Core.Modules.Calculator;
using NUnit.Framework;

namespace Cashlog.Core.Tests
{
    public class DebtsCalculatorTests
    {
        private IDebtsCalculator _calculator;

        [SetUp]
        public void Setup()
        {
            _calculator = new DebtsCalculator();
        }

        [Test]
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

            Assert.Pass();
        }

        [Test]
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

            Assert.Pass();
        }
    }
}