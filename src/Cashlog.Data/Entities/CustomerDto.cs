namespace Cashlog.Data.Entities;

public class CustomerDto : Entity
{
    public long GroupId { get; set; }
    public string Caption { get; set; }
    public bool IsDeleted { get; set; }

    public virtual ICollection<ReceiptDto> CustomerReceipts { get; set; }
    public virtual ICollection<ReceiptConsumerMapDto> ConsumerReceiptMaps { get; set; }
    public virtual GroupDto Group { get; set; }
}