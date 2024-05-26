using Cashlog.Core.Models.Main;

namespace Cashlog.Core.Models;

public class UserMessageInfo
{
    public GroupDto Group { get; set; }
    public CustomerDto[] Customers { get; set; }
    public string UserName { get; set; }
    public string UserToken { get; set; }
    public MessageInfo Message { get; set; }
    public MessageType MessageType { get; set; }
}