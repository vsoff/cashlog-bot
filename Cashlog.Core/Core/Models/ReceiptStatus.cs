namespace Cashlog.Core.Core.Models
{
    public enum ReceiptStatus : byte
    {
        Undefined = 0,
        New = 1,
        NewManual = 1,
        Filled = 2,
        WithWarning = 3,
        Manual = 4,
        Deleted = 5
    }

    public static class ReceiptStatusExtensions
    {
        public static bool IsFinalStatus(this ReceiptStatus status)
        {
            return status == ReceiptStatus.Filled
                   || status == ReceiptStatus.Manual
                   || status == ReceiptStatus.WithWarning;
        }
    }
}