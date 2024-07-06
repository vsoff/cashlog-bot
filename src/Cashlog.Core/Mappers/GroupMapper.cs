using Cashlog.Common.Models.Main;
using Cashlog.Data.Entities;
using Group = Cashlog.Data.Entities.Group;

namespace Cashlog.Core.Mappers;

public static class GroupMapper
{
    public static GroupDto ToCore(this Group obj)
    {
        return new GroupDto
        {
            AdminToken = obj.AdminToken,
            ChatName = obj.ChatName,
            ChatToken = obj.ChatToken,
            Id = obj.Id
        };
    }

    public static Group ToData(this GroupDto obj)
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