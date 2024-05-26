using Cashlog.Core.Models;
using Cashlog.Core.Models.Main;
using Cashlog.Data.Entities;
using Newtonsoft.Json;

namespace Cashlog.Core.Mappers;

public static class ReceiptMapper
{
    public static Receipt ToCore(this ReceiptDto obj)
    {
        return new Receipt
        {
            Id = obj.Id,
            Comment = obj.Comment,
            BillingPeriodId = obj.BillingPeriodId,
            PurchaseTime = obj.PurchaseTime,
            TotalAmount = obj.TotalAmount,
            FiscalDocument = obj.FiscalDocument,
            FiscalNumber = obj.FiscalNumber,
            FiscalSign = obj.FiscalSign,
            Status = obj.Status,
            CustomerId = obj.CustomerId,
            RetailAddress = obj.RetailAddress,
            RetailInn = obj.RetailInn,
            CompanyName = obj.CompanyName,
            CashierName = obj.CashierName,
            Items = JsonConvert.DeserializeObject<ReceiptItem[]>(obj.ReceiptItemsJson)
        };
    }

    public static ReceiptDto ToData(this Receipt obj)
    {
        return new ReceiptDto
        {
            Id = obj.Id,
            Comment = obj.Comment,
            BillingPeriodId = obj.BillingPeriodId,
            PurchaseTime = obj.PurchaseTime,
            TotalAmount = obj.TotalAmount,
            FiscalDocument = obj.FiscalDocument,
            FiscalNumber = obj.FiscalNumber,
            FiscalSign = obj.FiscalSign,
            Status = obj.Status,
            CustomerId = obj.CustomerId,
            RetailAddress = obj.RetailAddress,
            RetailInn = obj.RetailInn,
            CompanyName = obj.CompanyName,
            CashierName = obj.CashierName,
            ReceiptItemsJson = JsonConvert.SerializeObject(obj.Items)
        };
    }

    public static ReceiptMainInfo ToReceiptMainInfo(this Receipt obj)
    {
        return new ReceiptMainInfo
        {
            RawData = null,
            PurchaseTime = obj.PurchaseTime,
            TotalAmount = obj.TotalAmount,
            FiscalDocument = obj.FiscalDocument,
            FiscalNumber = obj.FiscalNumber,
            FiscalSign = obj.FiscalSign
        };
    }
}