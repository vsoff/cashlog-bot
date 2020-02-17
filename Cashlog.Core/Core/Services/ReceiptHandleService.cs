using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cashlog.Core.Core.Models;
using Cashlog.Core.Core.Providers;
using Cashlog.Core.Core.Services.Abstract;
using Cashlog.Core.Data.Mappers;
using Cashlog.Core.Fns;
using Cashlog.Core.Fns.Models;
using ZXing;

namespace Cashlog.Core.Core.Services
{
    public class ReceiptHandleService : IReceiptHandleService
    {
        private readonly ICashlogSettingsService _cashlogSettingsService;
        private readonly IFnsService _fnsService;

        public ReceiptHandleService(
            ICashlogSettingsService cashlogSettingsService,
            IFnsService fnsService)
        {
            _cashlogSettingsService = cashlogSettingsService ?? throw new ArgumentNullException(nameof(cashlogSettingsService));
            _fnsService = fnsService ?? throw new ArgumentNullException(nameof(fnsService));
        }

        public ReceiptMainInfo ParsePhoto(Bitmap photo)
        {
            byte[] byteArray;
            using (MemoryStream bitmapStream = new MemoryStream())
            {
                // Переводим изображение в BMP формат.
                photo.Save(bitmapStream, ImageFormat.Bmp);
                byteArray = bitmapStream.GetBuffer();
            }

            // Получаем LuminanceSource из байтов BMP. 
            LuminanceSource source = new RGBLuminanceSource(byteArray, photo.Width, photo.Height);

            // Получаем
            IBarcodeReader barcode = new BarcodeReader();
            var decodeResult = barcode.Decode(source);

            if (decodeResult?.BarcodeFormat == BarcodeFormat.QR_CODE)
            {
                Dictionary<string, string> values = decodeResult.Text.Split('&')
                    .ToDictionary(key => key.Split('=').First(), val => val.Split('=').Last());

                // Получаем дату.
                DateTime? date = ParseReceiptDateTime(values["t"]);

                // Получаем стоимость.
                var isAmountParsed = double.TryParse(values["s"].Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double amount);

                if (!date.HasValue || !isAmountParsed)
                    return null;

                return new ReceiptMainInfo
                {
                    RawData = decodeResult.Text,
                    FiscalDocument = values["i"],
                    FiscalNumber = values["fn"],
                    FiscalSign = values["fp"],
                    PurchaseTime = date.Value,
                    TotalAmount = amount
                };
            }

            return null;
        }

        public async Task<ReceiptInfo> GetReceiptInfoAsync(ReceiptMainInfo data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            bool receiptExists = await _fnsService.ReceiptExistsAsync(data);
            if (!receiptExists)
                return null;

            var settings = _cashlogSettingsService.ReadSettings();
            var detailInfo = await _fnsService.GetReceiptAsync(data, settings.FnsPhone, settings.FnsPassword);
            return detailInfo?.ToCore(data);
        }

        /// <summary>
        /// Парсит дату чека из определённого формата.
        /// </summary>
        private static DateTime? ParseReceiptDateTime(string checkDateTime)
        {
            const string defaultDatePattern = @"(?<year>\d{4})(?<month>\d{2})(?<day>\d{2})T(?<hours>\d{2})(?<minutes>\d{2})";

            Match match = Regex.Match(checkDateTime, defaultDatePattern);
            if (!match.Success)
                return null;

            var g = match.Groups;
            var newString = $"{g["year"]}.{g["month"]}.{g["day"]} {g["hours"]}:{g["minutes"]}";

            return DateTime.Parse(newString);
        }
    }
}