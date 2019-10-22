using System;

namespace Cashlog.Core.Core.Models
{
    public class ReceiptInfo
    {
        public DateTime PurchaseTime { get; set; }
        public double TotalAmount { get; set; }
        public string FiscalDocument { get; set; }
        public string FiscalNumber { get; set; }
        public string FiscalSign { get; set; }
        public string RetailAddress { get; set; }
        public string RetailInn { get; set; }
        public string CompanyName { get; set; }
        public string CashierName { get; set; }
        public ReceiptItem[] Items { get; set; }
    }
}