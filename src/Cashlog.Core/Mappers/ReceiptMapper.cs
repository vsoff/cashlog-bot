using System.Linq;
using Cashlog.Common;
using Cashlog.Core.Models;
using Cashlog.Core.Models.Main;
using Cashlog.Core.Modules.Fns.Models;
using Cashlog.Data.Entities;
using Newtonsoft.Json;

namespace Cashlog.Core.Mappers
{
    public static class FnsMapper
    {
        public static ReceiptInfo ToCore(this FnsReceiptDetailInfo detailInfo, ReceiptMainInfo receiptMainInfo)
        {
            return new ReceiptInfo
            {
                PurchaseTime = receiptMainInfo.PurchaseTime,
                FiscalSign = receiptMainInfo.FiscalSign,
                FiscalDocument = receiptMainInfo.FiscalDocument,
                FiscalNumber = receiptMainInfo.FiscalNumber,
                TotalAmount = (double) detailInfo.TotalSum / 100,
                RetailAddress = detailInfo.RetailPlaceAddress,
                RetailInn = detailInfo.RetailInn,
                CompanyName = detailInfo.StoreName,
                CashierName = detailInfo.Cashier,
                Items = detailInfo.Items.Select(x => x.ToCore()).ToArray()
            };
        }

        public static ReceiptItem ToCore(this Item obj)
        {
            return new ReceiptItem
            {
                Name = obj.Name,
                Price = (double) obj.Price / 100,
                Quantity = obj.Quantity
            };
        }
    }

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
                Items = JsonConvert.DeserializeObject<ReceiptItem[]>(obj.ReceiptItemsJson),
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
                ReceiptItemsJson = JsonConvert.SerializeObject(obj.Items),
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
                FiscalSign = obj.FiscalSign,
            };
        }
    }
}