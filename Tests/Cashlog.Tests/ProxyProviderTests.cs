﻿using System.Threading.Tasks;
using Cashlog.Core.Common;
using Cashlog.Core.Core.Providers;
using Cashlog.Core.Core.Providers.Abstract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
