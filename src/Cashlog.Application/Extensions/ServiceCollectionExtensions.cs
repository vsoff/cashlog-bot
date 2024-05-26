using Cashlog.Application.HostedServices;
using Cashlog.Core.Modules.Calculator;
using Cashlog.Core.Modules.MessageHandlers;
using Cashlog.Core.Modules.MessageHandlers.Handlers;
using Cashlog.Core.Modules.Messengers;
using Cashlog.Core.Modules.Messengers.Menu;
using Cashlog.Core.Options;
using Cashlog.Core.Providers;
using Cashlog.Core.Services;
using Cashlog.Core.Services.Abstract;
using Cashlog.Core.Services.Main;
using Cashlog.Data;

namespace Cashlog.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCashlog(
        this IServiceCollection services,
        IConfiguration config)
    {
        services
            .AddCashlogOptions(config)
            .AddHandlers()

            // Data.
            .AddSingleton<IDatabaseContextProvider, BotDatabaseContextProvider>()

            // etc.
            .AddSingleton<IReceiptHandleService, ReceiptHandleService>()
            .AddSingleton<IQueryDataSerializer, QueryDataSerializer>()
            .AddSingleton<MessagesMainHandler>()

            // Telegram services.
            .AddSingleton<IMenuProvider, TelegramMenuProvider>()
            .AddSingleton<IMessenger, TelegramMessenger>()

            // Core logic services.
            .AddSingleton<IMoneyOperationService, MoneyOperationService>()
            .AddSingleton<IBillingPeriodService, BillingPeriodService>()
            .AddSingleton<IMainLogicService, MainLogicService>()
            .AddSingleton<IDebtsCalculator, DebtsCalculator>()
            .AddSingleton<ICustomerService, CustomerService>()
            .AddSingleton<IReceiptService, ReceiptService>()
            .AddSingleton<IGroupService, GroupService>()

            // Hosted services.
            .AddHostedService<MessagesHandlerHostedService>()
            .AddHostedService<MessengerHostedService>()
            ;

        return services;
    }

    private static IServiceCollection AddHandlers(
        this IServiceCollection services)
    {
        // Text command handlers.
        services
            .AddSingleton<IMessageHandler, SendMoneyMessagesHandler>()
            .AddSingleton<IMessageHandler, CustomerMessagesHandler>()
            .AddSingleton<IMessageHandler, ReceiptMessagesHandler>()
            .AddSingleton<IMessageHandler, PeriodMessagesHandler>()
            .AddSingleton<IMessageHandler, ReportMessagesHandler>()
            .AddSingleton<IMessageHandler, DebtsMessagesHandler>()

            // Photo command handlers.
            .AddSingleton<IMessageHandler, PhotoMessageHandler>()
            ;

        return services;
    }

    private static IServiceCollection AddCashlogOptions(
        this IServiceCollection services,
        IConfiguration config)
    {
        // TODO: Find solution to remove `Bind`, because it has problems with realtime update.
        services
            .Configure<CashlogOptions>(config.GetSection(CashlogOptions.SectionName).Bind)
            .Configure<DatabaseOptions>(config.GetSection(DatabaseOptions.SectionName).Bind);

        return services;
    }
}