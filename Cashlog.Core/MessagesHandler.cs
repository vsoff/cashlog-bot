using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cashlog.Core.Common;
using Cashlog.Core.Extensions;
using Cashlog.Core.Models;
using Cashlog.Core.Models.Main;
using Cashlog.Core.Modules.Calculator;
using Cashlog.Core.Modules.Messengers;
using Cashlog.Core.Modules.Messengers.Menu;
using Cashlog.Core.Services.Abstract;

namespace Cashlog.Core
{
    public class MessagesHandler : IMessagesHandler
    {
        private readonly IMoneyOperationService _moneyOperationService;
        private readonly IBillingPeriodService _billingPeriodService;
        private readonly IMainLogicService _mainLogicService;
        private readonly ICustomerService _customerService;
        private readonly IReceiptService _receiptService;
        private readonly IMenuProvider _menuProvider;
        private readonly IMessenger _messenger;
        private readonly ILogger _logger;

        public MessagesHandler(
            IMoneyOperationService moneyOperationService,
            IBillingPeriodService billingPeriodService,
            IMainLogicService mainLogicService,
            ICustomerService customerService,
            IReceiptService receiptService,
            IMenuProvider menuProvider,
            IMessenger messenger,
            ILogger logger)
        {
            _moneyOperationService = moneyOperationService ?? throw new ArgumentNullException(nameof(moneyOperationService));
            _billingPeriodService = billingPeriodService ?? throw new ArgumentNullException(nameof(billingPeriodService));
            _mainLogicService = mainLogicService ?? throw new ArgumentNullException(nameof(mainLogicService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _receiptService = receiptService ?? throw new ArgumentNullException(nameof(receiptService));
            _menuProvider = menuProvider ?? throw new ArgumentNullException(nameof(menuProvider));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _messenger.OnMessage += OnMessage;
        }

        private async void OnMessage(object sender, UserMessageInfo e)
        {
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                _logger.Trace($"Получено сообщение типа {e.MessageType} от {e.UserToken} из группы {e.Group.ChatToken}");
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
                        await _receiptService.SetCustomersToReceiptAsync(receipt.Id, data.SelectedConsumerIds);
                        await _receiptService.UpdateAsync(receipt);
                    }

                    await _messenger.EditMessageAsync(userMessageInfo, $"Чек готов!\nОплатил: {customerText}\nСумма: {receipt.TotalAmount} руб.\nДелится на: {string.Join(", ", consumerTexts)}");
                    _logger.Info($"Добавлен новый чек. Оплатил: {customerText}; Сумма: {receipt.TotalAmount} руб.; Делится на: {string.Join(", ", consumerTexts)}");
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
            _logger.Trace($"@{userMessageInfo.UserName} прислал сообщение. Длина сообщения: {userMessageInfo.Message.Text.Length}");

            // Читаем только команды.
            string text = userMessageInfo.Message.Text;
            if (!text.StartsWith("/"))
            {
                await _messenger.SendMessageAsync(userMessageInfo, "Я не читаю сообщения, попробуй прислать мне фото или используй команду /help", true);
                return;
            }

            string[] args = text.Split(' ').ToArray();
            string cmd = args.First().Substring(1);
            args = args.Skip(1).ToArray();

            switch (cmd)
            {
                case "debts":
                {
                    if (args.Length != 0)
                    {
                        await _messenger.SendMessageAsync(userMessageInfo, "Неверное кол-во параметров для команды", true);
                        return;
                    }

                    var currentBilling = await _billingPeriodService.GetLastByGroupIdAsync(userMessageInfo.Group.Id);
                    var debts = await _mainLogicService.CalculatePeriodCurrentDebts(currentBilling.Id);

                    string[] debtsMessages = debts.Select(x =>
                    {
                        string from = userMessageInfo.Customers.First(c => c.Id == x.FromId).Caption;
                        string to = userMessageInfo.Customers.First(c => c.Id == x.ToId).Caption;
                        return $"* {from} должен {to}: {(int) x.Amount}р.";
                    }).ToArray();

                    await _messenger.SendMessageAsync(userMessageInfo, $"Промежуточный итог долгов на период с {currentBilling.PeriodBegin}:\n{string.Join("\n", debtsMessages)}", true);
                    return;
                }
                case "customer":
                {
                    if (args.Length != 1)
                    {
                        await _messenger.SendMessageAsync(userMessageInfo, "Неверное кол-во параметров для команды", true);
                        return;
                    }

                    await _customerService.AddAsync(new Customer
                    {
                        Caption = args[0],
                        GroupId = userMessageInfo.Group.Id
                    });

                    await _messenger.SendMessageAsync(userMessageInfo, $"Добавлен новый потребитель: {args[0]}", true);
                    return;
                }
                case "period":
                {
                    if (args.Length != 0)
                    {
                        await _messenger.SendMessageAsync(userMessageInfo, "Неверное кол-во параметров для команды", true);
                        return;
                    }

                    if (!userMessageInfo.IsAdmin())
                    {
                        await _messenger.SendMessageAsync(userMessageInfo, "Только создатель группы может создать новый расчётный период", true);
                        return;
                    }

                    ClosingPeriodResult newPeriodResult = await _mainLogicService.CloseCurrentAndOpenNewPeriod(userMessageInfo.Group.Id);
                    var namesMap = userMessageInfo.Customers.ToDictionary(x => x.Id, x => x.Caption);

                    string debts = string.Join("\n", newPeriodResult.Debts
                        .Select(x => $"* `{namesMap[x.CustomerFromId]}` должен отдать `{namesMap[x.CustomerToId]}` {x.Amount}р."));
                    await _messenger.SendMessageAsync(userMessageInfo, $"Был начат новый период, долги за предыдущий составляют:\n{debts}", true);
                    return;
                }
                case "send":
                {
                    if (args.Length <= 1)
                    {
                        await _messenger.SendMessageAsync(userMessageInfo, "Неверное кол-во параметров для команды", true);
                        return;
                    }

                    if (!int.TryParse(args[0], out var money))
                    {
                        await _messenger.SendMessageAsync(userMessageInfo, "Первый аргумент должен быть числом", true);
                        return;
                    }

                    var caption = string.Join(" ", args.Skip(1));
                    if (Encoding.ASCII.GetBytes(caption).Length > 12)
                    {
                        await _messenger.SendMessageAsync(userMessageInfo, "Комментарий должен весить 12 или меньше байт (пока что такое ограничение)", true);
                        return;
                    }

                    if (caption.Length > 15 || caption.Length < 3)
                    {
                        await _messenger.SendMessageAsync(userMessageInfo, "Комментарий должен быть не больше 15 и не меньше 3 символов", true);
                        return;
                    }

                    IMenu menu = _menuProvider.GetMenu(userMessageInfo, new MoneyTransferQueryData
                    {
                        Version = MoneyTransferQueryData.CurrentServerVersion,
                        Amount = money,
                        Caption = caption,
                        ChatToken = userMessageInfo.Group.ChatToken,
                        CustomerFromId = null,
                        CustomerToId = null,
                        MenuType = MenuType.MoneyTransferSelectFrom,
                        TargetId = null
                    });
                    await _messenger.SendMessageAsync(userMessageInfo, "Кто переводит деньги?", true, menu);
                    return;
                }
                case "receipt":
                {
                    if (args.Length <= 1)
                    {
                        await _messenger.SendMessageAsync(userMessageInfo, "Неверное кол-во параметров для команды", true);
                        return;
                    }

                    if (!int.TryParse(args[0], out var money))
                    {
                        await _messenger.SendMessageAsync(userMessageInfo, "Первый аргумент должен быть числом", true);
                        return;
                    }

                    var caption = string.Join(" ", args.Skip(1));

                    if (caption.Length > 50 || caption.Length < 3)
                    {
                        await _messenger.SendMessageAsync(userMessageInfo, "Комментарий должен быть от 3 до 50 символов", true);
                        return;
                    }

                    BillingPeriod lastBillingPeriod = await _billingPeriodService.GetLastByGroupIdAsync(userMessageInfo.Group.Id);
                    if (lastBillingPeriod == null)
                    {
                        await _messenger.SendMessageAsync(userMessageInfo, "В группе ещё не начат ни один расчётный период", true);
                        return;
                    }

                    var newReceipt = await _receiptService.AddAsync(new Receipt
                    {
                        BillingPeriodId = lastBillingPeriod.Id,
                        TotalAmount = money,
                        Status = ReceiptStatus.NewManual,
                    });

                    IMenu menu = _menuProvider.GetMenu(userMessageInfo, new AddReceiptQueryData
                    {
                        MenuType = MenuType.NewReceiptSelectCustomer,
                        ReceiptId = newReceipt.Id,
                        SelectedCustomerId = null,
                        SelectedConsumerIds = new long[0],
                        TargetId = null,
                        Version = AddReceiptQueryData.CurrentServerVersion,
                    });
                    await _messenger.SendMessageAsync(userMessageInfo, "_Кто оплатил чек?", true, menu);
                    return;
                }
                default:
                {
                    await _messenger.SendMessageAsync(userMessageInfo, "Неизвестная команда, попробуй команду /help", true);
                    return;
                }
            }
        }

        private async Task HandlePhotoMessageAsync(UserMessageInfo userMessageInfo)
        {
            ReceiptMainInfo data = userMessageInfo.Message.ReceiptInfo;

            if (data == null)
            {
                const string msg = "Не удалось прочитать чек";
                await _messenger.SendMessageAsync(userMessageInfo, msg, true);
                _logger.Trace(msg);
                return;
            }

            BillingPeriod lastBillingPeriod = await _billingPeriodService.GetLastByGroupIdAsync(userMessageInfo.Group.Id);
            if (lastBillingPeriod == null)
            {
                await _messenger.SendMessageAsync(userMessageInfo, "В группе ещё не начат ни один расчётный период", true);
                return;
            }

            var receipt = new Receipt
            {
                BillingPeriodId = lastBillingPeriod.Id,
                TotalAmount = data.TotalAmount,
                FiscalSign = data.FiscalSign,
                FiscalDocument = data.FiscalDocument,
                FiscalNumber = data.FiscalNumber,
                PurchaseTime = data.PurchaseTime,
                Status = ReceiptStatus.New,
            };

            if (await _receiptService.IsReceiptExists(receipt))
            {
                await _messenger.SendMessageAsync(userMessageInfo, "Такой чек уже добавлен в БД", true);
                return;
            }

            var newReceipt = await _receiptService.AddAsync(receipt);

            IMenu menu = _menuProvider.GetMenu(userMessageInfo, new AddReceiptQueryData
            {
                MenuType = MenuType.NewReceiptSelectCustomer,
                ReceiptId = newReceipt.Id,
                SelectedCustomerId = null,
                SelectedConsumerIds = new long[0],
                TargetId = null,
                Version = AddReceiptQueryData.CurrentServerVersion,
            });
            await _messenger.SendMessageAsync(userMessageInfo, "_Кто оплатил чек?", true, menu);
        }

        public void Dispose()
        {
            _messenger.OnMessage -= OnMessage;
        }
    }
}