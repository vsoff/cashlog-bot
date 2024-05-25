using System;
using Cashlog.Core.Providers.Abstract;
using Cashlog.Core.Services;
using Cashlog.Core.Services.Abstract;
using Cashlog.Data;

namespace Cashlog.Core.Providers
{
    public class BotDatabaseContextProvider : IDatabaseContextProvider
    {
        private readonly ISettingsService<CashlogSettings> _cashlogSettingsService;

        public BotDatabaseContextProvider(ISettingsService<CashlogSettings> cashlogSettingsService)
        {
            _cashlogSettingsService = cashlogSettingsService ?? throw new ArgumentNullException(nameof(cashlogSettingsService));
        }

        public ApplicationContext Create()
        {
            var settings = _cashlogSettingsService.ReadSettings();
            return new ApplicationContext(settings.DataBaseConnectionString, settings.DataProviderType);
        }
    }
}