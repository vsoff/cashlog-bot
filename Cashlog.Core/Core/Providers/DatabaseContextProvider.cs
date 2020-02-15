using System;
using Cashlog.Core.Core.Providers.Abstract;
using Cashlog.Core.Core.Services.Abstract;
using Cashlog.Data;

namespace Cashlog.Core.Core.Providers
{
    public class DatabaseContextProvider : IDatabaseContextProvider
    {
        private readonly ICashlogSettingsService _cashlogSettingsService;

        public DatabaseContextProvider(ICashlogSettingsService cashlogSettingsService)
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