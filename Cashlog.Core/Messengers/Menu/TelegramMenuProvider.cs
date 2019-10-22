using System.Collections.Generic;
using System.Linq;
using Cashlog.Core.Core.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace Cashlog.Core.Messengers.Menu
{
    public class TelegramMenuProvider : IMenuProvider
    {
        private class Builder
        {
            private readonly List<List<InlineKeyboardButton>> _markup;

            public Builder()
            {
                _markup = new List<List<InlineKeyboardButton>>();
            }

            public Builder AddButton(string text, string query)
            {
                if (_markup.Count == 0)
                    AddLine();

                _markup.Last().Add(new InlineKeyboardButton
                {
                    CallbackData = query,
                    Text = text,
                });

                return this;
            }

            public Builder AddLine()
            {
                _markup.Add(new List<InlineKeyboardButton>());
                return this;
            }

            public InlineKeyboardMarkup Build() => new InlineKeyboardMarkup(_markup.Select(x => x.ToArray()).Where(x => x.Length > 0).ToArray());
        }

        public IMenu BuildSelectCustomerMenu(UserMessageInfo userMessageInfo, AddReceiptQueryData data)
        {
            Builder builder = new Builder();

            for (int i = 0; i < userMessageInfo.Customers.Length; i++)
            {
                if (i % 3 == 0)
                    builder.AddLine();

                var customer = userMessageInfo.Customers[i];
                string query = new AddReceiptQueryData
                {
                    Version = AddReceiptQueryData.CurrentServerVersion,
                    ChatToken = userMessageInfo.Group.ChatToken,
                    ReceiptId = data.ReceiptId,
                    MenuType = data.MenuType,
                    SelectedConsumerIds = data.SelectedConsumerIds,
                    SelectedCustomerId = data.TargetId,
                    TargetId = customer.Id,
                    IsApply = false
                }.ToQuery();

                builder.AddButton(customer.Caption + (customer.Id == data.TargetId ? " ☑" : ""), query);
            }

            builder.AddLine();

            if (data.TargetId.HasValue)
            {
                builder.AddButton("Применить", new AddReceiptQueryData
                {
                    Version = AddReceiptQueryData.CurrentServerVersion,
                    ChatToken = userMessageInfo.Group.ChatToken,
                    ReceiptId = data.ReceiptId,
                    MenuType = MenuType.SelectConsumers,
                    SelectedConsumerIds = data.SelectedConsumerIds,
                    SelectedCustomerId = null,
                    TargetId = null,
                    IsApply = true
                }.ToQuery());
            }

            builder.AddButton("Отменить", new AddReceiptQueryData
            {
                Version = AddReceiptQueryData.CurrentServerVersion,
                ChatToken = userMessageInfo.Group.ChatToken,
                ReceiptId = data.ReceiptId,
                MenuType = MenuType.CancelReceipt
            }.ToQuery());

            var markUp = builder.Build();

            return new TelegramMenu
            {
                MenuType = MenuType.SelectCustomer,
                Markup = markUp,
            };
        }

        public IMenu BuildSelectConsumersMenu(UserMessageInfo userMessageInfo, AddReceiptQueryData data)
        {
            Builder builder = new Builder();

            for (int i = 0; i < userMessageInfo.Customers.Length; i++)
            {
                if (i % 3 == 0)
                    builder.AddLine();

                var customer = userMessageInfo.Customers[i];

                // Не отображаем того, кто покупал.
                if (customer.Id == data.SelectedCustomerId)
                    continue;

                string query = new AddReceiptQueryData
                {
                    Version = AddReceiptQueryData.CurrentServerVersion,
                    ChatToken = userMessageInfo.Group.ChatToken,
                    ReceiptId = data.ReceiptId,
                    MenuType = data.MenuType,
                    SelectedConsumerIds = data.SelectedConsumerIds.ToArray(),
                    SelectedCustomerId = data.SelectedCustomerId,
                    TargetId = customer.Id,
                    IsApply = false
                }.ToQuery();

                builder.AddButton(customer.Caption + (data.SelectedConsumerIds.Contains(customer.Id) ? " ☑" : ""), query);
            }

            builder.AddLine();

            if (data.SelectedConsumerIds.Any())
            {
                builder.AddButton("Применить", new AddReceiptQueryData
                {
                    Version = AddReceiptQueryData.CurrentServerVersion,
                    ChatToken = userMessageInfo.Group.ChatToken,
                    ReceiptId = data.ReceiptId,
                    MenuType = MenuType.AddReceipt,
                    SelectedConsumerIds = data.SelectedConsumerIds,
                    SelectedCustomerId = data.SelectedCustomerId,
                    TargetId = null,
                    IsApply = true
                }.ToQuery());
            }

            builder.AddButton("Отменить", new AddReceiptQueryData
            {
                Version = AddReceiptQueryData.CurrentServerVersion,
                ChatToken = userMessageInfo.Group.ChatToken,
                ReceiptId = data.ReceiptId,
                MenuType = MenuType.CancelReceipt
            }.ToQuery());

            var markUp = builder.Build();

            return new TelegramMenu
            {
                MenuType = MenuType.SelectCustomer,
                Markup = markUp,
            };
        }

        public IMenu BuildSelectCustomerMenu(UserMessageInfo userMessageInfo, long receiptId)
        {
            return BuildSelectCustomerMenu(userMessageInfo, new AddReceiptQueryData
            {
                MenuType = MenuType.SelectCustomer,
                ReceiptId = receiptId,
                SelectedCustomerId = null,
                SelectedConsumerIds = new long[0],
                TargetId = null,
                Version = AddReceiptQueryData.CurrentServerVersion,
                IsApply = false
            });
        }
    }
}