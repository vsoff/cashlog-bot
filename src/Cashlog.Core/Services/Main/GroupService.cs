using Cashlog.Core.Mappers;
using Cashlog.Core.Models.Main;
using Cashlog.Core.Services.Abstract;
using Cashlog.Data;
using Cashlog.Data.Entities;
using Cashlog.Data.UoW;
using Group = Cashlog.Data.Entities.Group;

namespace Cashlog.Core.Services.Main;

public class GroupService : IGroupService
{
    private readonly IDatabaseContextProvider _databaseContextProvider;

    public GroupService(IDatabaseContextProvider databaseContextProvider)
    {
        _databaseContextProvider =
            databaseContextProvider ?? throw new ArgumentNullException(nameof(databaseContextProvider));
    }

    public async Task<Models.Main.GroupDto> AddAsync(string chatToken, string adminToken, string chatName)
    {
        using (var uow = new UnitOfWork(_databaseContextProvider.Create()))
        {
            var newGroup = await uow.Groups.AddAsync(new Group
            {
                ChatToken = chatToken,
                AdminToken = adminToken,
                ChatName = chatName
            });
            await uow.SaveChangesAsync();
            return newGroup.ToCore();
        }
    }

    public async Task<Models.Main.GroupDto> GetByChatTokenAsync(string chatToken)
    {
        using (var uow = new UnitOfWork(_databaseContextProvider.Create()))
        {
            var group = await uow.Groups.GetByChatTokenAsync(chatToken);
            return group?.ToCore();
        }
    }
}