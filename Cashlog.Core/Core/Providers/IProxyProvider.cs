using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Cashlog.Core.Core.Providers
{
    /// <summary>
    /// Поставляет списки прокси серверов.
    /// </summary>
    public interface IProxyProvider
    {
        Task<ICollection<WebProxy>> GetProxiesAsync();
    }
}
