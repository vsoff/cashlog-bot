namespace Cashlog.Data.Entities
{
    public enum ReceiptStatusDto : byte
    {
        Undefined = 0,
        New = 1,
        Filled = 2,
        WithWarning = 3,
        Manual = 4,
        Deleted = 5
    }
}