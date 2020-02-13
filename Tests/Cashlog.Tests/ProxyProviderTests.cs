﻿using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cashlog.Core.Common;
using Cashlog.Core.Core.Providers;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Match = System.Text.RegularExpressions.Match;

namespace Cashlog.Tests
{
    [TestClass]
    public class ProxyProviderTests
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            IProxyProvider proxyProvider = new ProxyProvider(Mock.Of<ILogger>());
            var proxies = await proxyProvider.GetProxiesAsync();

            Assert.IsNotNull(proxies);
            Assert.IsTrue(proxies.Count > 0, "Количество полученных прокси было равно нулю.");
        }
    }
}