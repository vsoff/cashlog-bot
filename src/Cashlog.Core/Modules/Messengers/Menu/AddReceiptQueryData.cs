using ProtoBuf;

namespace Cashlog.Core.Modules.Messengers.Menu;

/// <summary>
///     Данные запроса добавления нового чека.
/// </summary>
[ProtoContract]
public class AddReceiptQueryData : IQueryData
{
    public const int ServerVersion = 1;

    [ProtoMember(4)] public long ReceiptId { get; set; }

    [ProtoMember(5)] public long? TargetId { get; set; }

    [ProtoMember(6)] public long? SelectedCustomerId { get; set; }

    [ProtoMember(7)] public long[] SelectedConsumerIds { get; set; }

    [ProtoMember(1)] public int Version { get; set; }

    [ProtoMember(2)] public MenuType MenuType { get; set; }

    [ProtoMember(3)] public string ChatToken { get; set; }
}