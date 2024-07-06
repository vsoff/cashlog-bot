using Cashlog.Common.Models;
using Cashlog.Core.Services.Abstract;
using Cashlog.Messenger;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Cashlog.Core.MessageHandlers.RequestHandlers;

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