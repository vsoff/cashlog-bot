using System;
using System.IO;
using Newtonsoft.Json;

namespace Cashlog.Core.Services
{
    public abstract class SettingsService<T> : ISettingsService<T> where T : new()
    {
        protected abstract string ConfigFileName { get; }
        private string _jsonSettingsCache;

        protected SettingsService()
        {
            _jsonSettingsCache = null;
        }

        /// <summary>
        /// Возвращает полный путь до файла конфига.
        /// </summary>
        private string GetSettingsFullPath() => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);

        public T ReadSettings()
        {
            if (!File.Exists(GetSettingsFullPath()))
            {
                var settings = new T();
                WriteSettings(settings);
                return settings;
            }

            if (_jsonSettingsCache == null)
                _jsonSettingsCache = File.ReadAllText(GetSettingsFullPath());

            return JsonConvert.DeserializeObject<T>(_jsonSettingsCache);
        }

        public void WriteSettings(T settings)
        {
            var configJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(GetSettingsFullPath(), configJson);
        }
    }
}