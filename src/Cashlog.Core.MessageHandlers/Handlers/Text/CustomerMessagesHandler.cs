using Cashlog.Common.Models;
using Cashlog.Common.Models.Main;
using Cashlog.Core.Services.Abstract;
using Cashlog.Messenger;

namespace Cashlog.Core.MessageHandlers.Handlers.Text;

internal sealed class CustomerMessagesHandler : TextCommandMessageHandler
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

    protected override async Task<bool> ValidateAsync(UserMessageInfo userMessageInfo, string argument)
    {
        const int maxLength = 30;
        if (argument.Length > maxLength)
        {
            await _messenger.SendMessageAsync(userMessageInfo,
                $"Название потребителя должно быть не больше {maxLength} символов", true);
            return false;
        }

        return true;
    }

    protected override async Task HandleAsync(UserMessageInfo userMessageInfo, string argument)
    {
        await _customerService.AddAsync(new CustomerDto
        {
            Caption = argument,
            GroupId = userMessageInfo.Group.Id
        });

        await _messenger.SendMessageAsync(userMessageInfo, $"Добавлен новый потребитель: {argument}", true);
    }
}