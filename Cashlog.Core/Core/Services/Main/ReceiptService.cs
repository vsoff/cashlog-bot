using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cashlog.Core.Core.Models;
using Cashlog.Core.Core.Providers.Abstract;
using Cashlog.Core.Data.Mappers;
using Cashlog.Data.Entities;
using Cashlog.Data.UoW;

namespace Cashlog.Core.Core.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly IDatabaseContextProvider _databaseContextProvider;

        public ReceiptService(IDatabaseContextProvider databaseContextProvider)
        {
            _databaseContextProvider = databaseContextProvider ?? throw new ArgumentNullException(nameof(databaseContextProvider));
        }

        public async Task<Receipt> AddAsync(Receipt receipt)
        {
            using (var uow = new UnitOfWork(_databaseContextProvider.Create()))
            {
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
    }
}