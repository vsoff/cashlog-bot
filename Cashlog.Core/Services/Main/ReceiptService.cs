using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cashlog.Core.Mappers;
using Cashlog.Core.Models.Main;
using Cashlog.Core.Providers.Abstract;
using Cashlog.Core.Services.Abstract;
using Cashlog.Data.Entities;
using Cashlog.Data.UoW;

namespace Cashlog.Core.Services.Main
{
    public class ReceiptService : IReceiptService
    {
        private readonly IDatabaseContextProvider _databaseContextProvider;

        public ReceiptService(IDatabaseContextProvider databaseContextProvider)
        {
            _databaseContextProvider = databaseContextProvider ?? throw new ArgumentNullException(nameof(databaseContextProvider));
        }

        public async Task<ICollection<Receipt>> GetReceiptsInPeriodAsync(DateTime periodFrom, DateTime periodTo)
        {
            using (var uow = new UnitOfWork(_databaseContextProvider.Create()))
            {
                var receipts = await uow.Receipts.GetListAsync(x => x.PurchaseTime >= periodFrom && x.PurchaseTime < periodTo);
                return receipts.Select(x => x.ToCore()).ToList();
            }
        }

        public async Task<bool> IsReceiptExists(Receipt receipt)
        {
            using (var uow = new UnitOfWork(_databaseContextProvider.Create()))
            {
                return await IsReceiptExistsInternal(uow, receipt);
            }
        }

        public async Task<Receipt> AddAsync(Receipt receipt)
        {
            using (var uow = new UnitOfWork(_databaseContextProvider.Create()))
            {
                ICollection<ReceiptDto> existsReceipts = await uow.Receipts.GetListAsync(x => x.FiscalDocument == receipt.FiscalDocument);
                if (await IsReceiptExistsInternal(uow, receipt))
                    throw new InvalidOperationException("Такой чек уже есть в БД");

                ReceiptDto newReceipt = await uow.Receipts.AddAsync(receipt.ToData());
                await uow.SaveChangesAsync();
                return newReceipt.ToCore();
            }
        }

        public async Task<Receipt> GetAsync(long receiptId)
        {
            using (var uow = new UnitOfWork(_databaseContextProvider.Create()))
            {
                return (await uow.Receipts.GetAsync(receiptId))?.ToCore();
            }
        }

        public async Task<Receipt> UpdateAsync(Receipt receipt)
        {
            using (var uow = new UnitOfWork(_databaseContextProvider.Create()))
            {
                ReceiptDto newReceipt = await uow.Receipts.UpdateAsync(receipt.ToData());
                await uow.SaveChangesAsync();
                return newReceipt.ToCore();
            }
        }

        public async Task<Receipt[]> GetByBillingPeriodIdAsync(long billingPeriodId)
        {
            using (var uow = new UnitOfWork(_databaseContextProvider.Create()))
            {
                return (await uow.Receipts.GetByBillingPeriodIdAsync(billingPeriodId))?.Select(x => x.ToCore()).ToArray();
            }
        }

        public async Task SetCustomersToReceiptAsync(long receiptId, long[] consumerIds)
        {
            using (var uow = new UnitOfWork(_databaseContextProvider.Create()))
            {
                await uow.ReceiptConsumerMaps.AddRangeAsync(consumerIds.Select(x => new ReceiptConsumerMapDto
                {
                    ConsumerId = x,
                    ReceiptId = receiptId
                }));
                await uow.SaveChangesAsync();
            }
        }

        public async Task<Dictionary<long, long[]>> GetConsumerIdsByReceiptIdsMapAsync(long[] receiptIds)
        {
            using (var uow = new UnitOfWork(_databaseContextProvider.Create()))
            {
                return await uow.ReceiptConsumerMaps.GetConsumerIdsByReceiptIdsMapAsync(receiptIds);
            }
        }

        /// <summary>
        /// Возвращает true, если такой чек уже присутствует в БД.
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="receipt"></param>
        /// <returns></returns>
        private async Task<bool> IsReceiptExistsInternal(UnitOfWork uow, Receipt receipt)
        {
            if (string.IsNullOrEmpty(receipt.FiscalDocument))
                return false;

            ICollection<ReceiptDto> existsReceipts = await uow.Receipts.GetListAsync(x => x.FiscalDocument == receipt.FiscalDocument);
            return existsReceipts.Count > 0;
        }
    }
}