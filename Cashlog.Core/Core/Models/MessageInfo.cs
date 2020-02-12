namespace Cashlog.Core.Core.Models
{
    public class MessageInfo
    {
        public string Token { get; set; }
        public string Text { get; set; }
        public QrCodeData QrCode { get; set; }
    }
}