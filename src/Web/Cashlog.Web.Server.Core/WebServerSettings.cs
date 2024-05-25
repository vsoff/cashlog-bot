using Cashlog.Data;

namespace Cashlog.Web.Server.Core
{
    public class WebServerSettings
    {
        public string DataBaseConnectionString { get; set; }
        public DataProviderType DataProviderType { get; set; }
    }
}