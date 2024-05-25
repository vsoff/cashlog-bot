using Cashlog.Common;
using Cashlog.Core.Models.Main;

namespace Cashlog.Core.Modules.Calculator;

public class DebtsCalculator : IDebtsCalculator
{
    public async Task<MoneyOperationShortInfo[]> Calculate(MoneyOperation[] operations,
        ReceiptCalculatorInfo[] receiptCalculatorInfos)
    {
        // Получаем все денежные операции.
        var allOperations = operations.Select(x => new MoneyOperationShortInfo
        {
            Amount = x.Amount,
            FromId = x.CustomerFromId,
            ToId = x.CustomerToId,
            Type = x.OperationType
        }).ToList();

        // Переводим траты на чеки в операции.
        foreach (var receipt in receiptCalculatorInfos)
        {
            var consumerIds = receipt.ConsumerIds.Where(x => x != receipt.CustomerId).ToArray();
            var part = receipt.Amount / (consumerIds.Length + 1);
            var receiptOperations = consumerIds.Select(x => new MoneyOperationShortInfo
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
            var pairOperations = debtsByPairs[operation.Key];
            var firstId = operation.Key.Item1;
            var secondId = operation.Key.Item2;
            var firstDebt = pairOperations.Where(x => x.FromId == firstId).Sum(x => x.Amount);
            var secondDebt = pairOperations.Where(x => x.FromId == secondId).Sum(x => x.Amount);

            if (Math.Abs(firstDebt - secondDebt) < 1)
                continue;

            var firstIsDebtor = firstDebt > secondDebt;
            var totalDebt = Math.Abs(firstDebt - secondDebt);

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

    private Tuple<long, long> GetToken(long fromId, long toId)
    {
        return Tuple.Create(Math.Max(fromId, toId), Math.Min(fromId, toId));
    }

    // TODO: Написать оптимизацию.
    //private static IEnumerable<MoneyOperationShortInfo> OptimizeDebts(IEnumerable<MoneyOperationShortInfo> operations)
    //{
    //    var a = new MoneyOperationShortInfo[0];
    //    return a;
    //}

    /// <summary>
    ///     Переводит операцию в тип `долг`, если она ещё не принадлежит этому типу.
    /// </summary>
    private static MoneyOperationShortInfo FixOperation(MoneyOperationShortInfo oper)
    {
        if (oper.FromId == oper.ToId)
            throw new Exception("Sender and receiver ids are equals");

        if (oper.Type != MoneyOperationType.Transfer && oper.Type != MoneyOperationType.Debt)
            throw new Exception($"Cannot {nameof(FixOperation)} with type {oper.Type}");

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