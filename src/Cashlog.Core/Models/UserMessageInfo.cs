using Cashlog.Core.Models.Main;

namespace Cashlog.Core.Models;

public class UserMessageInfo
{
    public Group Group { get; set; }
    public Customer[] Customers { get; set; }
    public string UserName { get; set; }
    public string UserToken { get; set; }
    public MessageInfo Message { get; set; }
    public MessageType MessageType { get; set; }
}