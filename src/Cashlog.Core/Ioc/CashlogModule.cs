using Autofac;
using Cashlog.Core.Common.Workers;
using Cashlog.Core.Modules.Calculator;
using Cashlog.Core.Modules.Fns;
using Cashlog.Core.Modules.MessageHandlers;
using Cashlog.Core.Modules.MessageHandlers.Handlers;
using Cashlog.Core.Modules.Messengers;
using Cashlog.Core.Modules.Messengers.Menu;
using Cashlog.Core.Providers;
using Cashlog.Core.Providers.Abstract;
using Cashlog.Core.Services;
using Cashlog.Core.Services.Abstract;
using Cashlog.Core.Services.Main;

namespace Cashlog.Core.Ioc;

public class CashlogModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<CashlogSettingsService>().As<ISettingsService<CashlogSettings>>().SingleInstance();
        builder.RegisterType<FnsService>().As<IFnsService>().SingleInstance().AutoActivate();
        builder.RegisterType<BotDatabaseContextProvider>().As<IDatabaseContextProvider>().SingleInstance();
        builder.RegisterType<TelegramMessenger>().As<IMessenger>().SingleInstance().AutoActivate()
            .OnActivated(x => x.Instance.StartReceiving())
            .OnRelease(x => x.StopReceiving());
        builder.RegisterType<MessagesMainHandler>().AsSelf().SingleInstance().AutoActivate();
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

        // Регистрируем хендлеры текстовых команд.
        builder.RegisterType<SendMoneyMessagesHandler>().As<IMessageHandler>()
            .Named<SendMoneyMessagesHandler>(nameof(SendMoneyMessagesHandler)).SingleInstance();
        builder.RegisterType<CustomerMessagesHandler>().As<IMessageHandler>()
            .Named<CustomerMessagesHandler>(nameof(CustomerMessagesHandler)).SingleInstance();
        builder.RegisterType<ReceiptMessagesHandler>().As<IMessageHandler>()
            .Named<ReceiptMessagesHandler>(nameof(ReceiptMessagesHandler)).SingleInstance();
        builder.RegisterType<PeriodMessagesHandler>().As<IMessageHandler>()
            .Named<PeriodMessagesHandler>(nameof(PeriodMessagesHandler)).SingleInstance();
        builder.RegisterType<ReportMessagesHandler>().As<IMessageHandler>()
            .Named<ReportMessagesHandler>(nameof(ReportMessagesHandler)).SingleInstance();
        builder.RegisterType<DebtsMessagesHandler>().As<IMessageHandler>()
            .Named<DebtsMessagesHandler>(nameof(DebtsMessagesHandler)).SingleInstance();

        // Регистрируем хендлер фото-команд.
        builder.RegisterType<PhotoMessageHandler>().As<IMessageHandler>()
            .Named<PhotoMessageHandler>(nameof(PhotoMessageHandler)).SingleInstance();
    }
}