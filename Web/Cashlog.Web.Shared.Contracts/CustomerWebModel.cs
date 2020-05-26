namespace Cashlog.Web.Shared.Contracts
{
    public class CustomerWebModel : IWebModel
    {
        public long Id { get; set; }
        public long GroupId { get; set; }
        public string Caption { get; set; }
        public bool IsDeleted { get; set; }
    }
}