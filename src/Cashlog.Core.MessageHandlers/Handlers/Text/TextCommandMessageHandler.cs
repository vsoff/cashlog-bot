using Cashlog.Common.Models;

namespace Cashlog.Core.MessageHandlers.Handlers.Text;

/// <summary>
///     Обработчик текстовых комманд.
/// </summary>
internal abstract class TextCommandMessageHandler : IMessageHandler
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

        if (!await ValidateAsync(userMessageInfo, argument))
        {
            return;
        }

        await HandleAsync(userMessageInfo, argument);
    }

    protected abstract Task<bool> ValidateAsync(UserMessageInfo userMessageInfo, string argument);
    protected abstract Task HandleAsync(UserMessageInfo userMessageInfo, string argument);
}