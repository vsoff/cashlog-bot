using Cashlog.Core.Services.Abstract;
using Cashlog.Messenger;
using Cashlog.Messenger.Telegram;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cashlog.Core.MessageHandlers.RequestHandlers;

public class UserIdentifyBehavior : IPipelineBehavior<ConsumeUserMessageRequest, Unit>
{
    private readonly IMessenger _messenger;
    private readonly ICustomerService _customerService;
    private readonly IGroupService _groupService;
    private readonly IOptions<TelegramOptions> _telegramOptions;
    private readonly ILogger<UserIdentifyBehavior> _logger;

    public UserIdentifyBehavior(
        IMessenger messenger,
        ICustomerService customerService,
        IGroupService groupService,
        IOptions<TelegramOptions> telegramOptions,
        ILogger<UserIdentifyBehavior> logger)
    {
        _messenger = messenger;
        _customerService = customerService;
        _groupService = groupService;
        _telegramOptions = telegramOptions;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        ConsumeUserMessageRequest request,
        RequestHandlerDelegate<Unit> next,
        CancellationToken cancellationToken)
    {
        if (request.ChatToken != _telegramOptions.Value.AdminChatToken)
        {
            _logger.LogInformation("Произведена попытка использования бота в группе ChatToken={ChatToken}",
                request.ChatToken);

            await _messenger.SendMessageAsync(request, "Чтобы бот мог работать в этой группе обратитесь к @vsoff");
            return Unit.Value;
        }

        var group = await _groupService.GetByChatTokenAsync(request.ChatToken);

        if (group is null)
        {
            group = await _groupService.AddAsync(request.ChatToken, request.UserToken, "Default group name");
        }
        
        var customers = await _customerService.GetListAsync(group.Id);

        request.Group = group;
        request.Customers = customers;

        await next();

        return Unit.Value;
    }
}