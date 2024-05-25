using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Text.RegularExpressions;
using Cashlog.Core.Mappers;
using Cashlog.Core.Models;
using Cashlog.Core.Modules.Fns;
using Cashlog.Core.Services.Abstract;
using ZXing;

namespace Cashlog.Core.Services;

public class ReceiptHandleService : IReceiptHandleService
{
    private readonly ISettingsService<CashlogSettings> _cashlogSettingsService;
    private readonly IFnsService _fnsService;

    public ReceiptHandleService(
        ISettingsService<CashlogSettings> cashlogSettingsService,
        IFnsService fnsService)
    {
        _cashlogSettingsService =
            cashlogSettingsService ?? throw new ArgumentNullException(nameof(cashlogSettingsService));
        _fnsService = fnsService ?? throw new ArgumentNullException(nameof(fnsService));
    }

    public ReceiptMainInfo ParsePhoto(byte[] photoBytes)
    {
        // Получаем изображение из потока.
        byte[] byteArray;
        Bitmap returnImage;
        try
        {
            using var clientStream = new MemoryStream(photoBytes);
            returnImage = (Bitmap)Image.FromStream(clientStream);

            using var bitmapStream = new MemoryStream();
            returnImage.Save(bitmapStream, ImageFormat.Bmp);
            byteArray = bitmapStream.GetBuffer();
        }
        catch (Exception ex)
        {
            throw new Exception("Ошибка во время получения изображения из потока", ex);
        }

        // Получаем LuminanceSource из байтов BMP. 
        LuminanceSource source = new RGBLuminanceSource(byteArray, returnImage.Width, returnImage.Height);

        // Декодируем QR код.
        IBarcodeReader barcode = new BarcodeReader();
        var decodeResult = barcode.Decode(source);

        if (decodeResult?.BarcodeFormat == BarcodeFormat.QR_CODE)
        {
            var values = decodeResult.Text.Split('&')
                .ToDictionary(key => key.Split('=').First(), val => val.Split('=').Last());

            // Должен содержать все эти ключи.
            if (!(values.ContainsKey("t")
                  && values.ContainsKey("s")
                  && values.ContainsKey("i")
                  && values.ContainsKey("fn")
                  && values.ContainsKey("fp")))
                return null;

            // Получаем дату.
            var date = ParseReceiptDateTime(values["t"]);

            // Получаем стоимость.
            var isAmountParsed = double.TryParse(
                values["s"].Replace(",", "."),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var amount);

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

        var receiptExists = await _fnsService.ReceiptExistsAsync(data);
        if (!receiptExists)
            return null;

        var settings = _cashlogSettingsService.ReadSettings();
        var detailInfo = await _fnsService.GetReceiptAsync(data, settings.FnsPhone, settings.FnsPassword);
        return detailInfo?.ToCore(data);
    }

    /// <summary>
    ///     Парсит дату чека из определённого формата.
    /// </summary>
    private static DateTime? ParseReceiptDateTime(string checkDateTime)
    {
        const string defaultDatePattern =
            @"(?<year>\d{4})(?<month>\d{2})(?<day>\d{2})T(?<hours>\d{2})(?<minutes>\d{2})";

        var match = Regex.Match(checkDateTime, defaultDatePattern);
        if (!match.Success)
            return null;

        var g = match.Groups;
        var newString = $"{g["year"]}.{g["month"]}.{g["day"]} {g["hours"]}:{g["minutes"]}";

        return DateTime.Parse(newString);
    }
}