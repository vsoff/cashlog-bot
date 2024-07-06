using System.Text;
using Cashlog.Common;
using Cashlog.Common.Models;
using Cashlog.Messenger;
using Cashlog.Messenger.Menu;

namespace Cashlog.Core.MessageHandlers.Handlers.Text;

internal sealed class SendMoneyMessagesHandler : TextCommandMessageHandler
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

    protected override Task<bool> ValidateAsync(UserMessageInfo userMessageInfo, string argument)
    {
        // TODO
        return Task.FromResult(true);
    }

    protected override async Task HandleAsync(UserMessageInfo userMessageInfo, string argument)
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