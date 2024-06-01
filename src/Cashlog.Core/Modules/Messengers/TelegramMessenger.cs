using Cashlog.Core.Models;
using Cashlog.Core.Modules.Messengers.Menu;
using Cashlog.Core.Options;
using Cashlog.Core.RequestHandlers;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using File = Telegram.Bot.Types.File;
using MessageType = Telegram.Bot.Types.Enums.MessageType;

namespace Cashlog.Core.Modules.Messengers;

/// <summary>
/// </summary>
/// <remarks>
///     В данный момент тут основная бизнес-логика, в последующем её необходимо вынести
///     в отдельный класс, чтобы можно было независимо менять мессенджеры.
/// </remarks>
public class TelegramMessenger : IMessenger
{
    private readonly IOptions<CashlogOptions> _cashlogOptions;
    private readonly IQueryDataSerializer _queryDataSerializer;
    private readonly IMediator _mediator;
    private readonly ILogger<TelegramMessenger> _logger;

    private TelegramBotClient _client;
    private CancellationTokenSource _clientCancellationTokenSource;

    public TelegramMessenger(
        IOptions<CashlogOptions> cashlogOptions,
        IQueryDataSerializer queryDataSerializer,
        IMediator mediator,
        ILogger<TelegramMessenger> logger)
    {
        _cashlogOptions = cashlogOptions;
        _queryDataSerializer = queryDataSerializer;
        _mediator = mediator;
        _logger = logger;
    }

    public async ValueTask StartReceivingAsync(CancellationToken cancellationToken)
    {
        if (_client != null)
            throw new InvalidOperationException("Бот уже запущен");

        var cashlogOptions = _cashlogOptions.Value;
        if (string.IsNullOrEmpty(cashlogOptions.TelegramBotToken))
            throw new InvalidOperationException("Telegram токен не должен быть пустым");

        _client = new TelegramBotClient(cashlogOptions.TelegramBotToken);

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates =
            [
                UpdateType.Message,
                UpdateType.CallbackQuery
            ],
            ThrowPendingUpdates = true
        };
        _clientCancellationTokenSource = new CancellationTokenSource();
        _client.StartReceiving(
            HandleUpdate,
            PollingErrorHandler,
            receiverOptions,
            _clientCancellationTokenSource.Token);

        await _client.SendTextMessageAsync(
            cashlogOptions.AdminChatToken,
            text: "Бот запущен!",
            cancellationToken: cancellationToken);

        _logger.LogInformation("Telegram бот запущен");
    }

    public async ValueTask StopReceivingAsync(CancellationToken cancellationToken)
    {
        await _clientCancellationTokenSource.CancelAsync();

        _logger.LogInformation("Telegram бот остановлен");
    }

    public async ValueTask EditMessageAsync(UserMessageInfo userMessageInfo, string text, IMenu menu = null)
    {
        await _client.EditMessageTextAsync(userMessageInfo.Group.ChatToken, int.Parse(userMessageInfo.Message.Token),
            text, replyMarkup: (InlineKeyboardMarkup)((TelegramMenu)menu)?.Markup);
    }

    public async ValueTask SendMessageAsync(UserMessageInfo userMessageInfo, string text, bool isReply = false,
        IMenu menu = null)
    {
        var replyToMessageId = isReply ? int.Parse(userMessageInfo.Message.Token) : 0;

        if (menu is not null && menu is not TelegramMenu)
            throw new ArgumentException(
                $"{nameof(TelegramMessenger)} может работать только с {nameof(IMenu)} типа {nameof(TelegramMenu)}");

        await _client.SendTextMessageAsync(
            userMessageInfo.Group.ChatToken,
            text,
            replyToMessageId: replyToMessageId,
            replyMarkup: (menu as TelegramMenu)?.Markup);
    }

    private Task PollingErrorHandler(ITelegramBotClient client, Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Telegram exception occurred");
        return Task.CompletedTask;
    }

    private async Task HandleUpdate(ITelegramBotClient client, Update updateData, CancellationToken cancellationToken)
    {
        try
        {
            switch (updateData.Type)
            {
                case UpdateType.Message:
                {
                    await HandleMessage(updateData.Message, cancellationToken);
                    break;
                }
                case UpdateType.CallbackQuery:
                {
                    await HandleCallbackQuery(updateData.CallbackQuery, cancellationToken);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Произошла ошибка во время обработки query запроса ");
        }
    }

    private async Task HandleCallbackQuery(CallbackQuery query, CancellationToken cancellationToken)
    {
        var queryData = _queryDataSerializer.DecodeBase64(query.Data);

        // Collect telegram specified data about message and sender.
        var consumeRequest = new ConsumeUserMessageRequest
        {
            ChatToken = query.Message.Chat.Id.ToString(),
            UserName = query.From.Username,
            UserToken = query.From.Id.ToString(),
            MessageType = Models.MessageType.Query,
            Message = new MessageInfo
            {
                Token = query.Message.MessageId.ToString(),
                Text = query.Message.Text,
                QueryData = queryData
            }
        };

        await _mediator.Send(consumeRequest, cancellationToken);
    }

    private async Task HandleMessage(Message message, CancellationToken cancellationToken)
    {
        if (message.Chat.Type != ChatType.Group)
            return;

        // Collect telegram specified data about message and sender.
        var request = new ConsumeUserMessageRequest
        {
            ChatToken = message.Chat.Id.ToString(),
            UserName = message.From.Username,
            UserToken = message.From.Id.ToString(),
            MessageType = Models.MessageType.Unknown,
            Message = new MessageInfo
            {
                Token = message.MessageId.ToString(),
                ReceiptInfo = null,
                Text = message.Text
            }
        };

        // Add specified message type data.
        switch (message.Type)
        {
            case MessageType.Photo:
            {
                var photoBytes = await GetMessagePhotoBytesAsync(message, cancellationToken);
                request.PhotoBytes = photoBytes;
                request.MessageType = Models.MessageType.QrCode;
                break;
            }

            case MessageType.Text:
            {
                request.MessageType = Models.MessageType.Text;
                break;
            }
        }

        await _mediator.Send(request, cancellationToken);
    }

    private async Task<byte[]> GetMessagePhotoBytesAsync(Message message, CancellationToken cancellationToken)
    {
        if (message.Photo is null)
        {
            throw new InvalidOperationException("Photos collection in message was empty");
        }
        
        var photoSize = message.Photo.MaxBy(x => x.Width);

        if (photoSize is null)
        {
            throw new InvalidOperationException("Not found photo in collection");
        }

        _logger.LogTrace(
            "Получено фото чека с разрешением W:{Width} H:{Height}",
            photoSize.Width,
            photoSize.Height);

        var file = await _client.GetFileAsync(photoSize.FileId, cancellationToken);
        var imageBytes = await DownloadFile(file, cancellationToken);

        _logger.LogTrace(
            "Получено изображение из потока размером {Bytes} байт",
            imageBytes.Length);

        return imageBytes;
    }

    private async Task<byte[]> DownloadFile(File file, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(file.FilePath))
        {
            throw new InvalidOperationException("Photo filePath was null or empty");
        }
        
        byte[] imageBytes;
        try
        {
            await using var clientStream = new MemoryStream();
            await _client.DownloadFileAsync(file.FilePath, clientStream, cancellationToken);
            imageBytes = clientStream.ToArray();
        }
        catch (Exception ex)
        {
            throw new Exception("Ошибка во время скачки изображения с чеком из telegram", ex);
        }

        return imageBytes;
    }
}