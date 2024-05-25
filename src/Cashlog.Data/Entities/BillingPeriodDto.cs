namespace Cashlog.Data.Entities;

public class BillingPeriodDto : Entity
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

    public virtual GroupDto Group { get; set; }
    public virtual ICollection<ReceiptDto> Receipts { get; set; }
    public virtual ICollection<MoneyOperationDto> MoneyOperations { get; set; }
}