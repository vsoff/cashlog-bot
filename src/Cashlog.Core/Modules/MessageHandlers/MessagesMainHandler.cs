using System.Diagnostics;
using Cashlog.Common;
using Cashlog.Core.Common;
using Cashlog.Core.Models;
using Cashlog.Core.Models.Main;
using Cashlog.Core.Modules.Messengers;
using Cashlog.Core.Modules.Messengers.Menu;
using Cashlog.Core.Services.Abstract;

namespace Cashlog.Core.Modules.MessageHandlers;

public class MessagesMainHandler
{
    private readonly IBillingPeriodService _billingPeriodService;
    private readonly ILogger _logger;
    private readonly IMenuProvider _menuProvider;
    private readonly IReadOnlyDictionary<MessageType, IMessageHandler[]> _messageHandlersMap;
    private readonly IMessenger _messenger;
    private readonly IMoneyOperationService _moneyOperationService;
    private readonly IReceiptService _receiptService;

    public MessagesMainHandler(
        IEnumerable<IMessageHandler> messageHandlers,
        IMoneyOperationService moneyOperationService,
        IBillingPeriodService billingPeriodService,
        IReceiptService receiptService,
        IMenuProvider menuProvider,
        IMessenger messenger,
        ILogger logger)
    {
        if (messageHandlers == null) throw new ArgumentNullException(nameof(messageHandlers));
        _moneyOperationService =
            moneyOperationService ?? throw new ArgumentNullException(nameof(moneyOperationService));
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
        var sw = Stopwatch.StartNew();
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
                    await _messenger.SendMessageAsync(e, Resources.UnknownMessageType, true);
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
                var data = (AddReceiptQueryData)userMessageInfo.Message.QueryData;
                if (data.TargetId == data.SelectedCustomerId)
                    break;

                data.SelectedCustomerId = data.TargetId;

                var menu = _menuProvider.GetMenu(userMessageInfo, data);
                await _messenger.EditMessageAsync(userMessageInfo, Resources.SelectCustomer, menu);
                break;
            }
            case MenuType.NewReceiptSelectConsumers:
            {
                var data = (AddReceiptQueryData)userMessageInfo.Message.QueryData;
                var selectedConsumers = new HashSet<long>(data.SelectedConsumerIds ?? new long[0]);
                if (data.TargetId != null)
                {
                    if (selectedConsumers.Contains(data.TargetId.Value))
                        selectedConsumers.Remove(data.TargetId.Value);
                    else
                        selectedConsumers.Add(data.TargetId.Value);
                }

                selectedConsumers = new HashSet<long>(selectedConsumers.Distinct());
                data.SelectedConsumerIds = selectedConsumers.ToArray();

                var menu = _menuProvider.GetMenu(userMessageInfo, data);
                await _messenger.EditMessageAsync(userMessageInfo, Resources.SelectConsumers, menu);
                break;
            }
            case MenuType.NewReceiptAdd:
            {
                var data = (AddReceiptQueryData)userMessageInfo.Message.QueryData;
                var customerText = userMessageInfo.Customers
                    .FirstOrDefault(x => data.SelectedCustomerId.Value == x.Id).Caption;
                var consumerTexts = userMessageInfo.Customers
                    .Where(x => data.SelectedCustomerId.Value == x.Id || data.SelectedConsumerIds.Contains(x.Id))
                    .Select(x => x.Caption).ToArray();
                var receipt = await _receiptService.GetAsync(data.ReceiptId);
                await _messenger.EditMessageAsync(userMessageInfo, Resources.InProgress);
                if (receipt.Status == ReceiptStatus.New || receipt.Status == ReceiptStatus.NewManual)
                {
                    receipt.Status = receipt.Status == ReceiptStatus.New
                        ? ReceiptStatus.Filled
                        : ReceiptStatus.Manual;
                    receipt.CustomerId = data.SelectedCustomerId;
                    receipt.PurchaseTime = DateTime.Now;
                    await _receiptService.SetCustomersToReceiptAsync(receipt.Id, data.SelectedConsumerIds);
                    await _receiptService.UpdateAsync(receipt);

                    var msgText = string.Format(Resources.NewReceiptAddedInfo, customerText, receipt.TotalAmount,
                        string.Join(", ", consumerTexts));
                    await _messenger.EditMessageAsync(userMessageInfo, msgText);
                    _logger.Info(msgText);
                }

                break;
            }
            case MenuType.NewReceiptCancel:
            {
                var data = (AddReceiptQueryData)userMessageInfo.Message.QueryData;
                await _messenger.EditMessageAsync(userMessageInfo, Resources.NewReceiptCancel);
                var receipt = await _receiptService.GetAsync(data.ReceiptId);
                receipt.Status = ReceiptStatus.Deleted;
                await _receiptService.UpdateAsync(receipt);
                break;
            }
            case MenuType.MoneyTransferSelectFrom:
            {
                var data = (MoneyTransferQueryData)userMessageInfo.Message.QueryData;
                var menu = _menuProvider.GetMenu(userMessageInfo, data);
                await _messenger.EditMessageAsync(userMessageInfo, Resources.MoneyTransferSelectFrom, menu);
                break;
            }
            case MenuType.MoneyTransferSelectTo:
            {
                var data = (MoneyTransferQueryData)userMessageInfo.Message.QueryData;
                var menu = _menuProvider.GetMenu(userMessageInfo, data);
                await _messenger.EditMessageAsync(userMessageInfo, Resources.MoneyTransferSelectTo, menu);
                break;
            }
            case MenuType.MoneyTransferAdd:
            {
                var data = (MoneyTransferQueryData)userMessageInfo.Message.QueryData;
                var customerFrom =
                    userMessageInfo.Customers.FirstOrDefault(x => data.CustomerFromId == x.Id)?.Caption ??
                    $"Id: {data.CustomerFromId}";
                var customerTo = userMessageInfo.Customers.FirstOrDefault(x => data.CustomerToId == x.Id)?.Caption ??
                                 $"Id: {data.CustomerToId}";
                await _messenger.EditMessageAsync(userMessageInfo, Resources.InProgress);

                var lastBillingPeriod = await _billingPeriodService.GetLastByGroupIdAsync(userMessageInfo.Group.Id);
                if (lastBillingPeriod == null)
                {
                    await _messenger.EditMessageAsync(userMessageInfo, Resources.BillingPeriodNotCreated);
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

                var msgText = string.Format(Resources.MoneyTransferSuccess, customerFrom, customerTo, data.Amount);
                await _messenger.EditMessageAsync(userMessageInfo, msgText);
                _logger.Info(msgText.Replace("\n", ""));
                break;
            }
            case MenuType.MoneyTransferCancel:
            {
                await _messenger.EditMessageAsync(userMessageInfo, Resources.MoneyTransferCanceled);
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

        _logger.Trace(
            $"@{userMessageInfo.UserName} прислал сообщение. Длина сообщения: {userMessageInfo.Message.Text.Length}");

        // Читаем только команды.
        var text = userMessageInfo.Message.Text;
        if (!text.StartsWith("/"))
        {
            await _messenger.SendMessageAsync(userMessageInfo, Resources.UnknownMessageFormat, true);
            return;
        }

        var command = text.GetCommand(out _);
        var handler = _messageHandlersMap[MessageType.Text]
            .OfType<TextCommandMessageHandler>()
            .FirstOrDefault(x => x.Command.Equals(command, StringComparison.OrdinalIgnoreCase));

        if (handler == null)
        {
            await _messenger.SendMessageAsync(userMessageInfo, Resources.UnknownCommandType, true);
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