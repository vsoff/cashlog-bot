using Cashlog.Core.MessageHandlers.Handlers;
using Cashlog.Core.MessageHandlers.Handlers.Photo;
using Cashlog.Core.MessageHandlers.Handlers.Text;
using Cashlog.Core.MessageHandlers.RequestHandlers;
using Cashlog.Messenger;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Cashlog.Core.MessageHandlers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessageHandlers(
        this IServiceCollection services)
    {
        // Text command handlers.
        services
            .AddMediatorHandlers()
            .AddSingleton<IMessagesMainHandler, MessagesMainHandler>()

            // Text command handlers.
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

    private static IServiceCollection AddMediatorHandlers(
        this IServiceCollection services)
    {
        services
            .AddTransient<IPipelineBehavior<ConsumeUserMessageRequest, Unit>, UserIdentifyBehavior>()
            .AddTransient<IPipelineBehavior<ConsumeUserMessageRequest, Unit>, DecodeImageBehavior>()
            .AddTransient<IRequestHandler<ConsumeUserMessageRequest, Unit>, ConsumeUserMessageRequestHandler>();

        return services;
    }
}