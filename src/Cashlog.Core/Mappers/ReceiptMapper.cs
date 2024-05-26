using Cashlog.Core.Models;
using Cashlog.Core.Models.Main;
using Cashlog.Data.Entities;
using Newtonsoft.Json;
using Receipt = Cashlog.Data.Entities.Receipt;

namespace Cashlog.Core.Mappers;

public static class ReceiptMapper
{
    public static Models.Main.ReceiptDto ToCore(this Receipt obj)
    {
        return new Models.Main.ReceiptDto
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
        };
    }

    public static Receipt ToData(this Models.Main.ReceiptDto obj)
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
        };
    }
}