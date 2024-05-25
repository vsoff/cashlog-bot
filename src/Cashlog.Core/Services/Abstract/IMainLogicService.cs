using Cashlog.Core.Modules.Calculator;

namespace Cashlog.Core.Services.Abstract;

/// <summary>
///     Сервис основной логики проекта.
/// </summary>
public interface IMainLogicService
{
    /// <summary>
    ///     Рассчитывает долги для расчётного периода.
    /// </summary>
    Task<MoneyOperationShortInfo[]> CalculatePeriodCurrentDebts(long billingPeriodId);

    /// <summary>
    ///     Закрывает расчётный период и открывает новый.
    /// </summary>
    Task<ClosingPeriodResult> CloseCurrentAndOpenNewPeriod(long groupId);
}