using Cashlog.Core.Models.Main;

namespace Cashlog.Core.Services.Abstract;

/// <summary>
///     Сервис для управления денежными операциями.
/// </summary>
public interface IMoneyOperationService
{
    /// <summary>
    ///     Добавляет новую денежную операцию.
    /// </summary>
    Task<MoneyOperationDto> AddAsync(MoneyOperationDto item);

    /// <summary>
    ///     Добавляет несколько новых денежных операций.
    /// </summary>
    Task<MoneyOperationDto[]> AddAsync(MoneyOperationDto[] items);

    /// <summary>
    ///     Возвращает все денежные операции за расчётный период.
    /// </summary>
    Task<MoneyOperationDto[]> GetByBillingPeriodIdAsync(long billingPeriodId);
}