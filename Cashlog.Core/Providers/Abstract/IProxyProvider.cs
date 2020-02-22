using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Cashlog.Core.Providers.Abstract
{
    /// <summary>
    /// Поставляет списки прокси серверов.
    /// </summary>
    public interface IProxyProvider
    {
        Task<ICollection<WebProxy>> GetProxiesAsync();
    }
}
