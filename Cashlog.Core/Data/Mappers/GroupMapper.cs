using System;
using System.Collections.Generic;
using System.Text;
using Cashlog.Core.Core.Models;
using Cashlog.Data.Entities;

namespace Cashlog.Core.Data.Mappers
{
    public static class GroupMapper
    {
        public static Group ToCore(this GroupDto obj)
        {
            return new Group
            {
                AdminToken = obj.AdminToken,
                ChatName = obj.ChatName,
                ChatToken = obj.ChatToken,
                Id = obj.Id
            };
        }

        public static GroupDto ToData(this Group obj)
        {
            return new GroupDto
            {
                AdminToken = obj.AdminToken,
                ChatName = obj.ChatName,
                ChatToken = obj.ChatToken,
                Id = obj.Id
            };
        }
    }
}