namespace Cashlog.Core.Models.Main;

public class Customer
{
    public long Id { get; set; }
    public long GroupId { get; set; }
    public string Caption { get; set; }
    public bool IsDeleted { get; set; }
}