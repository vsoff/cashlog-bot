using System;
using Cashlog.Core.Providers.Abstract;
using Cashlog.Core.Services;
using Cashlog.Data;

namespace Cashlog.Web.Server.Core
{
    public class WebServerDatabaseContextProvider : IDatabaseContextProvider
    {
        private readonly ISettingsService<WebServerSettings> _settingsService;

        public WebServerDatabaseContextProvider(ISettingsService<WebServerSettings> settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

        public ApplicationContext Create()
        {
            var settings = _settingsService.ReadSettings();
            return new ApplicationContext(settings.DataBaseConnectionString, settings.DataProviderType);
        }
    }
}