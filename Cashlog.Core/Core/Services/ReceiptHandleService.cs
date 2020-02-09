using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cashlog.Core.Core.Models;
using Cashlog.Core.Fns;
using Cashlog.Core.Fns.Models;
using ZXing;

namespace Cashlog.Core.Core.Services
{
    public class ReceiptHandleService : IReceiptHandleService
    {
        private readonly CashlogSettings _cashogSettings;

        public ReceiptHandleService(CashlogSettings cashogSettings)
        {
            _cashogSettings = cashogSettings ?? throw new ArgumentNullException(nameof(cashogSettings));
        }

        public async Task<QrCodeData> ParsePhotoAsync(Bitmap photo)
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
                var isAmountParsed = double.TryParse(values["s"].Replace('.', ','), out var amount);

                if (!date.HasValue || !isAmountParsed)
                    return null;

                return new QrCodeData
                {
                    Data = decodeResult.Text,
                    FiscalDocument = values["i"],
                    FiscalNumber = values["fn"],
                    FiscalSign = values["fp"],
                    PurchaseTime = date.Value,
                    TotalAmount = amount
                };
            }

            return null;
        }

        public async Task<ReceiptInfo> GetReceiptInfoAsync(QrCodeData data)
        {
            CheckFnsResult checkResult = await FnsManager.CheckAsync(data.FiscalNumber, data.FiscalDocument, data.FiscalSign, data.PurchaseTime, (decimal) data.TotalAmount);

            if (!checkResult.ReceiptExists)
                return null;

            ReceiptFnsResult fnsResult = await FnsManager.ReceiveAsync(data.FiscalNumber, data.FiscalDocument, data.FiscalSign, _cashogSettings.FnsPhone, _cashogSettings.FnsPassword);
            if (fnsResult.StatusCode == HttpStatusCode.Accepted)
                fnsResult = await FnsManager.ReceiveAsync(data.FiscalNumber, data.FiscalDocument, data.FiscalSign, _cashogSettings.FnsPhone, _cashogSettings.FnsPassword);

            if (!fnsResult.IsSuccess || fnsResult.StatusCode == HttpStatusCode.Accepted)
                return null;

            return new ReceiptInfo
            {
                PurchaseTime = data.PurchaseTime,
                FiscalSign = data.FiscalSign,
                FiscalDocument = data.FiscalDocument,
                FiscalNumber = data.FiscalNumber,
                TotalAmount = (double) fnsResult.Document.Receipt.TotalSum / 100,
                RetailAddress = fnsResult.Document.Receipt.RetailPlaceAddress,
                RetailInn = fnsResult.Document.Receipt.RetailInn,
                CompanyName = fnsResult.Document.Receipt.StoreName,
                CashierName = fnsResult.Document.Receipt.Cashier,
                Items = fnsResult.Document.Receipt.Items.Select(x => new ReceiptItem
                {
                    Name = x.Name,
                    Price = (double) x.Price / 100,
                    Quantity = x.Quantity
                }).ToArray()
            };
        }

        /// <summary>
        /// Парсит дату чека из определённого формата.
        /// </summary>
        private static DateTime? ParseReceiptDateTime(string checkDateTime)
        {
            const string defaultDatePattern =
                @"(?<year>\d{4})(?<month>\d{2})(?<day>\d{2})T(?<hours>\d{2})(?<minutes>\d{2})";

            Match match = Regex.Match(checkDateTime, defaultDatePattern);
            if (!match.Success)
                return null;

            var g = match.Groups;
            var newString = $"{g["year"]}.{g["month"]}.{g["day"]} {g["hours"]}:{g["minutes"]}";

            return DateTime.Parse(newString);
        }
    }
}