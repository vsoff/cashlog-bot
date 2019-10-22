using System;

namespace Cashlog.Core
{
    public class QrCodeData
    {
        public string Data { get; set; }
        public string FiscalNumber { get; set; }
        public string FiscalDocument { get; set; }
        public string FiscalSign { get; set; }
        public DateTime PurchaseTime { get; set; }
        public double TotalAmount { get; set; }
    }
}