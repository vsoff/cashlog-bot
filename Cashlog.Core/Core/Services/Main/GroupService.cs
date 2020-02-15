﻿using System;
using System.Threading.Tasks;
using Cashlog.Core.Core.Models;
using Cashlog.Core.Core.Providers;
using Cashlog.Core.Core.Providers.Abstract;
using Cashlog.Core.Data.Mappers;
using Cashlog.Data.Entities;
using Cashlog.Data.UoW;

namespace Cashlog.Core.Core.Services
{
    public class GroupService : IGroupService
    {
        private readonly IDatabaseContextProvider _databaseContextProvider;

        public GroupService(IDatabaseContextProvider databaseContextProvider)
        {
            _databaseContextProvider = databaseContextProvider ?? throw new ArgumentNullException(nameof(databaseContextProvider));
        }

        public async Task<Group> AddAsync(string chatToken, string adminToken, string chatName)
        {
            using (var uow = new UnitOfWork(_databaseContextProvider.Create()))
            {
                GroupDto newGroup = await uow.Groups.AddAsync(new GroupDto
                {
                    ChatToken = chatToken,
                    AdminToken = adminToken,
                    ChatName = chatName,
                });
                await uow.SaveChangesAsync();
                return newGroup.ToCore();
            }
        }

        public async Task<Group> GetByChatTokenAsync(string chatToken)
        {
            using (var uow = new UnitOfWork(_databaseContextProvider.Create()))
            {
                GroupDto group = await uow.Groups.GetByChatTokenAsync(chatToken);
                return group?.ToCore();
            }
        }
    }
}