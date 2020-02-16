using System;
using System.IO;
using Autofac;
using Cashlog.Core.Core;
using Cashlog.Core.Core.Providers;
using Cashlog.Core.Core.Providers.Abstract;
using Cashlog.Core.Core.Services;
using Cashlog.Core.Core.Services.Abstract;
using Cashlog.Core.Core.Services.Main;
using Cashlog.Core.Fns;
using Cashlog.Core.Messengers;
using Cashlog.Core.Messengers.Menu;
using Cashlog.Core.Modules.Calculator;
using Cashlog.Core.Modules.Messengers;
using Newtonsoft.Json;
using Serilog;
using ILogger = Cashlog.Core.Common.ILogger;

namespace Cashlog.Application.Selfhost
{
    class Program
    {
        /// <summary>
        /// Название файла с конфигурацией бота.
        /// </summary>
        private const string BotConfigFileName = "botconfig.json";

        private static IContainer _container;

        static void Main(string[] args)
        {
            ILogger logger = GetLogger();

            logger.Info("Приложение запущено!");

            if (!File.Exists(BotConfigFileName))
            {
                logger.Error($"Файл конфига {BotConfigFileName} не найден!");
                return;
            }

            ICashlogSettingsService settingsService = new CashlogSettingsService();
            var config = settingsService.ReadSettings();
            if (string.IsNullOrEmpty(config.TelegramBotToken))
            {
                logger.Error($"Поле `{nameof(config.TelegramBotToken)}` в конфиге пустое. Дальнейшее выполнение программы невозможно.");
                return;
            }

            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterInstance(settingsService);
            builder.RegisterInstance(logger);
            builder.RegisterType<FnsService>().As<IFnsService>().SingleInstance().AutoActivate();
            builder.RegisterType<DatabaseContextProvider>().As<IDatabaseContextProvider>().SingleInstance().AutoActivate();
            builder.RegisterType<TelegramMessenger>().As<IMessenger>().SingleInstance().AutoActivate();
            builder.RegisterType<MessagesHandler>().As<IMessagesHandler>().SingleInstance().AutoActivate();
            builder.RegisterType<ProxyProvider>().As<IProxyProvider>().SingleInstance();
            builder.RegisterType<QueryDataSerializer>().As<IQueryDataSerializer>().SingleInstance();
            builder.RegisterType<ReceiptHandleService>().As<IReceiptHandleService>().SingleInstance();
            builder.RegisterType<DebtsCalculator>().As<IDebtsCalculator>().SingleInstance();
            builder.RegisterType<MainLogicService>().As<IMainLogicService>().SingleInstance();
            builder.RegisterType<BillingPeriodService>().As<IBillingPeriodService>().SingleInstance();
            builder.RegisterType<MoneyOperationService>().As<IMoneyOperationService>().SingleInstance();
            builder.RegisterType<CustomerService>().As<ICustomerService>().SingleInstance();
            builder.RegisterType<ReceiptService>().As<IReceiptService>().SingleInstance();
            builder.RegisterType<TelegramMenuProvider>().As<IMenuProvider>().SingleInstance();
            builder.RegisterType<GroupService>().As<IGroupService>().SingleInstance();

            try
            {
                _container = builder.Build();
            }
            catch (Exception ex)
            {
                logger.Error("Произошла ошибка во время построения IoC контейнера", ex);
                throw;
            }

            Console.ReadLine();
        }

        private static ILogger GetLogger()
        {
            var serilogLogger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.RollingFile(@"logs\Main_{Date}.txt", outputTemplate: "{Timestamp:HH:mm:ss.fff zzz} [{Level}]: {Message}{NewLine}{Exception}")
                .CreateLogger();

            return new SerilogLogger(serilogLogger);
        }
    }
}