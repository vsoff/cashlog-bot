﻿using Telegram.Bot.Types.ReplyMarkups;

namespace Cashlog.Core.Modules.Messengers.Menu
{
    public class TelegramMenu : IMenu
    {
        public MenuType MenuType { get; set; }
        public IReplyMarkup Markup { get; set; }
    }
}