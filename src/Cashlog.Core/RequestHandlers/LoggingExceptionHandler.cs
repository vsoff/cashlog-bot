using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace Cashlog.Core.RequestHandlers;

public class LoggingExceptionHandler<TRequest, TResponse, TException>
    : IRequestExceptionHandler<TRequest, TResponse, TException>
    where TRequest : notnull
    where TException : Exception
{
    private readonly ILogger<LoggingExceptionHandler<TRequest, TResponse, TException>> _logger;

    public LoggingExceptionHandler(ILogger<LoggingExceptionHandler<TRequest, TResponse, TException>> logger)
    {
        _logger = logger;
    }

    public Task Handle(TRequest request, TException exception, RequestExceptionHandlerState<TResponse> state,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Unexpected exception while handling request of type {RequestType}",
            typeof(TRequest));

        state.SetHandled(default!);

        return Task.CompletedTask;
    }
}