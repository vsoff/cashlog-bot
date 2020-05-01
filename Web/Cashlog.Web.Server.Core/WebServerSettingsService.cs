using Cashlog.Core.Services;

namespace Cashlog.Web.Server.Core
{
    public class WebServerSettingsService : SettingsService<WebServerSettings>
    {
        protected override string ConfigFileName => "WebServerConfig.json";
    }
}