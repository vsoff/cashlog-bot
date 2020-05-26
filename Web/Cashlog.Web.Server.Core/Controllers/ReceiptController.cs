using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Cashlog.Core.Services.Abstract;
using Cashlog.Data;
using Cashlog.Web.Server.Core.Mappers;
using Cashlog.Web.Shared.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Cashlog.Web.Server.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReceiptController : ControllerBase
    {
        private readonly IReceiptService _receiptService;

        public ReceiptController(IReceiptService receiptService)
        {
            _receiptService = receiptService ?? throw new ArgumentNullException(nameof(receiptService));
        }

        [HttpGet]
        public async Task<IEnumerable<ReceiptWebModel>> GetAsync(int? page, int? take = 50)
        {
            if (page == null) throw new ArgumentNullException(nameof(page));
            if (take == null) throw new ArgumentNullException(nameof(take));

            var receipts = await _receiptService.GetListAsync(new PartitionRequest(take.Value, page.Value));
            return receipts.Select(x => x.ToModel());
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        }

        [HttpPost]
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public async Task<IEnumerable<CustomerWebModel>> GetAsync(IEnumerable<long> customerIds)
        {
            if (customerIds == null) throw new ArgumentNullException(nameof(customerIds));
            if (!customerIds.Any()) throw new ArgumentOutOfRangeException(nameof(customerIds));

            var receipts = await _customerService.GetListAsync(customerIds.ToArray());
            return receipts.Select(x => x.ToModel());
        }
    }
}