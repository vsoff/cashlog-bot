using Cashlog.Data.Entities;

namespace Cashlog.Data.Entities
{
    public class CustomerDto : Entity
    {
        public long GroupId { get; set; }
        public string Caption { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ReceiptDto[] CustomerReceipts { get; set; }
        public virtual ReceiptConsumerMapDto[] ConsumerReceiptMaps { get; set; }
        public virtual GroupDto Group { get; set; }
    }
}