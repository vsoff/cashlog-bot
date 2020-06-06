using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cashlog.Common;
using Cashlog.Core.Common;
using Cashlog.Core.Extensions;
using Cashlog.Core.Models;
using Cashlog.Core.Models.Main;
using Cashlog.Core.Modules.Calculator;
using Cashlog.Core.Modules.Messengers;
using Cashlog.Core.Modules.Messengers.Menu;
using Cashlog.Core.Services.Abstract;

namespace Cashlog.Core.Modules.MessageHandlers
{
    public class MessagesMainHandler
    {
        private readonly IReadOnlyDictionary<MessageType, IMessageHandler[]> _messageHandlersMap;
        private readonly IMoneyOperationService _moneyOperationService;
        private readonly IBillingPeriodService _billingPeriodService;
        private readonly IReceiptService _receiptService;
        private readonly IMenuProvider _menuProvider;
        private readonly IMessenger _messenger;
        private readonly ILogger _logger;

        public MessagesMainHandler(
            IMessageHandler[] messageHandlers,
            IMoneyOperationService moneyOperationService,
            IBillingPeriodService billingPeriodService,
            IReceiptService receiptService,
            IMenuProvider menuProvider,
            IMessenger messenger,
            ILogger logger)
        {
            if (messageHandlers == null) throw new ArgumentNullException(nameof(messageHandlers));
            _moneyOperationService = moneyOperationService ?? throw new ArgumentNullException(nameof(moneyOperationService));
            _billingPeriodService = billingPeriodService ?? throw new ArgumentNullException(nameof(billingPeriodService));
            _receiptService = receiptService ?? throw new ArgumentNullException(nameof(receiptService));
            _menuProvider = menuProvider ?? throw new ArgumentNullException(nameof(menuProvider));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _messenger.OnMessage += OnMessage;

            _messageHandlersMap = messageHandlers
                .GroupBy(x => x.MessageType)
                .ToDictionary(x => x.Key, x => x.ToArray());
        }

        private async void OnMessage(object sender, UserMessageInfo e)
        {
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                _logger.Trace($"Получено сообщение типа {e.MessageType} от {e.UserToken} из группы {e.Group.ChatToken}");
                // TODO: отрефачить проверку.
                switch (e.MessageType)
                {
                    case MessageType.Text:
                        await HandleTextMessageAsync(e);
                        break;
                    case MessageType.QrCode:
                        await HandlePhotoMessageAsync(e);
                        break;
                    case MessageType.Query:
                        await HandleCommandMessageAsync(e);
                        break;
                    default:
                        await _messenger.SendMessageAsync(e, "Неподдерживаемый тип сообщения", true);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"{GetType().Name}: Во время обработки сообщения произошла ошибка", ex);
            }
            finally
            {
                _logger.Trace($"Обработка сообщения для @{e.UserToken} завершена за {sw.Elapsed}");
            }
        }

