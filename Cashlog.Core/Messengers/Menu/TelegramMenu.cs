using Telegram.Bot.Types.ReplyMarkups;

namespace Cashlog.Core.Messengers.Menu
{
    public class TelegramMenu : IMenu
    {
        public MenuType MenuType { get; set; }
        public IReplyMarkup Markup { get; set; }
    }
}