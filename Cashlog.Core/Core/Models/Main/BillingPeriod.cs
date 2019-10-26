using System;
using System.Collections.Generic;
using System.Text;

namespace Cashlog.Core.Core.Models
{
    public class BillingPeriod
    {
        public long Id { get; set; }

        /// <summary>
        /// ID группы, в которой происходит расчёт.
        /// </summary>
        public long GroupId { get; set; }

        /// <summary>
        /// Время начала расчётного периода.
        /// </summary>
        public DateTime PeriodBegin { get; set; }

        /// <summary>
        /// Время окончания расчётного периода.
        /// </summary>
        public DateTime? PeriodEnd { get; set; }
    }
}