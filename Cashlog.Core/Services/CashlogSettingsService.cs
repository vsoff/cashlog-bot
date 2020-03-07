using System;
using System.IO;
using System.Reflection;
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

            var executingPath = AppDomain.CurrentDomain.BaseDirectory;
            var fullPath = Path.Combine(executingPath, BotConfigFileName);

            if (_jsonSettingsCache == null)
                _jsonSettingsCache = File.ReadAllText(fullPath);

            return JsonConvert.DeserializeObject<CashlogSettings>(_jsonSettingsCache);
        }

        public void WriteSettings(CashlogSettings settings)
        {
            var configJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(BotConfigFileName, configJson);
        }
    }
}