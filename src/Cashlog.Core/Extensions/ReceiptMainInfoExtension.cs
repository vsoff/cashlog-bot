namespace Cashlog.Core.Extensions;

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