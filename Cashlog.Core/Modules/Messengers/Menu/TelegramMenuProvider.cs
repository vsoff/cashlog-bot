using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cashlog.Core.Core.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace Cashlog.Core.Messengers.Menu
{
    public class TelegramMenuProvider : IMenuProvider
    {
        private readonly IQueryDataSerializer _queryDataSerializer;

        public TelegramMenuProvider(IQueryDataSerializer queryDataSerializer)
        {
            _queryDataSerializer = queryDataSerializer ?? throw new ArgumentNullException(nameof(queryDataSerializer));
        }

        public IMenu GetMenu(UserMessageInfo userMessageInfo, IQueryData data)
        {
            // Todo Проверка №2. Надо перенести в одно место.
            switch (data.MenuType)
            {
                case MenuType.NewReceiptSelectCustomer: return GetNewReceiptSelectCustomerMenu(userMessageInfo, (AddReceiptQueryData) data);
                case MenuType.NewReceiptSelectConsumers: return GetNewReceiptSelectConsumersMenu(userMessageInfo, (AddReceiptQueryData) data);
                case MenuType.MoneyTransferSelectFrom: return GetMoneyTransferSelectFromMenu(userMessageInfo, (MoneyTransferQueryData) data);
                case MenuType.MoneyTransferSelectTo: return GetMoneyTransferSelectToMenu(userMessageInfo, (MoneyTransferQueryData) data);
            }

            return null;
        }

        private IMenu GetNewReceiptSelectCustomerMenu(UserMessageInfo userMessageInfo, AddReceiptQueryData data)
        {
            Builder builder = new Builder();

            for (int i = 0; i < userMessageInfo.Customers.Length; i++)
            {
                if (i % 3 == 0)
                    builder.AddLine();

                var customer = userMessageInfo.Customers[i];
                string query = _queryDataSerializer.EncodeBase64(new AddReceiptQueryData
                {
                    Version = AddReceiptQueryData.CurrentServerVersion,
                    ChatToken = userMessageInfo.Group.ChatToken,
                    ReceiptId = data.ReceiptId,
                    MenuType = MenuType.NewReceiptSelectCustomer,
                    SelectedConsumerIds = data.SelectedConsumerIds,
                    SelectedCustomerId = data.TargetId,
                    TargetId = customer.Id,
                }, MenuType.NewReceiptSelectCustomer);

                builder.AddButton(customer.Caption + (customer.Id == data.TargetId ? " ☑" : ""), query);
            }

            builder.AddLine();

            if (data.TargetId.HasValue)
            {
                builder.AddButton("Применить", _queryDataSerializer.EncodeBase64(new AddReceiptQueryData
                {
                    Version = AddReceiptQueryData.CurrentServerVersion,
                    ChatToken = userMessageInfo.Group.ChatToken,
                    ReceiptId = data.ReceiptId,
                    MenuType = MenuType.NewReceiptSelectConsumers,
                    SelectedConsumerIds = data.SelectedConsumerIds,
                    SelectedCustomerId = data.SelectedCustomerId.Value,
                    TargetId = null,
                }, MenuType.NewReceiptSelectCustomer));
            }

            builder.AddButton("Отменить", _queryDataSerializer.EncodeBase64(new AddReceiptQueryData
            {
                Version = AddReceiptQueryData.CurrentServerVersion,
                ChatToken = userMessageInfo.Group.ChatToken,
                ReceiptId = data.ReceiptId,
                MenuType = MenuType.NewReceiptCancel
            }, MenuType.NewReceiptSelectCustomer));

            var markUp = builder.Build();

            return new TelegramMenu
            {
                MenuType = MenuType.NewReceiptSelectCustomer,
                Markup = markUp,
            };
        }

        private IMenu GetNewReceiptSelectConsumersMenu(UserMessageInfo userMessageInfo, AddReceiptQueryData data)
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

                string query = _queryDataSerializer.EncodeBase64(new AddReceiptQueryData
                {
                    Version = AddReceiptQueryData.CurrentServerVersion,
                    ChatToken = userMessageInfo.Group.ChatToken,
                    ReceiptId = data.ReceiptId,
                    MenuType = data.MenuType,
                    SelectedConsumerIds = data.SelectedConsumerIds.ToArray(),
                    SelectedCustomerId = data.SelectedCustomerId,
                    TargetId = customer.Id,
                }, MenuType.NewReceiptSelectConsumers);

                builder.AddButton(customer.Caption + (data.SelectedConsumerIds.Contains(customer.Id) ? " ☑" : ""), query);
            }

            builder.AddLine();

            if (data.SelectedConsumerIds.Any())
            {
                builder.AddButton("Применить", _queryDataSerializer.EncodeBase64(new AddReceiptQueryData
                {
                    Version = AddReceiptQueryData.CurrentServerVersion,
                    ChatToken = userMessageInfo.Group.ChatToken,
                    ReceiptId = data.ReceiptId,
                    MenuType = MenuType.NewReceiptAdd,
                    SelectedConsumerIds = data.SelectedConsumerIds,
                    SelectedCustomerId = data.SelectedCustomerId.Value,
                    TargetId = null,
                }, MenuType.NewReceiptSelectConsumers));
            }

            builder.AddButton("Отменить", _queryDataSerializer.EncodeBase64(new AddReceiptQueryData
            {
                Version = AddReceiptQueryData.CurrentServerVersion,
                ChatToken = userMessageInfo.Group.ChatToken,
                ReceiptId = data.ReceiptId,
                MenuType = MenuType.NewReceiptCancel
            }, MenuType.NewReceiptSelectConsumers));

            var markUp = builder.Build();

            return new TelegramMenu
            {
                MenuType = MenuType.NewReceiptSelectCustomer,
                Markup = markUp,
            };
        }

        private IMenu GetMoneyTransferSelectFromMenu(UserMessageInfo userMessageInfo, MoneyTransferQueryData data)
        {
            Builder builder = new Builder();

            for (int i = 0; i < userMessageInfo.Customers.Length; i++)
            {
                if (i % 3 == 0)
                    builder.AddLine();

                var customer = userMessageInfo.Customers[i];

                string query = _queryDataSerializer.EncodeBase64(new MoneyTransferQueryData
                {
                    Version = MoneyTransferQueryData.CurrentServerVersion,
                    Amount = data.Amount,
                    Caption = data.Caption,
                    ChatToken = data.ChatToken,
                    CustomerFromId = null,
                    CustomerToId = null,
                    TargetId = customer.Id,
                    MenuType = MenuType.MoneyTransferSelectFrom
                }, MenuType.MoneyTransferSelectFrom);
                
                builder.AddButton(customer.Caption + (customer.Id == data.TargetId ? " ☑" : ""), query);
            }

            builder.AddLine();

            if (data.TargetId.HasValue)
            {
                builder.AddButton("Применить", _queryDataSerializer.EncodeBase64(new MoneyTransferQueryData
                {
                    Version = MoneyTransferQueryData.CurrentServerVersion,
                    Amount = data.Amount,
                    Caption = data.Caption,
                    ChatToken = data.ChatToken,
                    CustomerFromId = data.TargetId,
                    CustomerToId = null,
                    TargetId = null,
                    MenuType = MenuType.MoneyTransferSelectTo
                }, MenuType.MoneyTransferSelectFrom));
            }

            builder.AddButton("Отменить", _queryDataSerializer.EncodeBase64(new MoneyTransferQueryData
            {
                Version = MoneyTransferQueryData.CurrentServerVersion,
                ChatToken = data.ChatToken,
                MenuType = MenuType.MoneyTransferCancel
            }, MenuType.MoneyTransferSelectFrom));

            var markUp = builder.Build();

            return new TelegramMenu
            {
                MenuType = MenuType.MoneyTransferSelectFrom,
                Markup = markUp,
            };
        }

        private IMenu GetMoneyTransferSelectToMenu(UserMessageInfo userMessageInfo, MoneyTransferQueryData data)
        {
            Builder builder = new Builder();

            for (int i = 0; i < userMessageInfo.Customers.Length; i++)
            {
                if (i % 3 == 0)
                    builder.AddLine();

                var customer = userMessageInfo.Customers[i];

                // Не отображаем того, кто посылал.
                if (customer.Id == data.CustomerFromId)
                    continue;

                string query = _queryDataSerializer.EncodeBase64(new MoneyTransferQueryData
                {
                    Version = MoneyTransferQueryData.CurrentServerVersion,
                    Amount = data.Amount,
                    Caption = data.Caption,
                    ChatToken = data.ChatToken,
                    CustomerFromId = data.CustomerFromId,
                    CustomerToId = null,
                    TargetId = customer.Id,
                    MenuType = MenuType.MoneyTransferSelectTo
                }, MenuType.MoneyTransferSelectTo);

                builder.AddButton(customer.Caption + (customer.Id == data.TargetId ? " ☑" : ""), query);
            }

            builder.AddLine();

            if (data.TargetId.HasValue)
            {
                builder.AddButton("Применить", _queryDataSerializer.EncodeBase64(new MoneyTransferQueryData
                {
                    Version = MoneyTransferQueryData.CurrentServerVersion,
                    Amount = data.Amount,
                    Caption = data.Caption,
                    ChatToken = data.ChatToken,
                    CustomerFromId = data.CustomerFromId,
                    CustomerToId = data.TargetId,
                    TargetId = null,
                    MenuType = MenuType.MoneyTransferAdd
                }, MenuType.MoneyTransferSelectTo));
            }

            builder.AddButton("Отменить", _queryDataSerializer.EncodeBase64(new MoneyTransferQueryData
            {
                Version = MoneyTransferQueryData.CurrentServerVersion,
                ChatToken = data.ChatToken,
                MenuType = MenuType.MoneyTransferCancel
            }, MenuType.MoneyTransferSelectTo));

            var markUp = builder.Build();

            return new TelegramMenu
            {
                MenuType = MenuType.MoneyTransferSelectTo,
                Markup = markUp,
            };
        }

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
    }
}