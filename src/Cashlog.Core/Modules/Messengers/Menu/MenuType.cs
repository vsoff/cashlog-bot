namespace Cashlog.Core.Modules.Messengers.Menu;

public enum MenuType
{
    None = 0,

    NewReceiptSelectCustomer = 1,
    NewReceiptSelectConsumers = 2,
    NewReceiptAdd = 3,
    NewReceiptCancel = 4,

    MoneyTransferSelectFrom = 5,
    MoneyTransferSelectTo = 6,
    MoneyTransferAdd = 7,
    MoneyTransferCancel = 8
}