using Cashlog.Core.Options;
using Cashlog.Core.Services.Abstract;
using Cashlog.Data;
using Microsoft.Extensions.Options;

namespace Cashlog.Core.Providers;

public class BotDatabaseContextProvider : IDatabaseContextProvider
{
    private readonly IOptions<DatabaseOptions> _databaseOptions;

    public BotDatabaseContextProvider(IOptions<DatabaseOptions> databaseOptions)
    {
        _databaseOptions = databaseOptions;
    }

    public ApplicationContext Create()
    {
        return new ApplicationContext(
            _databaseOptions.Value.DataBaseConnectionString,
            _databaseOptions.Value.DataProviderType);
    }
}