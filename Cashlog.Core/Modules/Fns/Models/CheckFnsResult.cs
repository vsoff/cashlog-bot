namespace Cashlog.Core.Fns.Models
{
    /// <summary>
    /// Класс, представляющий ответ, полученный в результате проверки существования чека
    /// </summary>
    public sealed class CheckFnsResult : FnsResult
    {
        /// <summary>
        /// Существует ли чек в базе ФНС?
        /// </summary>
        public bool ReceiptExists { get; internal set; }

        internal CheckFnsResult()
        {
        }
    }
}