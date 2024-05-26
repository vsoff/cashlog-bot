using Cashlog.Core.Models.Main;

namespace Cashlog.Core.Services.Abstract;

/// <summary>
///     Сервис управления группами.
/// </summary>
public interface IGroupService
{
    /// <summary>
    ///     Добавляет новую группу.
    /// </summary>
    Task<GroupDto> AddAsync(string chatToken, string adminToken, string chatName);

    /// <summary>
    ///     Возвращает группу по её токену.
    /// </summary>
    Task<GroupDto> GetByChatTokenAsync(string chatToken);
}