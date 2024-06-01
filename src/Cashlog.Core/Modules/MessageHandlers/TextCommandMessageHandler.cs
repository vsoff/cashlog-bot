using System.Text;
using Cashlog.Common;
using Cashlog.Core.Extensions;
using Cashlog.Core.Models;
using Cashlog.Core.Models.Main;
using Cashlog.Core.Modules.Messengers;
using Cashlog.Core.Modules.Messengers.Menu;
using Cashlog.Core.Services.Abstract;

namespace Cashlog.Core.Modules.MessageHandlers;

/// <summary>
///     Обработчик текстовых комманд.
/// </summary>
public abstract class TextCommandMessageHandler : IMessageHandler
{
    public abstract string Command { get; }
    public MessageType MessageType => MessageType.Text;

    public async Task HandleAsync(UserMessageInfo userMessageInfo)
    {
        // Читаем только команды.
        var text = userMessageInfo.Message.Text;
        if (text == null || !text.StartsWith("/"))
            throw new ArgumentException("Хендлер принимает только команды начинающиеся с символа `/`");

        var command = text.GetCommand(out var argument);
        if (command != Command)
            throw new ArgumentException($"Хендлер принимает команду `{Command}`, а не `{command}`");

        await HandleAsync(userMessageInfo, argument);
    }

    public abstract Task HandleAsync(UserMessageInfo userMessageInfo, string argument);
}

public class ReceiptMessagesHandler : TextCommandMessageHandler
{
    private readonly IBillingPeriodService _billingPeriodService;
    private readonly IMenuProvider _menuProvider;
    private readonly IMessenger _messenger;
    private readonly IReceiptService _receiptService;

    public ReceiptMessagesHandler(
        IBillingPeriodService billingPeriodService,
        IReceiptService receiptService,
        IMenuProvider menuProvider,
        IMessenger messenger)
    {
        _billingPeriodService = billingPeriodService ?? throw new ArgumentNullException(nameof(billingPeriodService));
        _receiptService = receiptService ?? throw new ArgumentNullException(nameof(receiptService));
        _menuProvider = menuProvider ?? throw new ArgumentNullException(nameof(menuProvider));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
    }

    public override string Command => "receipt";

    public override async Task HandleAsync(UserMessageInfo userMessageInfo, string argument)
    {
        var args = argument.Split(' ').ToArray();
        const int argsCount = 1;
        if (args.Length <= argsCount)
        {
            await _messenger.SendMessageAsync(userMessageInfo, Resources.CommandWrongArgsCount, true);
            return;
        }

        if (!int.TryParse(args[0], out var money))
        {
            await _messenger.SendMessageAsync(userMessageInfo, Resources.CommandFirstArgMustBeANumber, true);
            return;
        }

        var caption = string.Join(" ", args.Skip(argsCount));

        const int commentMinLength = 2;
        const int commentMaxLength = 50;
        if (caption.Length < commentMinLength || caption.Length > commentMaxLength)
        {
            var msgText = string.Format(Resources.CommentLengthWrong, commentMinLength, commentMaxLength);
            await _messenger.SendMessageAsync(userMessageInfo, msgText, true);
            return;
        }

        await HandleAsync(userMessageInfo, money, caption);
    }

    public async Task HandleAsync(UserMessageInfo userMessageInfo, int amount, string caption)
    {
        var lastBillingPeriod = await _billingPeriodService.GetLastByGroupIdAsync(userMessageInfo.Group.Id);
        if (lastBillingPeriod == null)
        {
            await _messenger.SendMessageAsync(userMessageInfo, "В группе ещё не начат ни один расчётный период", true);
            return;
        }

        var newReceipt = await _receiptService.AddAsync(new ReceiptDto
        {
            BillingPeriodId = lastBillingPeriod.Id,
            TotalAmount = amount,
            Status = ReceiptStatus.NewManual,
            Comment = caption
        });

        var menu = _menuProvider.GetMenu(userMessageInfo, new AddReceiptQueryData
        {
            MenuType = MenuType.NewReceiptSelectCustomer,
            ReceiptId = newReceipt.Id,
            SelectedCustomerId = null,
            SelectedConsumerIds = [],
            TargetId = null,
            Version = AddReceiptQueryData.ServerVersion
        });
        await _messenger.SendMessageAsync(userMessageInfo, Resources.SelectCustomer, true, menu);
    }
}

public class SendMoneyMessagesHandler : TextCommandMessageHandler
{
    private readonly IMenuProvider _menuProvider;
    private readonly IMessenger _messenger;

    public SendMoneyMessagesHandler(
        IMenuProvider menuProvider,
        IMessenger messenger)
    {
        _menuProvider = menuProvider ?? throw new ArgumentNullException(nameof(menuProvider));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
    }

