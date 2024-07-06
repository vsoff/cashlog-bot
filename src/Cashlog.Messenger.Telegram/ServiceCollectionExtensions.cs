using Cashlog.Messenger.Menu;
using Cashlog.Messenger.Telegram.Menu;
using Microsoft.Extensions.DependencyInjection;

namespace Cashlog.Messenger.Telegram;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelegramMessenger(
        this IServiceCollection services)
    {
        services
            .AddOptions<TelegramOptions>()
            .BindConfiguration(TelegramOptions.SectionName)
            ;
        
        services
            .AddSingleton<IMenuProvider, TelegramMenuProvider>()
            .AddSingleton<IMessenger, TelegramMessenger>()
            ;
        
        return services;
    }
}