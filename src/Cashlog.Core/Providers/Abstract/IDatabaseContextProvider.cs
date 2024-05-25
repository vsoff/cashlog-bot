using Cashlog.Data;

namespace Cashlog.Core.Providers.Abstract;

public interface IDatabaseContextProvider
{
    ApplicationContext Create();
}