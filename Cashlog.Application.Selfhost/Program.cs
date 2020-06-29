using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Cashlog.Core;
using Cashlog.Core.Common.Workers;
using Cashlog.Core.Modules.Calculator;
using Cashlog.Core.Modules.Fns;
using Cashlog.Core.Modules.MessageHandlers;
using Cashlog.Core.Modules.Messengers;
using Cashlog.Core.Modules.Messengers.Menu;
using Cashlog.Core.Providers;
using Cashlog.Core.Providers.Abstract;
using Cashlog.Core.Services;
using Cashlog.Core.Services.Abstract;
using Cashlog.Core.Services.Main;
using Cashlog.Core.Workers;
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

        private static ManualResetEvent _resetEvent;
        private static IContainer _container;

        static void Main(string[] args)
        {
            ILogger logger = GetLogger();

            logger.Info("Приложение запущено!");

            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, BotConfigFileName)))
            {
                logger.Error($"Файл конфига {BotConfigFileName} не найден!");
                return;
            }

            ISettingsService<CashlogSettings> settingsService = new CashlogSettingsService();
            var config = settingsService.ReadSettings();
            if (string.IsNullOrEmpty(config.TelegramBotToken))
            {
                logger.Error($"Поле `{nameof(config.TelegramBotToken)}` в конфиге пустое. Дальнейшее выполнение программы невозможно.");
            }
            else
            {
                ContainerBuilder builder = new ContainerBuilder();
                builder.RegisterInstance(settingsService);
                builder.RegisterInstance(logger);
                builder.RegisterType<FnsService>().As<IFnsService>().SingleInstance().AutoActivate();
                builder.RegisterType<BotDatabaseContextProvider>().As<IDatabaseContextProvider>().SingleInstance();
                builder.RegisterType<TelegramMessenger>().As<IMessenger>().SingleInstance().AutoActivate()
                    .OnActivated(x => x.Instance.StartReceiving())
                    .OnRelease(x => x.StopReceiving());
                builder.RegisterType<MessagesMainHandler>().AsSelf().SingleInstance().AutoActivate();
                //builder.RegisterType<ProxyProvider>().As<IProxyProvider>().SingleInstance();
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
                builder.RegisterType<DefaultWorkerController>().As<IWorkerController>().SingleInstance();
                //builder.RegisterType<ProxyWorker>().As<IWorker>().SingleInstance();

                // Регистрируем хендлеры текстовых команд.
                builder.RegisterType<SendMoneyMessagesHandler>().As<IMessageHandler>().Named<SendMoneyMessagesHandler>(nameof(SendMoneyMessagesHandler)).SingleInstance();
                builder.RegisterType<CustomerMessagesHandler>().As<IMessageHandler>().Named<CustomerMessagesHandler>(nameof(CustomerMessagesHandler)).SingleInstance();
                builder.RegisterType<ReceiptMessagesHandler>().As<IMessageHandler>().Named<ReceiptMessagesHandler>(nameof(ReceiptMessagesHandler)).SingleInstance();
                builder.RegisterType<PeriodMessagesHandler>().As<IMessageHandler>().Named<PeriodMessagesHandler>(nameof(PeriodMessagesHandler)).SingleInstance();
                builder.RegisterType<ReportMessagesHandler>().As<IMessageHandler>().Named<ReportMessagesHandler>(nameof(ReportMessagesHandler)).SingleInstance();
                builder.RegisterType<DebtsMessagesHandler>().As<IMessageHandler>().Named<DebtsMessagesHandler>(nameof(DebtsMessagesHandler)).SingleInstance();

                // Регистрируем хендлер фото-команд.
                builder.RegisterType<PhotoMessageHandler>().As<IMessageHandler>().Named<PhotoMessageHandler>(nameof(PhotoMessageHandler)).SingleInstance();

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
                    logger.Error("Произошла ошибка во время построения IoC контейнера", ex);
                    throw;
                }
            }

            _resetEvent = new ManualResetEvent(false);
            _resetEvent.WaitOne();

            logger.Info("Приложение завершило свою работу");
        }


        private static ILogger GetLogger()
        {
            var fullPathFormat = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "Main_{Date}.txt");
            var serilogLogger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.RollingFile(fullPathFormat, outputTemplate: "{Timestamp:HH:mm:ss.fff zzz} [{Level}]: {Message}{NewLine}{Exception}")
                .CreateLogger();

            return new SerilogLogger(serilogLogger);
        }
    }
}