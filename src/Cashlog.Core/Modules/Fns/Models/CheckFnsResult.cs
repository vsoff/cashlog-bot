namespace Cashlog.Core.Modules.Fns.Models;

/// <summary>
///     Класс, представляющий ответ, полученный в результате проверки существования чека
/// </summary>
public sealed class CheckFnsResult : FnsResult
{
    internal CheckFnsResult()
    {
    }

    /// <summary>
    ///     Существует ли чек в базе ФНС?
    /// </summary>
    public bool ReceiptExists { get; internal set; }
}