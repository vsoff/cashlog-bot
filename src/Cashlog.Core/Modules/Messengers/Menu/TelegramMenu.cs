using Telegram.Bot.Types.ReplyMarkups;

namespace Cashlog.Core.Modules.Messengers.Menu;

public class TelegramMenu : IMenu
{
    public IReplyMarkup Markup { get; set; }
    public MenuType MenuType { get; set; }
}