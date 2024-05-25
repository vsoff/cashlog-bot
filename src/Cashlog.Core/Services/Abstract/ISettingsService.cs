namespace Cashlog.Core.Services.Abstract
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