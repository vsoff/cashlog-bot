namespace Cashlog.Data.Entities
{
    public enum MoneyOperationTypeDto
    {
        /// <summary>
        /// Неизвестный тип операции.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Долг, расписанный с предыдущего месяца.
        /// </summary>
        ResultDebt = 1,

        /// <summary>
        /// Перевод денег.
        /// </summary>
        Transfer = 2
    }
}