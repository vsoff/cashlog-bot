using System;
using System.Threading;
using Autofac;
using Cashlog.Core;
using Cashlog.Core.Common.Workers;
using Cashlog.Core.Ioc;
using Cashlog.Core.Services;
using Cashlog.Core.Services.Abstract;
using ILogger = Cashlog.Core.Common.ILogger;

namespace Cashlog.Application.Selfhost
{
    class Program
    {
        private static ManualResetEvent _resetEvent;
        private static IContainer _container;
        private static ILogger _logger;

        static void Main(string[] args)
        {
            _logger = LoggerModule.GetLogger();
            _logger.Info("Приложение запущено!");

            try
            {
                ISettingsService<CashlogSettings> settingsService = new CashlogSettingsService();
                var config = settingsService.ReadSettings();
                if (string.IsNullOrEmpty(config.TelegramBotToken))
                    throw new InvalidOperationException($"Поле `{nameof(config.TelegramBotToken)}` в конфиге пустое");

                WorkOn();

                _logger.Info("Приложение завершило свою работу");
            }
            catch (Exception ex)
            {
                _logger.Error("Во время работы приложения произошла ошибка", ex);
                throw;
            }
        }

        private static void WorkOn()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterModule<LoggerModule>();
            builder.RegisterModule<CashlogModule>();

            try
            {
                _container = builder.Build();

                // Запускаем все воркеры.
                var workers = _container.Resolve<IWorker[]>();
                foreach (var worker in workers)
                    worker.Start();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Произошла ошибка во время построения IoC контейнера", ex);
            }

            // Ожидаем команды на окончания работы приложения.
            _resetEvent = new ManualResetEvent(false);
            _resetEvent.WaitOne();

            try
            {
                // Уничтожаем все воркеры.
                var workers = _container.Resolve<IWorker[]>();
                foreach (var worker in workers)
                    worker.Dispose();

                // Уничтожаем контейнер.
                _container.Dispose();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Во время уничтожения IoC контейнера произошла ошибка", ex);
            }
        }
    }
}