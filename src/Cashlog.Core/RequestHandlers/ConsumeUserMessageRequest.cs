using Cashlog.Core.Models;
using Cashlog.Core.Modules.MessageHandlers;
using Cashlog.Core.Modules.Messengers;
using Cashlog.Core.Options;
using Cashlog.Core.Services.Abstract;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cashlog.Core.RequestHandlers;

public class ConsumeUserMessageRequest : UserMessageInfo, IRequest<Unit>
{
    public byte[]? PhotoBytes { get; set; }
    public required string ChatToken { get; init; }
}

public class UserIdentifyBehavior : IPipelineBehavior<ConsumeUserMessageRequest, Unit>
{
    private readonly IMessenger _messenger;
    private readonly ICustomerService _customerService;
    private readonly IGroupService _groupService;
    private readonly IOptions<CashlogOptions> _cashlogOptions;
    private readonly ILogger<UserIdentifyBehavior> _logger;

    public UserIdentifyBehavior(
        IMessenger messenger,
        ICustomerService customerService,
        IGroupService groupService,
        IOptions<CashlogOptions> cashlogOptions,
        ILogger<UserIdentifyBehavior> logger)
    {
        _messenger = messenger;
        _customerService = customerService;
        _groupService = groupService;
        _cashlogOptions = cashlogOptions;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        ConsumeUserMessageRequest request,
        RequestHandlerDelegate<Unit> next,
        CancellationToken cancellationToken)
    {
        if (request.ChatToken != _cashlogOptions.Value.AdminChatToken)
        {
            _logger.LogInformation("Произведена попытка использования бота в группе ChatToken={ChatToken}",
                request.ChatToken);

            await _messenger.SendMessageAsync(request, "Чтобы бот мог работать в этой группе обратитесь к @vsoff");
            return Unit.Value;
        }

        var group = await _groupService.GetByChatTokenAsync(request.ChatToken);
        var customers = await _customerService.GetListAsync(group.Id);

        if (group is null)
        {
            // TODO: Feature to auto-create group.
            throw new NotImplementedException(
                "Got message from unknown group, now group recreation is not implemented");
        }

        request.Group = group;
        request.Customers = customers;

        await next();

        return Unit.Value;
    }
}

public class DecodeImageBehavior : IPipelineBehavior<ConsumeUserMessageRequest, Unit>
{
    private readonly IReceiptHandleService _receiptHandleService;
    private readonly ILogger<DecodeImageBehavior> _logger;

    public DecodeImageBehavior(
        IReceiptHandleService receiptHandleService,
        ILogger<DecodeImageBehavior> logger)
    {
        _receiptHandleService = receiptHandleService;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        ConsumeUserMessageRequest request,
        RequestHandlerDelegate<Unit> next,
        CancellationToken cancellationToken)
    {
        if (request.MessageType == MessageType.QrCode
            && request.PhotoBytes is not null
            && request.PhotoBytes.Length > 0)
        {
            var data = _receiptHandleService.ParsePhoto(request.PhotoBytes);
            _logger.LogTrace(data == null
                ? "Не удалось распознать QR код на чеке"
                : $"Данные с QR кода чека {data.RawData}");

            request.Message.ReceiptInfo = data;
        }

        await next();
        return Unit.Value;
    }
}

public class ConsumeUserMessageRequestHandler : IRequestHandler<ConsumeUserMessageRequest, Unit>
{
    private readonly IMessagesMainHandler _messagesMainHandler;

    public ConsumeUserMessageRequestHandler(
        IMessagesMainHandler messagesMainHandler)
    {
        _messagesMainHandler = messagesMainHandler;
    }

    public async Task<Unit> Handle(ConsumeUserMessageRequest request, CancellationToken cancellationToken)
    {
        await _messagesMainHandler.HandleMessageAsync(request, cancellationToken);
        return Unit.Value;
    }
}