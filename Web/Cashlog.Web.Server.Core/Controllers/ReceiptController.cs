using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cashlog.Common;
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
        public async Task<IEnumerable<ReceiptWebModel>> GetAsync()
        {
            var receipts = await _receiptService.GetListAsync(new PartitionRequest(400, 1));
            return receipts.Select(x => x.ToModel());
        }
    }
}