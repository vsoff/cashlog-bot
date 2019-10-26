using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cashlog.Core.Core.Models;
using Cashlog.Core.Core.Services;

namespace Cashlog.Core.Modules.Calculator
{
    public class DebtsCalculator : IDebtsCalculator
    {
        private Tuple<long, long> GetToken(long fromId, long toId) => Tuple.Create(Math.Max(fromId, toId), Math.Min(fromId, toId));

        public async Task<MoneyOperationShortInfo[]> Calculate(MoneyOperation[] operations, ReceiptCalculatorInfo[] receiptCalculatorInfos)
        {
            // Получаем все денежные операции.
            List<MoneyOperationShortInfo> allOperations = operations.Select(x => new MoneyOperationShortInfo
            {
                Amount = x.Amount,
                FromId = x.CustomerFromId,
                ToId = x.CustomerToId,
                Type = x.OperationType
            }).ToList();

            // Переводим траты на чеки в операции.
            foreach (var receipt in receiptCalculatorInfos)
            {
                double part = receipt.Amount / (receipt.ConsumerIds.Length + 1);
                var receiptOperations = receipt.ConsumerIds.Where(x => x != receipt.CustomerId).Select(x => new MoneyOperationShortInfo
                {
                    Amount = part,
                    FromId = x,
                    ToId = receipt.CustomerId,
                    Type = MoneyOperationType.Debt
                });
                allOperations.AddRange(receiptOperations);
            }

            // Группируем операции
            var debtsByPairs = allOperations.Where(x => x.ToId != x.FromId)
                .GroupBy(x => GetToken(x.FromId, x.ToId))
                .ToDictionary(x => x.Key, x => x.Select(FixOperation).ToArray());

            // Получаем долги.
            var result = new List<MoneyOperationShortInfo>();
            foreach (var operation in debtsByPairs)
            {
                MoneyOperationShortInfo[] pairOperations = debtsByPairs[operation.Key];
                long firstId = operation.Key.Item1;
                long secondId = operation.Key.Item2;
                double firstDebt = pairOperations.Where(x => x.FromId == firstId).Sum(x => x.Amount);
                double secondDebt = pairOperations.Where(x => x.FromId == secondId).Sum(x => x.Amount);

                if (Math.Abs(firstDebt - secondDebt) < 1)
                    continue;

                bool firstIsDebtor = firstDebt > secondDebt;
                double totalDebt = Math.Abs(firstDebt - secondDebt);

                result.Add(new MoneyOperationShortInfo
                {
                    Amount = totalDebt,
                    FromId = firstIsDebtor ? firstId : secondId,
                    ToId = firstIsDebtor ? secondId : firstId,
                    Type = MoneyOperationType.Debt
                });
            }

            return result.ToArray();
        }

        /// <summary>
        /// Переводит операцию в тип `долг`, если она ещё не принадлежит этому типу.
        /// </summary>
        private MoneyOperationShortInfo FixOperation(MoneyOperationShortInfo oper)
        {
            if (oper.FromId == oper.ToId)
                throw new Exception($"{GetType().Name} sender and receiver ids are equals");

            if (oper.Type != MoneyOperationType.Transfer && oper.Type != MoneyOperationType.Debt)
                throw new Exception($"{GetType().Name} cannot {nameof(FixOperation)} with type {oper.Type}");

            if (oper.Type == MoneyOperationType.Debt)
                return oper;

            return new MoneyOperationShortInfo
            {
                Amount = oper.Amount,
                FromId = oper.ToId,
                ToId = oper.FromId,
                Type = MoneyOperationType.Debt
            };
        }
    }
}