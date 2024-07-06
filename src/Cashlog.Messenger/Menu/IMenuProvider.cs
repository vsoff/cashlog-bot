using Cashlog.Common;
using Cashlog.Common.Models;

namespace Cashlog.Messenger.Menu;

public interface IMenuProvider
{
    IMenu GetMenu(UserMessageInfo userMessageInfo, IQueryData data);
}