    public override string Command => "send";

    public override async Task HandleAsync(UserMessageInfo userMessageInfo, string argument)
    {
        var args = argument.Split(' ').ToArray();
        if (args.Length <= 1)
        {
            await _messenger.SendMessageAsync(userMessageInfo, Resources.CommandWrongArgsCount, true);
            return;
        }

        if (!int.TryParse(args[0], out var money))
        {
            await _messenger.SendMessageAsync(userMessageInfo, Resources.CommandFirstArgMustBeANumber, true);
            return;
        }

        var caption = string.Join(" ", args.Skip(1));
        if (Encoding.ASCII.GetBytes(caption).Length > 12)
        {
            await _messenger.SendMessageAsync(userMessageInfo,
                "Комментарий должен весить 12 или меньше байт (пока что такое ограничение)", true);
            return;
        }

        const int commentMinLength = 2;
        const int commentMaxLength = 15;
        if (caption.Length < commentMinLength || caption.Length > commentMaxLength)
        {
            var msgText = string.Format(Resources.CommentLengthWrong, commentMinLength, commentMaxLength);
            await _messenger.SendMessageAsync(userMessageInfo, msgText, true);
            return;
        }

        await HandleAsync(userMessageInfo, money, caption);
    }

    public async Task HandleAsync(UserMessageInfo userMessageInfo, int amount, string caption)
    {
        var menu = _menuProvider.GetMenu(userMessageInfo, new MoneyTransferQueryData
        {
            Version = MoneyTransferQueryData.ServerVersion,
            Amount = amount,
            Caption = caption,
            ChatToken = userMessageInfo.Group.ChatToken,
            CustomerFromId = null,
            CustomerToId = null,
            MenuType = MenuType.MoneyTransferSelectFrom,
            TargetId = null
        });
        await _messenger.SendMessageAsync(userMessageInfo, Resources.MoneyTransferSelectFrom, true, menu);
    }
}

public class PeriodMessagesHandler : TextCommandMessageHandler
{
    private readonly IMainLogicService _mainLogicService;
    private readonly IMessenger _messenger;

    public PeriodMessagesHandler(
        IMainLogicService mainLogicService,
        IMessenger messenger)
    {
        _mainLogicService = mainLogicService ?? throw new ArgumentNullException(nameof(mainLogicService));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
    }

    public override string Command => "period";

    public override async Task HandleAsync(UserMessageInfo userMessageInfo, string argument)
    {
        if (!string.IsNullOrEmpty(argument))
        {
            await _messenger.SendMessageAsync(userMessageInfo, Resources.CommandWrongArgsCount, true);
            return;
        }

        await HandleAsync(userMessageInfo);
    }

    public async Task HandleAsync(UserMessageInfo userMessageInfo)
    {
        if (!userMessageInfo.IsAdmin())
        {
            await _messenger.SendMessageAsync(userMessageInfo,
                "Только создатель группы может создать новый расчётный период", true);
            return;
        }

        var newPeriodResult = await _mainLogicService.CloseCurrentAndOpenNewPeriod(userMessageInfo.Group.Id);
        var namesMap = userMessageInfo.Customers.ToDictionary(x => x.Id, x => x.Caption);

        var debts = string.Join("\n", newPeriodResult.Debts
            .Select(x => $"* `{namesMap[x.CustomerFromId]}` должен отдать `{namesMap[x.CustomerToId]}` {x.Amount}р."));
        await _messenger.SendMessageAsync(userMessageInfo,
            $"Был начат новый период, долги за предыдущий составляют:\n{debts}", true);
    }
}

public class CustomerMessagesHandler : TextCommandMessageHandler
{
    private readonly ICustomerService _customerService;
    private readonly IMessenger _messenger;

    public CustomerMessagesHandler(
        ICustomerService customerService,
        IMessenger messenger)
    {
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
    }

    public override string Command => "customer";

    public override async Task HandleAsync(UserMessageInfo userMessageInfo, string argument)
    {
        const int maxLength = 30;
        if (argument.Length > maxLength)
        {
            await _messenger.SendMessageAsync(userMessageInfo,
                $"Название потребителя должно быть не больше {maxLength} символов", true);
            return;
        }

        await _customerService.AddAsync(new CustomerDto
        {
            Caption = argument,
            GroupId = userMessageInfo.Group.Id
        });

        await _messenger.SendMessageAsync(userMessageInfo, $"Добавлен новый потребитель: {argument}", true);
    }
}

public class DebtsMessagesHandler : TextCommandMessageHandler
{
    private readonly IBillingPeriodService _billingPeriodService;
    private readonly IMainLogicService _mainLogicService;
    private readonly IMessenger _messenger;

