using Cashlog.Common.Models.Main;
using Cashlog.Data.Entities;
using Newtonsoft.Json;
using Receipt = Cashlog.Data.Entities.Receipt;

namespace Cashlog.Core.Mappers;

public static class ReceiptMapper
{
    public static ReceiptDto ToCore(this Receipt obj)
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
            GroupId = obj.GroupId,
            RetailAddress = obj.RetailAddress,
            RetailInn = obj.RetailInn,
            CompanyName = obj.CompanyName,
            CashierName = obj.CashierName,
        };
    }

    public static Receipt ToData(this ReceiptDto obj)
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
            GroupId = obj.GroupId,
            RetailAddress = obj.RetailAddress,
            RetailInn = obj.RetailInn,
            CompanyName = obj.CompanyName,
            CashierName = obj.CashierName,
        };
    }
}