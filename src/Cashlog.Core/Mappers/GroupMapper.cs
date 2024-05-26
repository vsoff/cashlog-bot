using Cashlog.Core.Models.Main;
using Cashlog.Data.Entities;
using Group = Cashlog.Data.Entities.Group;

namespace Cashlog.Core.Mappers;

public static class GroupMapper
{
    public static Models.Main.GroupDto ToCore(this Group obj)
    {
        return new Models.Main.GroupDto
        {
            AdminToken = obj.AdminToken,
            ChatName = obj.ChatName,
            ChatToken = obj.ChatToken,
            Id = obj.Id
        };
    }

    public static Group ToData(this Models.Main.GroupDto obj)
    {
        return new Group
        {
            AdminToken = obj.AdminToken,
            ChatName = obj.ChatName,
            ChatToken = obj.ChatToken,
            Id = obj.Id
        };
    }
}