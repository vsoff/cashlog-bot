using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cashlog.Core.Common;
using Cashlog.Core.Providers.Abstract;
using Flurl.Http;
using HtmlAgilityPack;

namespace Cashlog.Core.Providers
{
    public class ProxyProvider : IProxyProvider
    {
        private static string _userAgent = @"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.103 Safari/537.36 OPR/60.0.3255.151 (Edition Yx)";
        private static readonly string _proxyListUrl = @"https://proxy-list.org/russian/search.php?search=ssl-yes&country=any&type=any&port=any&ssl=yes";
        private readonly ILogger _logger;

        public ProxyProvider(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ICollection<WebProxy>> GetProxiesAsync()
        {
            var response = await _proxyListUrl.WithHeaders(new { Accept = "text/plain", User_Agent = _userAgent }).GetAsync();
            var html = await response.Content.ReadAsStringAsync();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var httpNodes = doc.DocumentNode.SelectNodes("//li[@class=\"https\"]");
            var proxyNodes = doc.DocumentNode.SelectNodes("//li[@class=\"proxy\"]");

            if (httpNodes.Count != proxyNodes.Count)
            {
                _logger.Warning($"Количество элементов в {nameof(httpNodes)} и {nameof(proxyNodes)} было разное. Невозможно получить новый прокси адрес.");
                return new WebProxy[0];
            }

            const string base64Regex = "(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=|[A-Za-z0-9+/]{4})";

            List<string> proxies = new List<string>();
            for (int i = 0; i < proxyNodes.Count; i++)
            {
                Match match = Regex.Match(proxyNodes[i].InnerHtml, $"Proxy\\('(?<addressBase>{base64Regex})'\\)");
                if (!match.Success)
                    continue;

                var addressBase = match.Groups["addressBase"].Value;
                var proxy = Base64Decode(addressBase);

                const string proxyAddressRegex = "(\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}):(\\d{1,5})";
                if (Regex.IsMatch(proxy, proxyAddressRegex))
                    proxies.Add(proxy);
            }

            return proxies.Select(x => new WebProxy(x)).ToArray();
        }

        private string Base64Decode(string base64EncodedData)
        {
            byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}