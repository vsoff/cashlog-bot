using System.Drawing;
using System.Threading.Tasks;
using Cashlog.Core.Core.Models;

namespace Cashlog.Core.Core.Services
{
    public interface IReceiptHandleService
    {
        /// <summary>
        /// Парсит данные из QR код на фотографии и возвращает их. Если QR код не удалось распознать, тогда возвращает null.
        /// </summary>
        QrCodeData ParsePhoto(Bitmap photo);

        /// <summary>
        /// Возвращает подробную информацию о чеке, по данным из QR кода. 
        /// </summary>
        Task<ReceiptInfo> GetReceiptInfoAsync(QrCodeData data);
    }
}