    public DebtsMessagesHandler(
        IBillingPeriodService billingPeriodService,
        IMainLogicService mainLogicService,
        IMessenger messenger)
    {
        _billingPeriodService = billingPeriodService ?? throw new ArgumentNullException(nameof(billingPeriodService));
        _mainLogicService = mainLogicService ?? throw new ArgumentNullException(nameof(mainLogicService));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
    }

    public override string Command => "debts";

    public override async Task HandleAsync(UserMessageInfo userMessageInfo, string argument)
    {
        if (!string.IsNullOrEmpty(argument))
        {
            await _messenger.SendMessageAsync(userMessageInfo, Resources.CommandWrongArgsCount, true);
            return;
        }

        await HandleAsync(userMessageInfo);
    }

    public async Task HandleAsync(UserMessageInfo userMessageInfo)
    {
        var currentBilling = await _billingPeriodService.GetLastByGroupIdAsync(userMessageInfo.Group.Id);
        var debts = await _mainLogicService.CalculatePeriodCurrentDebts(currentBilling.Id);

        var debtsMessages = debts.Select(x =>
        {
            var from = userMessageInfo.Customers.First(c => c.Id == x.FromId).Caption;
            var to = userMessageInfo.Customers.First(c => c.Id == x.ToId).Caption;
            return $"* {from} должен {to}: {(int)x.Amount}р.";
        }).ToArray();

        await _messenger.SendMessageAsync(userMessageInfo,
            $"Промежуточный итог долгов на период с {currentBilling.PeriodBegin}:\n{string.Join("\n", debtsMessages)}",
            true);
    }
}

public class ReportMessagesHandler : TextCommandMessageHandler
{
    private readonly ICustomerService _customerService;
    private readonly IMessenger _messenger;
    private readonly IReceiptService _receiptService;

    public ReportMessagesHandler(
        ICustomerService customerService,
        IReceiptService receiptService,
        IMessenger messenger)
    {
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        _receiptService = receiptService ?? throw new ArgumentNullException(nameof(receiptService));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
    }

    public override string Command => "report";

    public override async Task HandleAsync(UserMessageInfo userMessageInfo, string argument)
    {
        var args = argument.Split(' ').ToArray();
        if (args.Length != 2)
        {
            await _messenger.SendMessageAsync(userMessageInfo, Resources.CommandWrongArgsCount, true);
            return;
        }

        if (!DateTime.TryParse(args[0], out var periodBegin) || !DateTime.TryParse(args[1], out var periodEnd))
        {
            await _messenger.SendMessageAsync(userMessageInfo, "Первый и второй аргумент должны быть датами", true);
            return;
        }

        await HandleAsync(userMessageInfo, periodBegin, periodEnd);
    }

    public async Task HandleAsync(UserMessageInfo userMessageInfo, DateTime periodBegin, DateTime periodEnd)
    {
        if (periodEnd - periodBegin < TimeSpan.FromDays(1))
        {
            await _messenger.SendMessageAsync(userMessageInfo, "Период дней не должен быть меньше дня", true);
            return;
        }

        var receipts = await _receiptService.GetReceiptsInPeriodAsync(periodBegin, periodEnd, userMessageInfo.Group.Id);
        var summary = receipts.Sum(x => x.TotalAmount);

        // Формируем сообщение.
        var customersMap = receipts
            .Where(x => x.CustomerId.HasValue)
            .GroupBy(x => x.CustomerId.Value)
            .ToDictionary(x => x.Key, x => x.ToArray());
        var customersNameMap = (await _customerService.GetListAsync(customersMap.Keys.ToArray()))
            .ToDictionary(x => x.Id, x => x.Caption);
        var receiptLines = receipts.Select(x =>
                $"ID {x.Id}: [{x.TotalAmount}р.] {x.Comment ?? "(Нет описания)"} ({customersNameMap[x.CustomerId.Value]})")
            .ToArray();

        var sb = new StringBuilder();
        sb.AppendLine(
            $"Траты за период с {periodBegin.ToShortDateString()} по {periodEnd.ToShortDateString()} составляют: {summary:F2}р.");
        sb.AppendLine();
        sb.AppendLine("Список участников:");
        foreach (var customerKvp in customersMap)
            sb.AppendLine(
                $"* {customersNameMap[customerKvp.Key]} потратил {customerKvp.Value.Sum(x => x.TotalAmount):F2}р.");
        sb.AppendLine();
        sb.AppendLine("Список чеков:");
        sb.AppendLine($"{string.Join(";\n", receiptLines)}.");

        await _messenger.SendMessageAsync(userMessageInfo, sb.ToString(), true);
    }
}