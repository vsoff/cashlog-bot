using System;
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

        /// <summary>
        /// Возвращает полный путь до файла конфига.
        /// </summary>
        private string GetSettingsFullPath() => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, BotConfigFileName);

        public CashlogSettings ReadSettings()
        {
            if (!File.Exists(GetSettingsFullPath()))
            {
                var settings = new CashlogSettings();
                WriteSettings(settings);
                return settings;
            }

            if (_jsonSettingsCache == null)
                _jsonSettingsCache = File.ReadAllText(GetSettingsFullPath());

            return JsonConvert.DeserializeObject<CashlogSettings>(_jsonSettingsCache);
        }

        public void WriteSettings(CashlogSettings settings)
        {
            var configJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(GetSettingsFullPath(), configJson);
        }
    }
}