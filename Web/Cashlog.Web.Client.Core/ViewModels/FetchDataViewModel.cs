using System;
using System.Net.Http;
using System.Threading.Tasks;
using Cashlog.Web.Shared.Contracts;
using Microsoft.AspNetCore.Components;

namespace Cashlog.Web.Client.Core.ViewModels
{
    public class FetchDataViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;

        public double Val { get; set; }

        public FetchDataViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ReceiptWebModel[]> GetReceipts()
        {
            return await _httpClient.GetJsonAsync<ReceiptWebModel[]>("https://localhost:6001/Receipt");
        }
    }
}