        private async Task HandleCommandMessageAsync(UserMessageInfo userMessageInfo)
        {
            // Todo Проверка №3. Надо перенести в одно место.
            switch (userMessageInfo.Message.QueryData.MenuType)
            {
                case MenuType.NewReceiptSelectCustomer:
                {
                    var data = (AddReceiptQueryData) userMessageInfo.Message.QueryData;
                    if (data.TargetId == data.SelectedCustomerId)
                        break;

                    data.SelectedCustomerId = data.TargetId;

                    IMenu menu = _menuProvider.GetMenu(userMessageInfo, data);
                    await _messenger.EditMessageAsync(userMessageInfo, "Кто оплатил чек?", menu);
                    break;
                }
                case MenuType.NewReceiptSelectConsumers:
                {
                    var data = (AddReceiptQueryData) userMessageInfo.Message.QueryData;
                    var selectedConsumers = new List<long>(data.SelectedConsumerIds ?? new long[0]);
                    if (data.TargetId != null)
                    {
                        if (selectedConsumers.Contains(data.TargetId.Value))
                            selectedConsumers.Remove(data.TargetId.Value);
                        else
                            selectedConsumers.Add(data.TargetId.Value);
                    }

                    selectedConsumers = selectedConsumers.Distinct().ToList();
                    data.SelectedConsumerIds = selectedConsumers.ToArray();

                    IMenu menu = _menuProvider.GetMenu(userMessageInfo, data);
                    await _messenger.EditMessageAsync(userMessageInfo, "На кого делится чек?", menu);
                    break;
                }
                case MenuType.NewReceiptAdd:
                {
                    var data = (AddReceiptQueryData) userMessageInfo.Message.QueryData;
                    string customerText = userMessageInfo.Customers.FirstOrDefault(x => data.SelectedCustomerId.Value == x.Id).Caption;
                    string[] consumerTexts = userMessageInfo.Customers
                        .Where(x => data.SelectedCustomerId.Value == x.Id || data.SelectedConsumerIds.Contains(x.Id))
                        .Select(x => x.Caption).ToArray();
                    Receipt receipt = await _receiptService.GetAsync(data.ReceiptId);
                    await _messenger.EditMessageAsync(userMessageInfo, $"Обработка...");
                    if (receipt.Status == ReceiptStatus.New || receipt.Status == ReceiptStatus.NewManual)
                    {
                        receipt.Status = receipt.Status == ReceiptStatus.New ? ReceiptStatus.Filled : ReceiptStatus.Manual;
                        receipt.CustomerId = data.SelectedCustomerId;
                        receipt.PurchaseTime = DateTime.Now;
                        await _receiptService.SetCustomersToReceiptAsync(receipt.Id, data.SelectedConsumerIds);
                        await _receiptService.UpdateAsync(receipt);

                        await _messenger.EditMessageAsync(userMessageInfo, $"Чек готов!\nОплатил: {customerText}\nСумма: {receipt.TotalAmount} руб.\nДелится на: {string.Join(", ", consumerTexts)}");
                        _logger.Info($"Добавлен новый чек. Оплатил: {customerText}; Сумма: {receipt.TotalAmount} руб.; Делится на: {string.Join(", ", consumerTexts)}");
                    }

                    break;
                }
                case MenuType.NewReceiptCancel:
                {
                    var data = (AddReceiptQueryData) userMessageInfo.Message.QueryData;
                    await _messenger.EditMessageAsync(userMessageInfo, "Добавление чека было отменено");
                    Receipt receipt = await _receiptService.GetAsync(data.ReceiptId);
                    receipt.Status = ReceiptStatus.Deleted;
                    await _receiptService.UpdateAsync(receipt);
                    break;
                }
                case MenuType.MoneyTransferSelectFrom:
                {
                    var data = (MoneyTransferQueryData) userMessageInfo.Message.QueryData;
                    IMenu menu = _menuProvider.GetMenu(userMessageInfo, data);
                    await _messenger.EditMessageAsync(userMessageInfo, "Кто переводит?", menu);
                    break;
                }
                case MenuType.MoneyTransferSelectTo:
                {
                    var data = (MoneyTransferQueryData) userMessageInfo.Message.QueryData;
                    IMenu menu = _menuProvider.GetMenu(userMessageInfo, data);
                    await _messenger.EditMessageAsync(userMessageInfo, "Кому переводит?", menu);
                    break;
                }
                case MenuType.MoneyTransferAdd:
                {
                    var data = (MoneyTransferQueryData) userMessageInfo.Message.QueryData;
                    string customerFrom = userMessageInfo.Customers.FirstOrDefault(x => data.CustomerFromId == x.Id)?.Caption ?? $"Id: {data.CustomerFromId}";
                    string customerTo = userMessageInfo.Customers.FirstOrDefault(x => data.CustomerToId == x.Id)?.Caption ?? $"Id: {data.CustomerToId}";
                    await _messenger.EditMessageAsync(userMessageInfo, $"Обработка...");

                    var lastBillingPeriod = await _billingPeriodService.GetLastByGroupIdAsync(userMessageInfo.Group.Id);
                    if (lastBillingPeriod == null)
                    {
                        await _messenger.EditMessageAsync(userMessageInfo, "Нельзя добавить перевод денег, если не начат расчётный период");
                        break;
                    }

                    await _moneyOperationService.AddAsync(new MoneyOperation
                    {
                        Amount = data.Amount,
                        Comment = data.Caption,
                        CustomerFromId = data.CustomerFromId.Value,
                        CustomerToId = data.CustomerToId.Value,
                        OperationType = MoneyOperationType.Transfer,
                        BillingPeriodId = lastBillingPeriod.Id
                    });
                    await _messenger.EditMessageAsync(userMessageInfo, $"Добавление перевода было успешно совершено.\nПеревёл: {customerFrom}\nКому: {customerTo}\nСумма: {data.Amount} руб.");
                    _logger.Info($"Был добавлен новый перевод. Перевёл: {customerFrom}; Кому: {customerTo}; Сумма: {data.Amount} руб.");
                    break;
                }
                case MenuType.MoneyTransferCancel:
                {
                    await _messenger.EditMessageAsync(userMessageInfo, "Добавление перевода было отменено");
                    break;
                }
            }
        }

        private async Task HandleTextMessageAsync(UserMessageInfo userMessageInfo)
        {
            // TODO Вынести проверку на верхний уровень.
            if (!_messageHandlersMap.ContainsKey(MessageType.Text))
            {
                _logger.Warning("Не зарегистрировано ни одного обработчика текстовых команд.");
                return;
            }

            _logger.Trace($"@{userMessageInfo.UserName} прислал сообщение. Длина сообщения: {userMessageInfo.Message.Text.Length}");

            // Читаем только команды.
            string text = userMessageInfo.Message.Text;
            if (!text.StartsWith("/"))
            {
                await _messenger.SendMessageAsync(userMessageInfo, "Я не читаю сообщения, попробуй прислать мне фото или используй команду /help", true);
                return;
            }

            var command = text.GetCommand(out _);
            var handler = _messageHandlersMap[MessageType.Text]
                .OfType<TextCommandMessageHandler>()
                .FirstOrDefault(x => x.Command.Equals(command, StringComparison.OrdinalIgnoreCase));

            if (handler == null)
            {
                await _messenger.SendMessageAsync(userMessageInfo, "Неизвестная команда, попробуй команду /help", true);
                return;
            }

            await handler.HandleAsync(userMessageInfo);
        }

        private async Task HandlePhotoMessageAsync(UserMessageInfo userMessageInfo)
        {
            // TODO Вынести проверку на верхний уровень.
            if (!_messageHandlersMap.ContainsKey(MessageType.QrCode))
            {
                _logger.Warning("Не зарегистрирован обработчик фото с QR кодом.");
                return;
            }

            var handler = _messageHandlersMap[MessageType.QrCode].Single();
            await handler.HandleAsync(userMessageInfo);
        }

        public void Dispose()
        {
            _messenger.OnMessage -= OnMessage;
        }
    }
}