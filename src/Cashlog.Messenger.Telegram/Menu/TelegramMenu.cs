using Cashlog.Common;
using Cashlog.Messenger.Menu;
using Telegram.Bot.Types.ReplyMarkups;

namespace Cashlog.Messenger.Telegram.Menu;

internal sealed class TelegramMenu : IMenu
{
    public IReplyMarkup Markup { get; set; }
    public MenuType MenuType { get; set; }
}