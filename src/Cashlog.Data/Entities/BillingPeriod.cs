namespace Cashlog.Data.Entities;

public class BillingPeriod : Entity
{
    public long GroupId { get; set; }

    /// <summary>
    ///     Время начала расчётного периода.
    /// </summary>
    public DateTime PeriodBegin { get; set; }

    /// <summary>
    ///     Время окончания расчётного периода.
    /// </summary>
    public DateTime? PeriodEnd { get; set; }

    public virtual Group Group { get; set; }
    public virtual ICollection<Receipt> Receipts { get; set; }
    public virtual ICollection<MoneyOperation> MoneyOperations { get; set; }
}