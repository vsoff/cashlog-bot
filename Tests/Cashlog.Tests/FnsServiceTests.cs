using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cashlog.Core.Common;
using Cashlog.Core.Core;
using Cashlog.Core.Core.Extensions;
using Cashlog.Core.Data.Mappers;
using Cashlog.Core.Fns;
using Cashlog.Core.Fns.Models;
using Cashlog.Data;
using Cashlog.Data.Entities;
using Cashlog.Data.UoW;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Cashlog.Tests
{
    [TestClass]
    public class FnsServiceTests
    {
        [TestMethod]
        public async Task Test1()
        {
            CashlogSettings settings = new CashlogSettings
            {
                DataBaseConnectionString = "",
                DataProviderType = DataProviderType.MySql,
                FnsPhone = "",
                FnsPassword = ""
            };

            int i = 0;
            IFnsService fnsService = new FnsService(Mock.Of<ILogger>());
            IList<FnsReceiptDetailInfo> receiptDetailInfos = new List<FnsReceiptDetailInfo>();
            ReceiptDto[] allReceipts;
            using (var uow = new UnitOfWork(new ApplicationContext(settings.DataBaseConnectionString, settings.DataProviderType)))
                allReceipts = await uow.Receipts.GetAllAsync();

            var checkingReceipts = allReceipts
                .Where(x => !string.IsNullOrEmpty(x.FiscalDocument))
                .OrderByDescending(x => x.Id)
                .Take(12);

            foreach (var receipt in checkingReceipts)
            {
                var receiptMainInfo = receipt.ToCore().ToReceiptMainInfo();
                if (receiptMainInfo.IsValid())
                {
                    try
                    {
                        FnsReceiptDetailInfo data = await fnsService.GetReceiptAsync(receiptMainInfo, settings.FnsPhone, settings.FnsPassword);

                        if (data != null)
                            receiptDetailInfos.Add(data);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                    Task.Delay(TimeSpan.FromSeconds(2)).Wait();
                }

                i++;
            }
        }
    }
}