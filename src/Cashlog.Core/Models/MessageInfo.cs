using Cashlog.Core.Modules.Messengers.Menu;

namespace Cashlog.Core.Models;

public class MessageInfo
{
    public string Token { get; set; }
    public string Text { get; set; }

    /// <summary>
    ///     Информация о чеке.
    /// </summary>
    /// <remarks>Поле содержится только в сообщении типа MessageType.Text.</remarks>
    public ReceiptMainInfo ReceiptInfo { get; set; }

    /// <summary>
    ///     Информация о запросе из меню.
    /// </summary>
    /// <remarks>Поле содержится только в сообщении типа MessageType.Query.</remarks>
    public IQueryData QueryData { get; set; }
}