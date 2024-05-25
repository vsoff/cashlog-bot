namespace Cashlog.Data.Entities
{
    public class ReceiptConsumerMapDto : Entity
    {
        public long ReceiptId { get; set; }
        public long ConsumerId { get; set; }
        public virtual ReceiptDto Receipt { get; set; }
        public virtual CustomerDto Consumer { get; set; }
    }
}