using System;
using System.Collections.Generic;
using System.Text;

namespace Cashlog.Core.Core.Extensions
{
    public static class ReceiptMainInfoExtension
    {
        public static bool IsValid(this ReceiptMainInfo receiptInfo)
        {
            return !(string.IsNullOrEmpty(receiptInfo.FiscalDocument)
                     || string.IsNullOrEmpty(receiptInfo.FiscalNumber)
                     || string.IsNullOrEmpty(receiptInfo.FiscalSign)
                     || receiptInfo.PurchaseTime.Equals(DateTime.MinValue));
        }
    }
}