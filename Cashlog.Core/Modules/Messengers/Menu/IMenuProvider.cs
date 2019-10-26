using Cashlog.Core.Core.Models;

namespace Cashlog.Core.Messengers.Menu
{
    public interface IMenuProvider
    {
        IMenu GetMenu(UserMessageInfo userMessageInfo, IQueryData data);
    }
}