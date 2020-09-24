using Cashlog.Core.Services.Abstract;

namespace Cashlog.Core.Services
{
    public class CashlogSettingsService : FileSettingsService<CashlogSettings>
    {
        protected override string ConfigFileName => "botconfig.json";
    }
}