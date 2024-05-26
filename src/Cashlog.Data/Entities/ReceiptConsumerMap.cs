namespace Cashlog.Data.Entities;

public class ReceiptConsumerMap : Entity
{
    public long ReceiptId { get; set; }
    public long ConsumerId { get; set; }
    public virtual Receipt Receipt { get; set; }
    public virtual Customer Consumer { get; set; }
}