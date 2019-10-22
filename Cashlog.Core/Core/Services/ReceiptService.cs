﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Cashlog.Core.Core.Models;
using Cashlog.Core.Data.Mappers;
using Cashlog.Data.Entities;
using Cashlog.Data.UoW;

namespace Cashlog.Core.Core.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly ICashogSettings _cashogSettings;

        public ReceiptService(ICashogSettings cashogSettings)
        {
            _cashogSettings = cashogSettings ?? throw new ArgumentNullException(nameof(cashogSettings));
        }

        public async Task<Receipt> AddAsync(Receipt receipt)
        {
            using (var uow = new UnitOfWork(_cashogSettings.DataBaseConnectionString))
            {
                ReceiptDto newReceipt = await uow.Receipts.AddAsync(receipt.ToData());
                await uow.SaveChangesAsync();
                return newReceipt.ToCore();
            }
        }

        public async Task<Receipt> GetAsync(long receiptId)
        {
            using (var uow = new UnitOfWork(_cashogSettings.DataBaseConnectionString))
            {
                return (await uow.Receipts.GetAsync(receiptId))?.ToCore();
            }
        }

        public async Task<Receipt> UpdateAsync(Receipt receipt)
        {
            using (var uow = new UnitOfWork(_cashogSettings.DataBaseConnectionString))
            {
                ReceiptDto newReceipt = await uow.Receipts.UpdateAsync(receipt.ToData());
                await uow.SaveChangesAsync();
                return newReceipt.ToCore();
            }
        }

        public async Task<Receipt[]> GetByGroupIdAsync(long groupId)
        {
            using (var uow = new UnitOfWork(_cashogSettings.DataBaseConnectionString))
            {
                return (await uow.Receipts.GetByGroupIdAsync(groupId))?.Select(x => x.ToCore()).ToArray();
            }
        }

        public async Task SetCustomersToReceiptAsync(long receiptId, long[] consumerIds)
        {
            using (var uow = new UnitOfWork(_cashogSettings.DataBaseConnectionString))
            {
                await uow.ReceiptConsumerMaps.AddRangeAsync(consumerIds.Select(x => new ReceiptConsumerMapDto
                {
                    ConsumerId = x,
                    ReceiptId = receiptId
                }));
                await uow.SaveChangesAsync();
            }
        }
    }
}