using System;
using System.Threading.Tasks;
using Cashlog.Core.Models;
using Cashlog.Core.Modules.Messengers.Menu;

namespace Cashlog.Core.Modules.MessageHandlers
{
    /// <summary>
    /// Обработчик запросов типа <see cref="IQueryData"/>.
    /// </summary>
    /// <typeparam name="TQueryData">Тип данных запроса.</typeparam>
    public abstract class QueryCommandMessageHandler<TQueryData> : IMessageHandler where TQueryData : IQueryData
    {
        public MessageType MessageType => MessageType.Query;

        public async Task HandleAsync(UserMessageInfo userMessageInfo)
        {
            if (userMessageInfo.Message.QueryData is TQueryData)
                throw new ArgumentException($"{nameof(userMessageInfo.Message.QueryData)} должна быть типа" +
                                            $" {typeof(TQueryData)}, а не {userMessageInfo.Message.QueryData.GetType()}");

            // ReSharper disable once PossibleInvalidCastException
            await HandleAsync(userMessageInfo, (TQueryData) userMessageInfo.Message.QueryData);
        }

        public abstract Task HandleAsync(UserMessageInfo userMessageInfo, TQueryData queryData);
    }
}