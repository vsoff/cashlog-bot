using Cashlog.Application.HostedServices;
using Cashlog.Core.Calculator;
using Cashlog.Core.MessageHandlers;
using Cashlog.Core.Options;
using Cashlog.Core.Providers;
using Cashlog.Core.RequestHandlers;
using Cashlog.Core.Services;
using Cashlog.Core.Services.Abstract;
using Cashlog.Core.Services.Main;
using Cashlog.Data;
using Cashlog.Messenger.Menu;
using Cashlog.Messenger.Telegram;
using MediatR.Pipeline;
using Serilog;
using Serilog.Events;

namespace Cashlog.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCashlogLogger(
        this IServiceCollection services,
        IHostBuilder host)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "logs", "log.txt"),
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        services
            .AddLogging()
            .AddSerilog()
            ;

        host
            .ConfigureLogging(logging =>
            {
                logging.AddSerilog();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .UseSerilog();

        return services;
    }

    public static IServiceCollection AddCashlog(
        this IServiceCollection services,
        IConfiguration config)
    {
        services
            .AddMediatorHandlers()
            .AddCashlogDatabase(config)
            .AddTelegramMessenger()
            .AddMessageHandlers()

            // etc.
            .AddSingleton<IReceiptHandleService, ReceiptHandleService>()
            .AddSingleton<IQueryDataSerializer, QueryDataSerializer>()

            // Core logic services.
            .AddSingleton<IMoneyOperationService, MoneyOperationService>()
            .AddSingleton<IBillingPeriodService, BillingPeriodService>()
            .AddSingleton<IMainLogicService, MainLogicService>()
            .AddSingleton<IDebtsCalculator, DebtsCalculator>()
            .AddSingleton<ICustomerService, CustomerService>()
            .AddSingleton<IReceiptService, ReceiptService>()
            .AddSingleton<IGroupService, GroupService>()

            // Hosted services.
            .AddHostedService<MessengerHostedService>()
            ;

        return services;
    }

    public static IServiceCollection AddCashlogDatabase(
        this IServiceCollection services,
        IConfiguration config)
    {
        services
            .AddSingleton<IDatabaseContextProvider, BotDatabaseContextProvider>()
            // TODO: Find solution to remove `Bind`, because it has problems with realtime update.
            .Configure<DatabaseOptions>(config.GetSection(DatabaseOptions.SectionName).Bind);

        return services;
    }

    private static IServiceCollection AddMediatorHandlers(
        this IServiceCollection services)
    {
        services
            .AddMediatR(options =>
            {
                options.RegisterServicesFromAssemblyContaining(typeof(LoggingExceptionHandler<,,>));
            })
            .AddTransient(typeof(IRequestExceptionHandler<,,>), typeof(LoggingExceptionHandler<,,>))
            ;

        return services;
    }
}