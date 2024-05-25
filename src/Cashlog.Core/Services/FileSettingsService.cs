using Cashlog.Core.Services.Abstract;
using Newtonsoft.Json;

namespace Cashlog.Core.Services;

public abstract class FileSettingsService<T> : ISettingsService<T> where T : new()
{
    private string _jsonSettingsCache;

    protected FileSettingsService()
    {
        _jsonSettingsCache = null;
    }

    protected abstract string ConfigFileName { get; }

    public T ReadSettings()
    {
        if (_jsonSettingsCache == null)
        {
            if (!File.Exists(GetSettingsFullPath()))
                throw new InvalidOperationException($"Не удалось прочитать конфиг из файла `{ConfigFileName}`");

            _jsonSettingsCache = File.ReadAllText(GetSettingsFullPath());
        }

        if (_jsonSettingsCache == null)
        {
            var settings = new T();
            WriteSettings(settings);
            return settings;
        }

        return JsonConvert.DeserializeObject<T>(_jsonSettingsCache);
    }

    public void WriteSettings(T settings)
    {
        var configJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
        File.WriteAllText(GetSettingsFullPath(), configJson);
    }

    /// <summary>
    ///     Возвращает полный путь до файла конфига.
    /// </summary>
    private string GetSettingsFullPath()
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
    }
}