using System;
using System.Threading.Tasks;
using Cashlog.Core.Core;
using Cashlog.Data;
using Cashlog.Data.Entities;
using Cashlog.Data.UoW;

namespace Cashlog.Utils.DataMigrator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Нажмите enter, чтобы начать миграцию данных");
            Console.ReadLine();
            CashlogSettings settingsFrom = new CashlogSettings
            {
                DataBaseConnectionString = "",
                DataProviderType = DataProviderType.MsSql
            };
            CashlogSettings settingsTo = new CashlogSettings
            {
                DataBaseConnectionString = "",
                DataProviderType = DataProviderType.MySql
            };

            if (string.IsNullOrEmpty(settingsFrom.DataBaseConnectionString) || string.IsNullOrEmpty(settingsTo.DataBaseConnectionString))
            {
                Console.WriteLine("Ошибка! Необходимо указать подключение к БД для обоих источников данных");
            }
            else
            {
                try
                {
                    Console.WriteLine();

                    await MigrateObjects(x => x.Groups, settingsFrom, settingsTo);
                    await MigrateObjects(x => x.Customers, settingsFrom, settingsTo);
                    await MigrateObjects(x => x.BillingPeriods, settingsFrom, settingsTo);
                    await MigrateObjects(x => x.MoneyOperations, settingsFrom, settingsTo);
                    await MigrateObjects(x => x.Receipts, settingsFrom, settingsTo);
                    await MigrateObjects(x => x.ReceiptConsumerMaps, settingsFrom, settingsTo);

                    Console.WriteLine("Миграцию данных успешно завершена");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Во время миграции данных произошла ошибка: {ex.Message}:\n{ex.StackTrace}");
                }
            }

            Console.WriteLine("Нажмите enter, чтобы завершить приложение...");
            Console.ReadLine();
        }

        /// <summary>
        /// Производит миграцию объектов для определённого репозитория.
        /// </summary>
        /// <typeparam name="T">Типа данных в репозитории</typeparam>
        /// <param name="getTargetField">Функция-указатель на поле репозитория.</param>
        /// <param name="settingsFrom">Настройки сервера с которого происходит миграция данных.</param>
        /// <param name="settingsTo">Настройки сервера на который происходит миграция данных.</param>
        private static async Task MigrateObjects<T>(Func<UnitOfWork, IRepository<T>> getTargetField, CashlogSettings settingsFrom, CashlogSettings settingsTo) where T : Entity
        {
            Console.WriteLine($"Миграция данных для типа {typeof(T)}...");

            using var uowTo = new UnitOfWork(new ApplicationContext(settingsTo.DataBaseConnectionString, settingsTo.DataProviderType));
            var repositoryTo = getTargetField(uowTo);

            // Проверяем что табличка пустая.
            if (await repositoryTo.AnyAsync())
                throw new InvalidOperationException($"В репозитории типа {typeof(T)} уже есть данные");

            using var uowFrom = new UnitOfWork(new ApplicationContext(settingsFrom.DataBaseConnectionString, settingsFrom.DataProviderType));

            // Получаем все объекты.
            var data = await getTargetField(uowFrom).GetAllAsync();
            Console.WriteLine($"Получено {data.Length} объектов типа {typeof(T)}.");

            // Добавляем и сохраняем данные.
            await repositoryTo.AddRangeAsync(data);
            await uowTo.SaveChangesAsync();

            Console.WriteLine($"Миграция данных для типа {typeof(T)} закончена успешно!");
            Console.WriteLine();
        }
    }
}