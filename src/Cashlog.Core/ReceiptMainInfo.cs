namespace Cashlog.Core;

public class ReceiptMainInfo
{
    /// <summary>
    ///     Сырой вид данных, зашифроманных в QR коде чека.
    /// </summary>
    public string RawData { get; set; }

    /// <summary>
    ///     Фискальный номер, также известный как ФН. Номер состоит из 16 цифр.
    /// </summary>
    public string FiscalNumber { get; set; }

    /// <summary>
    ///     Номер фискального документа, также известный как ФД. Состоит максимум из 10 цифр.
    /// </summary>
    public string FiscalDocument { get; set; }

    /// <summary>
    ///     Фискальный признак документа, также известный как ФП, ФПД. Состоит максимум из 10 цифр.
    /// </summary>
    public string FiscalSign { get; set; }

    /// <summary>
    ///     Дата, указанная в чеке. Секунды не обязательные.
    /// </summary>
    public DateTime PurchaseTime { get; set; }

    /// <summary>
    ///     Сумма, указанная в чеке. Включая копейки.
    /// </summary>
    public double TotalAmount { get; set; }
}