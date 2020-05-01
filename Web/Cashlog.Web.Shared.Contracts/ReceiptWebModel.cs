using Cashlog.Common;
using System;

namespace Cashlog.Web.Shared.Contracts
{
    public class ReceiptWebModel : IWebModel
    {
        public long Id { get; set; }
        public string Comment { get; set; }
        public DateTime PurchaseTime { get; set; }
        public double TotalAmount { get; set; }
        public string FiscalDocument { get; set; }
        public string FiscalNumber { get; set; }
        public string FiscalSign { get; set; }
        public ReceiptStatus Status { get; set; }

        public long? CustomerId { get; set; }
        public long BillingPeriodId { get; set; }

        public string RetailAddress { get; set; }
        public string RetailInn { get; set; }
        public string CompanyName { get; set; }
        public string CashierName { get; set; }
        public ReceiptItemWebModel[] Items { get; set; }
    }
}