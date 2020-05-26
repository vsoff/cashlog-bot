using Cashlog.Web.Shared.Contracts;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Cashlog.Web.Client.Core.ViewModels
{
    public class ReceiptListItemModel
    {
        public ReceiptWebModel Receipt { get; set; }
        public string CustomerCaption { get; set; }
    }

    public class ReceiptsListViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;

        public ICollection<ReceiptListItemModel> Receipts { get; private set; }

        private int _itemsPageCount = 20;
        private int _currentPageIndex = 1;

        public double Val { get; set; }

        public bool IsPrevButtonDisabled { get; private set; }

        public ReceiptsListViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
            IsPrevButtonDisabled = true;
        }

        public async Task UpdateReceiptsList()
        {
            var receipts = await GetReceipts(_itemsPageCount, _currentPageIndex);
            var customers = await GetCustomers(receipts.Where(x => x.CustomerId.HasValue).Select(x => x.CustomerId.Value));
            var customersMap = customers.ToDictionary(x => x.Id, x => x);
            Receipts = receipts.Select(x =>
            {
                CustomerWebModel customer = null;
                if (x.CustomerId.HasValue)
                    customersMap.TryGetValue(x.CustomerId.Value, out customer);

                return new ReceiptListItemModel
                {
                    Receipt = x,
                    CustomerCaption = customer?.Caption
                };
            }).ToArray();

            IsPrevButtonDisabled = _currentPageIndex <= 1;
        }

        public async Task PrevPage()
        {
            _currentPageIndex = Math.Max(0, _currentPageIndex - 1);
            await UpdateReceiptsList();
        }

        public async Task NextPage()
        {
            _currentPageIndex++;
            await UpdateReceiptsList();
        }

        private async Task<ReceiptWebModel[]> GetReceipts(int take, int page)
        {
            return await _httpClient.GetJsonAsync<ReceiptWebModel[]>($"https://localhost:6001/receipt?take={take}&page={page}");
        }

        private async Task<CustomerWebModel[]> GetCustomers(IEnumerable<long> customerIds)
        {
            if (!customerIds.Any())
                return new CustomerWebModel[0];

            return await _httpClient.PostJsonAsync<CustomerWebModel[]>($"https://localhost:6001/customer", customerIds);
        }
    }
}