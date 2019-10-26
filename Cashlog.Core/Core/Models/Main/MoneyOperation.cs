using System;
using System.Collections.Generic;
using System.Text;

namespace Cashlog.Core.Core.Models
{
    public class MoneyOperation
    {
        public long Id { get; set; }

        /// <summary>
        /// ID времени расчёта.
        /// </summary>
        public long BillingPeriodId { get; set; }

        /// <summary>
        /// ID покупателя который переводит деньги.
        /// </summary>
        public long CustomerFromId { get; set; }

        /// <summary>
        /// ID покупателя которому переводятся деньги.
        /// </summary>
        public long CustomerToId { get; set; }

        /// <summary>
        /// Количество переводимых у.е.
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// Тип операции с деньгами.
        /// </summary>
        public MoneyOperationType OperationType { get; set; }

        /// <summary>
        /// Комментарий операции.
        /// </summary>
        public string Comment { get; set; }
    }
}