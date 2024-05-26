namespace Cashlog.Data.Entities;

public class Customer : Entity
{
    public long GroupId { get; set; }
    public string Caption { get; set; }
    public bool IsDeleted { get; set; }

    public virtual ICollection<Receipt> CustomerReceipts { get; set; }
    public virtual ICollection<ReceiptConsumerMap> ConsumerReceiptMaps { get; set; }
    public virtual Group Group { get; set; }
}