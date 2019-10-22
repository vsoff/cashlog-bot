using System;

namespace Cashlog.Data.Entities
{
    public class ReceiptDto : Entity
    {
        public long GroupId { get; set; }
        public DateTime PurchaseTime { get; set; }
        public double TotalAmount { get; set; }
        public string FiscalDocument { get; set; }
        public string FiscalNumber { get; set; }
        public string FiscalSign { get; set; }
        public ReceiptStatusDto Status { get; set; }

        public long? CustomerId { get; set; }
        public long BillingTimeId { get; set; }

        public string RetailAddress { get; set; }
        public string RetailInn { get; set; }
        public string CompanyName { get; set; }
        public string CashierName { get; set; }
        public string ReceiptItemsJson { get; set; }

        public virtual ReceiptConsumerMapDto[] ConsumerMaps { get; set; }
        public virtual BillingTimeDto BillingTime { get; set; }
        public virtual CustomerDto Customer { get; set; }
        public virtual GroupDto Group { get; set; }
    }
}