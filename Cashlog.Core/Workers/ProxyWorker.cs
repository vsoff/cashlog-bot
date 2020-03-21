using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Cashlog.Core.Common;
using Cashlog.Core.Common.Workers;
using Cashlog.Core.Providers.Abstract;
using Cashlog.Core.Services.Abstract;

namespace Cashlog.Core.Workers
{
    /// <summary>
    /// Воркер, который периодически проверяет работоспособность прокси и уведомляет об этом потребителей.
    /// </summary>
    public class ProxyWorker : IWorker
    {
        // TODO Вынести в конфиг.
        private static string _userAgent = @"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.103 Safari/537.36 OPR/60.0.3255.151 (Edition Yx)";

        private readonly TimeSpan _proxyRevalidateInterval = TimeSpan.FromMinutes(5);

        private readonly ICashlogSettingsService _cashlogSettingsService;
        private readonly IProxyProvider _proxyProvider;
        private readonly ILogger _logger;
        private readonly IProxyConsumer[] _proxyConsumers;

        /// <summary>
        /// Кешированный список прокси из последнего запроса.
        /// </summary>
        private Queue<WebProxy> _cachedProxies;

        /// <summary>
        /// Воркер обновления актуальных прокси.
        /// </summary>
        private readonly IWorker _worker;

        public ProxyWorker(
            ICashlogSettingsService cashlogSettingsService,
            IWorkerController workerController,
            IProxyProvider proxyProvider,
            ILogger logger,
            IProxyConsumer[] proxyConsumers)
        {
            _cashlogSettingsService = cashlogSettingsService ?? throw new ArgumentNullException(nameof(cashlogSettingsService));
            _proxyProvider = proxyProvider ?? throw new ArgumentNullException(nameof(proxyProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _proxyConsumers = proxyConsumers ?? throw new ArgumentNullException(nameof(proxyConsumers));
            _cachedProxies = new Queue<WebProxy>();

            _worker = workerController.CreateWorker(() => RevalidateProxy().GetAwaiter().GetResult(), _proxyRevalidateInterval, true);
        }

        private async Task RevalidateProxy()
        {
            var client = new WebClient();
            _logger.Trace($"{GetType().Name}: Запущена проверка работоспособности прокси-сервера...");

            // Проверяем работоспособность текущего прокси-сервера.
            var settings = _cashlogSettingsService.ReadSettings();
            var currentProxy = string.IsNullOrEmpty(settings.ProxyAddress) ? null : new WebProxy(settings.ProxyAddress);
            if (IsProxyWorks(currentProxy, client))
            {
                _logger.Trace($"{GetType().Name}: Текущий прокси-сервер работает!");
                SetNewProxy(currentProxy, null);
                return;
            }

            _logger.Trace($"{GetType().Name}: Текущий прокси-сервер больше не работает!");

            // Иначе, проверяем закешированные прокси сервера.
            _logger.Trace($"{GetType().Name}: Проверяем закешированные прокси-сервера. Количество серверов: {_cachedProxies.Count}");
            var workingCachedProxy = GetWorksProxies(_cachedProxies, client).FirstOrDefault();
            if (workingCachedProxy != null)
            {
                SetNewProxy(workingCachedProxy, currentProxy);
                return;
            }

            // Среди закешированных нету работающего. Получаем прокси сервера от провайдера.
            _logger.Trace($"{GetType().Name}: Получаем прокси-сервера от провайдера...");
            var proxies = await _proxyProvider.GetProxiesAsync();
            if (proxies == null || proxies.Count == 0)
            {
                _logger.Warning($"{GetType().Name}: От провадйера не было получено ни одного прокси-сервера");
                SetNewProxy(null, currentProxy);
                return;
            }

            _logger.Trace($"{GetType().Name}: От провадйера было получено {proxies.Count} прокси-серверов");

            // Проверяем прокси, полученные от провайдера.
            var newProxies = new Queue<WebProxy>(proxies);
            var workingProxy = GetWorksProxies(newProxies, client).FirstOrDefault();
            SetNewProxy(workingProxy, currentProxy);

            _cachedProxies = newProxies;
        }

        /// <summary>
        /// Возвращает только работающие прокси из <see cref="Queue{T}"/>.
        /// </summary>
        private IEnumerable<WebProxy> GetWorksProxies(Queue<WebProxy> proxiesQueue, WebClient client)
        {
            while (proxiesQueue.TryDequeue(out var cachedProxy))
                if (IsProxyWorks(cachedProxy, client))
                    yield return cachedProxy;
        }

        private bool IsProxyWorks(WebProxy proxy, WebClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (proxy == null) return false;

            client.Headers.Add("user-agent", _userAgent);
            client.Proxy = proxy;

            try
            {
                var settings = _cashlogSettingsService.ReadSettings();
                var testUrl = $"https://api.telegram.org/bot{settings.TelegramBotToken}/getMe";

                _logger.Trace($"{GetType().Name}: Начата проверка прокси-сервер `{proxy.Address}`...");
                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(10));
                Task.Run(() => client.DownloadString(testUrl), cts.Token).GetAwaiter().GetResult();
                _logger.Trace($"{GetType().Name}: Прокси-сервер `{proxy.Address}` работает");
            }
            catch (OperationCanceledException)
            {
                _logger.Trace($"{GetType().Name}: Прокси-сервер `{proxy.Address}` не работает");
                return false;
            }
            catch (Exception ex)
            {
                _logger.Trace($"{GetType().Name}: Прокси-сервер `{proxy.Address}` не работает");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Устанавливает адрес нового прокси сервера.
        /// </summary>
        private void SetNewProxy(WebProxy newProxy, WebProxy oldProxy)
        {
            if (oldProxy?.Address == newProxy?.Address)
                return;

            _logger.Info($"{GetType().Name}: Был изменён адрес прокси сервера. {oldProxy?.Address?.ToString() ?? "<NONE>"} ==> {newProxy?.Address?.ToString() ?? "<NONE>"}");

            // Обновляем адрес прокси сервера в конфиге.
            var settings = _cashlogSettingsService.ReadSettings();
            settings.ProxyAddress = newProxy?.Address?.ToString();
            _cashlogSettingsService.WriteSettings(settings);

            foreach (var proxyConsumer in _proxyConsumers)
                proxyConsumer.OnProxyChanged(newProxy);
        }

        public void Dispose() => _worker.Dispose();

        public void Start() => _worker.Start();

        public void Stop() => _worker.Stop();
    }

    /// <summary>
    /// Потребитель прокси-сервера.
    /// </summary>
    public interface IProxyConsumer
    {
        public void OnProxyChanged(WebProxy proxy);
    }
}