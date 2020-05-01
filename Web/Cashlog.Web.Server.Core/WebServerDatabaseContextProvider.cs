using System;
using Cashlog.Core.Providers.Abstract;
using Cashlog.Core.Services;
using Cashlog.Data;

namespace Cashlog.Web.Server.Core
{
    public class WebServerDatabaseContextProvider : IDatabaseContextProvider
    {
        private readonly ISettingsService<WebServerSettings> _cashlogSettingsService;

        public WebServerDatabaseContextProvider(ISettingsService<WebServerSettings> cashlogSettingsService)
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