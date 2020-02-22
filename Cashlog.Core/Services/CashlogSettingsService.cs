using System.IO;
using Cashlog.Core.Services.Abstract;
using Newtonsoft.Json;

namespace Cashlog.Core.Services
{
    public class CashlogSettingsService : ICashlogSettingsService
    {
        private const string BotConfigFileName = "botconfig.json";
        private string _jsonSettingsCache;

        public CashlogSettingsService()
        {
            _jsonSettingsCache = null;
        }

        public CashlogSettings ReadSettings()
        {
            if (!File.Exists(BotConfigFileName))
            {
                var settings = new CashlogSettings();
                WriteSettings(settings);
                return settings;
            }

            if (_jsonSettingsCache == null)
                _jsonSettingsCache = File.ReadAllText(BotConfigFileName);

            return JsonConvert.DeserializeObject<CashlogSettings>(_jsonSettingsCache);
        }

        public void WriteSettings(CashlogSettings settings)
        {
            var configJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(BotConfigFileName, configJson);
        }
    }
}