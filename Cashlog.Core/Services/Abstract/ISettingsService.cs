namespace Cashlog.Core.Services
{
    /// <summary>
    /// Сервис управления настройками.
    /// </summary>
    public interface ISettingsService<T>
    {
        /// <summary>
        /// Читает настройки из файла.
        /// </summary>
        T ReadSettings();

        /// <summary>
        /// Записывает настройки в файл.
        /// </summary>
        void WriteSettings(T settings);
    }
}