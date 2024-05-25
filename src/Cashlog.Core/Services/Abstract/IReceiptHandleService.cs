using Cashlog.Core.Models;

namespace Cashlog.Core.Services.Abstract;

/// <summary>
///     Сервис для обработки чеков.
/// </summary>
public interface IReceiptHandleService
{
    /// <summary>
    ///     Парсит данные из QR код на фотографии и возвращает их. Если QR код не удалось распознать, тогда возвращает null.
    /// </summary>
    ReceiptMainInfo ParsePhoto(byte[] photoBytes);

    /// <summary>
    ///     Возвращает подробную информацию о чеке, по данным из QR кода.
    /// </summary>
    Task<ReceiptInfo> GetReceiptInfoAsync(ReceiptMainInfo data);
}