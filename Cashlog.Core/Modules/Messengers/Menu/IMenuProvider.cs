using Cashlog.Core.Models;

namespace Cashlog.Core.Modules.Messengers.Menu
{
    public interface IMenuProvider
    {
        IMenu GetMenu(UserMessageInfo userMessageInfo, IQueryData data);
    }
}