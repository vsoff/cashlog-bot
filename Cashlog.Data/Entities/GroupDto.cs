namespace Cashlog.Data.Entities
{
    public class GroupDto : Entity
    {
        public string ChatName { get; set; }
        public string ChatToken { get; set; }
        public string AdminToken { get; set; }

        public virtual ReceiptDto[] Receipts { get; set; }
        public virtual CustomerDto[] Customers { get; set; }
        public virtual BillingPeriodDto[] BillingPeriods { get; set; }
    }
}