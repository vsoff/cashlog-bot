using Cashlog.Common;
using Cashlog.Common.Models;

namespace Cashlog.Core.MessageHandlers.Handlers.Query;

/// <summary>
///     Обработчик запросов типа <see cref="IQueryData" />.
/// </summary>
/// <typeparam name="TQueryData">Тип данных запроса.</typeparam>
internal abstract class QueryCommandMessageHandler<TQueryData> : IMessageHandler where TQueryData : IQueryData
{
    public MessageType MessageType => MessageType.Query;

    public async Task HandleAsync(UserMessageInfo userMessageInfo)
    {
        if (userMessageInfo.Message.QueryData is TQueryData)
            throw new ArgumentException(
                $"параметр должен быть типа {typeof(TQueryData)}, а не {userMessageInfo.Message.QueryData.GetType()}",
                nameof(userMessageInfo.Message.QueryData));

        // ReSharper disable once PossibleInvalidCastException
        await HandleAsync(userMessageInfo, (TQueryData)userMessageInfo.Message.QueryData);
    }

    protected abstract Task HandleAsync(UserMessageInfo userMessageInfo, TQueryData queryData);
}