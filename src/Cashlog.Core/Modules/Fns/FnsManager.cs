using System.Net;
using System.Text;
using Cashlog.Core.Common;
using Cashlog.Core.Modules.Fns.Models;
using Flurl.Http;
using Newtonsoft.Json;

namespace Cashlog.Core.Modules.Fns;

/// <summary>
///     Сервис взаимодействия с API ФНС.
/// </summary>
/// <remarks>HABR https://habr.com/ru/post/358966/ </remarks>
public interface IFnsService
{
    /// <summary>
    ///     Возвращает true, если в ФНС есть информация по чеку.
    /// </summary>
    Task<bool> ReceiptExistsAsync(ReceiptMainInfo receiptInfo);

    /// <summary>
    ///     Возвращает детальную информацию по чеку.
    /// </summary>
    Task<FnsReceiptDetailInfo> GetReceiptAsync(ReceiptMainInfo receiptInfo, string phone, string password);
}

public interface IFnsAccountService
{
    /// <summary>
    ///     Регистрация нового пользователя. Необходима для получения детальной информации по чекам.
    /// </summary>
    /// <param name="email">Электронный адрес пользователя</param>
    /// <param name="name">Имя пользователя</param>
    /// <param name="phone">Номер телефона пользователя в формате +79991234567</param>
    /// <returns></returns>
    Task<FnsResult> RegisterNewAsync(string email, string name, string phone);

    /// <summary>
    ///     Аутентификация пользователя. Необходимости в ней нет, но раз ФНС предоставляет, как не воспользоваться.
    /// </summary>
    /// <param name="phone">Номер телефона пользователя в формате +79991234567</param>
    /// <param name="password">Пароль пользователя, который он получал из СМС при регистрации или восстановлении пароля</param>
    /// <returns>Возвращает адрес электронной почты и имя указанные при регистрации</returns>
    Task<FnsResult> LoginAsync(string phone, string password);

    /// <summary>
    ///     Восстановление пароля. Восстановленный пароль придет в СМС.
    /// </summary>
    /// <param name="phone">Номер телефона в формате +79991234567</param>
    /// <returns></returns>
    Task<FnsResult> RestorePasswordAsync(string phone);
}

public class FnsRegistrationRequest
{
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
}

public class FnsRestorePasswordRequest
{
    public string Phone { get; set; }
}

public class FnsService : IFnsService, IFnsAccountService
{
    private readonly ILogger _logger;
    private readonly IFlurlClient _client;

    public FnsService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = new FlurlClient();
    }

    public async Task<FnsResult> RegisterNewAsync(string email, string name, string phone)
    {
        if (string.IsNullOrEmpty(email)) throw new ArgumentNullException(nameof(email));
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
        if (string.IsNullOrEmpty(phone)) throw new ArgumentNullException(nameof(phone));

        var response = await Urls.Registration
            .WithClient(_client)
            .PostJsonAsync(new FnsRegistrationRequest
            {
                Email = email,
                Name = name,
                Phone = phone
            });

        return await GetResultAsync(response);
    }

    public async Task<FnsResult> LoginAsync(string phone, string password)
    {
        if (string.IsNullOrEmpty(phone)) throw new ArgumentNullException(nameof(phone));
        if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));

        var request = Urls.Login.WithClient(_client);
        request = AddAuthorizationHeaders(request, phone, password);

        var response = await request.GetAsync();
        return await GetResultAsync(response);
    }

    public async Task<FnsResult> RestorePasswordAsync(string phone)
    {
        if (string.IsNullOrEmpty(phone)) throw new ArgumentNullException(nameof(phone));

        var response = await Urls.Restore
            .WithClient(_client)
            .PostJsonAsync(new FnsRestorePasswordRequest
            {
                Phone = phone
            });

        return await GetResultAsync(response);
    }

    public async Task<bool> ReceiptExistsAsync(ReceiptMainInfo receiptInfo)
    {
        var url = Urls.GetCheckUrl(receiptInfo.FiscalNumber, receiptInfo.FiscalDocument, receiptInfo.FiscalSign,
            receiptInfo.PurchaseTime, (decimal)receiptInfo.TotalAmount);
        var response = await url.WithClient(_client).GetAsync();
        var result = new CheckFnsResult
        {
            IsSuccess = response.IsSuccessStatusCode,
            Message = await response.Content.ReadAsStringAsync(),
            ReceiptExists = response.IsSuccessStatusCode,
            StatusCode = response.StatusCode
        };
        return result.ReceiptExists;
    }

    public async Task<FnsReceiptDetailInfo> GetReceiptAsync(ReceiptMainInfo receiptInfo, string phone, string password)
    {
        if (!await ReceiptExistsAsync(receiptInfo))
            return null;

        // Формируем запрос.
        var request = Urls.GetReceiveUrl(receiptInfo.FiscalNumber, receiptInfo.FiscalDocument, receiptInfo.FiscalSign)
            .WithClient(_client);
        request = AddRequiredHeaders(request);
        request = AddAuthorizationHeaders(request, phone, password);

        // Запрашиваем данные из ФНС.
        HttpResponseMessage response = null;
        const int retires = 4;
        for (var i = 0; i < retires; i++)
            try
            {
                response = await request.GetAsync();
                if (response.StatusCode != HttpStatusCode.Accepted)
                    break;

                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
            catch (Exception ex)
            {
                _logger.Error($"Во время выполнения запроса информации о чеке (попытка №{i}) произошло исключение", ex);
            }

        if (response == null || response.StatusCode != HttpStatusCode.OK)
            return null;

        var result = new ReceiptFnsResult
        {
            IsSuccess = response.IsSuccessStatusCode,
            StatusCode = response.StatusCode
        };

        // Десериализуем ответ.
        try
        {
            var json = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(json))
                result.Document = JsonConvert.DeserializeObject<RootObject>(json).Document;
        }
        catch (Exception ex)
        {
            _logger.Error("Во время десериализации информации о чеке произошло исключение", ex);
        }

        if (result.Document?.ReceiptInfo == null)
            return null;

        return result.Document?.ReceiptInfo;
    }

    /// <summary>
    ///     Преобразует HttpResponseMessage в FnsResult.
    /// </summary>
    private static async Task<FnsResult> GetResultAsync(HttpResponseMessage response)
    {
        return new FnsResult()
        {
            IsSuccess = response.IsSuccessStatusCode,
            Message = await response.Content.ReadAsStringAsync(),
            StatusCode = response.StatusCode
        };
    }

    /// <summary>
    ///     Некоторые методы требуют специальных заголовков. Данный метод добавляет их.
    /// </summary>
    private static IFlurlRequest AddRequiredHeaders(IFlurlRequest request)
    {
        return request.WithHeader("Device-Id", string.Empty)
            .WithHeader("Device-OS", string.Empty);
    }

    /// <summary>
    ///     Некоторые методы требуют авторизации. Данный метод добавляет эту авторизацию.
    /// </summary>
    private static IFlurlRequest AddAuthorizationHeaders(IFlurlRequest request, string phone, string password)
    {
        var credentialBuffer = new UTF8Encoding().GetBytes($"{phone}:{password}");
        return request.WithHeader("Authorization", $"Basic {Convert.ToBase64String(credentialBuffer)}");
    }
}