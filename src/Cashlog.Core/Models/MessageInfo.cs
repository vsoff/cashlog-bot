using Cashlog.Core.Modules.Messengers.Menu;

namespace Cashlog.Core.Models;

public class MessageInfo
{
    public required string Token { get; init; }
    public required string Text { get; init; }

    /// <summary>
    ///     Информация о чеке.
    /// </summary>
    /// <remarks>Поле содержится только в сообщении типа MessageType.Photo.</remarks>
    public ReceiptMainInfo? ReceiptInfo { get; set; }

    /// <summary>
    ///     Информация о запросе из меню.
    /// </summary>
    /// <remarks>Поле содержится только в сообщении типа MessageType.Query.</remarks>
    public IQueryData? QueryData { get; set; }
}