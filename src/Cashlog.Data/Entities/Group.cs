namespace Cashlog.Data.Entities;

public class Group : Entity
{
    public string ChatName { get; set; }
    public string ChatToken { get; set; }
    public string AdminToken { get; set; }

    public virtual ICollection<Receipt> Receipts { get; set; }
    public virtual ICollection<Customer> Customers { get; set; }
    public virtual ICollection<BillingPeriod> BillingPeriods { get; set; }
}