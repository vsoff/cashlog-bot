namespace Cashlog.Common;

public static class ReceiptStatusExtensions
{
    public static bool IsFinalStatus(this ReceiptStatus status)
    {
        return status == ReceiptStatus.Filled
               || status == ReceiptStatus.Manual
               || status == ReceiptStatus.WithWarning;
    }
}