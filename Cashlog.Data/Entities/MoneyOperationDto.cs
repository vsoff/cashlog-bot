namespace Cashlog.Data.Entities
{
    public class MoneyOperationDto : Entity
    {
        /// <summary>
        /// ID времени расчёта.
        /// </summary>
        public long BillingTimeId { get; set; }

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

        public virtual BillingTimeDto BillingTime { get; set; }
        public virtual CustomerDto CustomerFrom { get; set; }
        public virtual CustomerDto CustomerTo { get; set; }
    }
}