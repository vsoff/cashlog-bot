using System.Text;

namespace Cashlog.Core.Messengers.Menu
{
    public interface IMenu
    {
        MenuType MenuType { get; }
    }
}