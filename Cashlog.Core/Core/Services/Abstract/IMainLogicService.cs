using System.Threading.Tasks;
using Cashlog.Core.Modules.Calculator;

namespace Cashlog.Core.Core.Services.Abstract
{
    public interface IMainLogicService
    {
        /// <summary>
        /// Рассчитывает долги для расчётного периода.
        /// </summary>
        Task<MoneyOperationShortInfo[]> CalculatePeriodCurrentDebts(long billingPeriodId);

        /// <summary>
        /// Закрывает расчётный период и открывает новый.
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        Task<ClosingPeriodResult> CloseCurrentAndOpenNewPeriod(long groupId);
    }
}