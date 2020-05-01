using System.Linq;
using Cashlog.Core.Models;
using Cashlog.Core.Models.Main;
using Cashlog.Web.Shared.Contracts;

namespace Cashlog.Web.Server.Core.Mappers
{
    public static class ReceiptMapper
    {
        public static ReceiptItemWebModel ToModel(this ReceiptItem source)
        {
            if (source == null) return null;
            return new ReceiptItemWebModel
            {
                Name = source.Name,
                Price = source.Price,
                Quantity = source.Quantity
            };
        }

        public static ReceiptWebModel ToModel(this Receipt source)
        {
            if (source == null) return null;
            return new ReceiptWebModel
            {
                Id = source.Id,
                Comment = source.Comment,
                PurchaseTime = source.PurchaseTime,
                TotalAmount = source.TotalAmount,
                FiscalDocument = source.FiscalDocument,
                FiscalNumber = source.FiscalNumber,
                FiscalSign = source.FiscalSign,
                Status = source.Status,
                CustomerId = source.CustomerId,
                BillingPeriodId = source.BillingPeriodId,
                RetailAddress = source.RetailAddress,
                RetailInn = source.RetailInn,
                CompanyName = source.CompanyName,
                CashierName = source.CashierName,
                Items = source.Items?.Select(x => x.ToModel()).ToArray(),
            };
        }
    }
}