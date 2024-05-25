using Cashlog.Core;
using Cashlog.Core.Common;
using Cashlog.Core.Extensions;
using Cashlog.Core.Mappers;
using Cashlog.Core.Modules.Fns;
using Cashlog.Core.Modules.Fns.Models;
using Cashlog.Data;
using Cashlog.Data.Entities;
using Cashlog.Data.UoW;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Cashlog.Tests;

[TestClass]
public class FnsServiceTests
{
    [TestMethod]
    public async Task Test1()
    {
        var settings = new CashlogSettings
        {
            DataBaseConnectionString = "",
            DataProviderType = DataProviderType.MySql,
            FnsPhone = "",
            FnsPassword = ""
        };

        var i = 0;
        IFnsService fnsService = new FnsService(Mock.Of<ILogger>());
        IList<FnsReceiptDetailInfo> receiptDetailInfos = new List<FnsReceiptDetailInfo>();
        ReceiptDto[] allReceipts;
        using (var uow = new UnitOfWork(new ApplicationContext(settings.DataBaseConnectionString,
                   settings.DataProviderType)))
        {
            allReceipts = await uow.Receipts.GetAllAsync();
        }

        var checkingReceipts = allReceipts
            .Where(x => !string.IsNullOrEmpty(x.FiscalDocument))
            .OrderByDescending(x => x.Id)
            .Take(12);

        foreach (var receipt in checkingReceipts)
        {
            var receiptMainInfo = receipt.ToCore().ToReceiptMainInfo();
            if (receiptMainInfo.IsValid())
            {
                var data = await fnsService.GetReceiptAsync(receiptMainInfo, settings.FnsPhone, settings.FnsPassword);

                if (data != null)
                    receiptDetailInfos.Add(data);

                Task.Delay(TimeSpan.FromSeconds(2)).Wait();
            }

            i++;
        }
    }
}