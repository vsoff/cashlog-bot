using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cashlog.Core.Core.Models;

namespace Cashlog.Core.Core.Services.Abstract
{
    public interface IMoneyOperationService
    {
        Task<MoneyOperation> AddAsync(MoneyOperation item);
        Task<MoneyOperation[]> AddAsync(MoneyOperation[] items);

        /// <summary>
        /// Возвращает все денежные операции за расчётный период.
        /// </summary>
        Task<MoneyOperation[]> GetByBillingPeriodIdAsync(long billingPeriodId);
    }
}