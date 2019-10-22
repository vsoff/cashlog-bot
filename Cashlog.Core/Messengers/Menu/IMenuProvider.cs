using Cashlog.Core.Core.Models;

namespace Cashlog.Core.Messengers.Menu
{
    public interface IMenuProvider
    {
        IMenu BuildSelectCustomerMenu(UserMessageInfo userMessageInfo, long receiptId);
        IMenu BuildSelectCustomerMenu(UserMessageInfo userMessageInfo, AddReceiptQueryData data);
        IMenu BuildSelectConsumersMenu(UserMessageInfo userMessageInfo, AddReceiptQueryData data);
    }
}