using System;

namespace Cashlog.Data.Entities
{
    public class BillingTimeDto : Entity
    {
        public long GroupId { get; set; }

        /// <summary>
        /// Дата расчёта.
        /// </summary>
        public DateTime BillingDate { get; set; }

        public virtual GroupDto Group { get; set; }
        public virtual ReceiptDto[] Receipts { get; set; }
        public virtual MoneyOperationDto[] MoneyOperations { get; set; }
    }
}