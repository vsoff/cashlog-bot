namespace Cashlog.Core.Core.Services.Abstract
{
    public interface ICashlogSettingsService
    {
        /// <summary>
        /// Читает настройки из файла.
        /// </summary>
        CashlogSettings ReadSettings();

        /// <summary>
        /// Записывает настройки в файл.
        /// </summary>
        void WriteSettings(CashlogSettings settings);
    }
}