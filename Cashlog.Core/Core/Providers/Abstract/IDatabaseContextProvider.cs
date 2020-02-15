using Cashlog.Data;

namespace Cashlog.Core.Core.Providers.Abstract
{
    public interface IDatabaseContextProvider
    {
        ApplicationContext Create();
    }
}