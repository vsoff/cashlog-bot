namespace Cashlog.Core.Models
{
    public enum MoneyOperationType
    {
        /// <summary>
        /// Неизвестный тип операции.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Долг.
        /// </summary>
        Debt = 1,

        /// <summary>
        /// Перевод денег.
        /// </summary>
        Transfer = 2
    }
}