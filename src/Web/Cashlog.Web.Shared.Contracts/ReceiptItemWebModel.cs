namespace Cashlog.Web.Shared.Contracts
{
    public class ReceiptItemWebModel : IWebModel
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public double Quantity { get; set; }
    }
}