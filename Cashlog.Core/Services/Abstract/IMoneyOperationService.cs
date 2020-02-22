using System.Threading.Tasks;
using Cashlog.Core.Models.Main;

namespace Cashlog.Core.Services.Abstract
{
    /// <summary>
    /// Сервис для управления денежными операциями.
    /// </summary>
    public interface IMoneyOperationService
    {
        /// <summary>
        /// Добавляет новую денежную операцию.
        /// </summary>
        Task<MoneyOperation> AddAsync(MoneyOperation item);

        /// <summary>
        /// Добавляет несколько новых денежных операций.
        /// </summary>
        Task<MoneyOperation[]> AddAsync(MoneyOperation[] items);

        /// <summary>
        /// Возвращает все денежные операции за расчётный период.
        /// </summary>
        Task<MoneyOperation[]> GetByBillingPeriodIdAsync(long billingPeriodId);
    }
}