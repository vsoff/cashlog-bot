using Cashlog.Core.Common.Workers;
using Cashlog.Core.Logging;
using Cashlog.Core.Modules.Calculator;
using Cashlog.Core.Modules.MessageHandlers;
using Cashlog.Core.Modules.MessageHandlers.Handlers;
using Cashlog.Core.Modules.Messengers;
using Cashlog.Core.Modules.Messengers.Menu;
using Cashlog.Core.Options;
using Cashlog.Core.Providers;
using Cashlog.Core.Providers.Abstract;
using Cashlog.Core.Services;
using Cashlog.Core.Services.Abstract;
using Cashlog.Core.Services.Main;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Cashlog.Application.Extensions;

public static class ServiceCollectionExtensions
{
    [Obsolete("Should be removed. Use microsoft logger")]
    public static IServiceCollection AddOldLogger(
        this IServiceCollection services)
    {
        var fullPathFormat = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "Main_{Date}.txt");
        var serilogLogger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.RollingFile(fullPathFormat,
                outputTemplate: "{Timestamp:HH:mm:ss.fff zzz} [{Level}]: {Message}{NewLine}{Exception}")
            .CreateLogger();

        var logger = new SerilogLogger(serilogLogger);

        services.AddSingleton<Cashlog.Core.Common.ILogger>(logger);
        return services;
    }
    

    public static IServiceCollection AddCashlog(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddCashlogOptions(config);
        services.AddHandlers(config);

        // Data.
        services.AddSingleton<IDatabaseContextProvider, BotDatabaseContextProvider>();
        
        // etc.
        services.AddSingleton<IReceiptHandleService, ReceiptHandleService>();
        services.AddSingleton<IWorkerController, DefaultWorkerController>();
        services.AddSingleton<IQueryDataSerializer, QueryDataSerializer>();
        services.AddSingleton<MessagesMainHandler>();

        // Telegram services.
        services.AddSingleton<IMenuProvider, TelegramMenuProvider>();
        services.AddSingleton<IMessenger, TelegramMessenger>();
        
        // Core logic services.
        services.AddSingleton<IMoneyOperationService, MoneyOperationService>();
        services.AddSingleton<IBillingPeriodService, BillingPeriodService>();
        services.AddSingleton<IMainLogicService, MainLogicService>();
        services.AddSingleton<IDebtsCalculator, DebtsCalculator>();
        services.AddSingleton<ICustomerService, CustomerService>();
        services.AddSingleton<IReceiptService, ReceiptService>();
        services.AddSingleton<IGroupService, GroupService>();

        return services;
    }

    private static IServiceCollection AddHandlers(
        this IServiceCollection services,
        IConfiguration config)
    {
        // Text command handlers.
        services.AddSingleton<IMessageHandler, SendMoneyMessagesHandler>();
        services.AddSingleton<IMessageHandler, CustomerMessagesHandler>();
        services.AddSingleton<IMessageHandler, ReceiptMessagesHandler>();
        services.AddSingleton<IMessageHandler, PeriodMessagesHandler>();
        services.AddSingleton<IMessageHandler, ReportMessagesHandler>();
        services.AddSingleton<IMessageHandler, DebtsMessagesHandler>();
        
        // Photo command handlers.
        services.AddSingleton<IMessageHandler, PhotoMessageHandler>();

        return services;
    }

    private static IServiceCollection AddCashlogOptions(
        this IServiceCollection services,
        IConfiguration config)
    {
        // TODO: Find solution to remove `Bind`, because it has problems with realtime update.
        services.Configure<CashlogOptions>(config.GetSection(CashlogOptions.SectionName).Bind);
        services.Configure<DatabaseOptions>(config.GetSection(DatabaseOptions.SectionName).Bind);

        return services;
    }
}