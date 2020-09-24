using Cashlog.Core.Services;

namespace Cashlog.Web.Server.Core
{
    public class WebServerSettingsService : FileSettingsService<WebServerSettings>
    {
        protected override string ConfigFileName => "WebServerConfig.json";
    }
}