using System.Collections.Generic;

namespace Cashlog.Data.Entities
{
    public class GroupDto : Entity
    {
        public string ChatName { get; set; }
        public string ChatToken { get; set; }
        public string AdminToken { get; set; }

        public virtual ICollection<ReceiptDto> Receipts { get; set; }
        public virtual ICollection<CustomerDto> Customers { get; set; }
        public virtual ICollection<BillingPeriodDto> BillingPeriods { get; set; }
    }
}