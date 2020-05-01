namespace Cashlog.Common
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
}