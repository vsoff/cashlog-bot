using System;
using ProtoBuf;

namespace Cashlog.Core.Messengers.Menu
{
    /// <summary>
    /// Данные перевода денег между кастомерами.
    /// </summary>
    [ProtoContract]
    public class MoneyTransferQueryData : IQueryData
    {
        public const int CurrentServerVersion = 1;

        [ProtoMember(1)]
        public int Version { get; set; }

        [ProtoMember(2)]
        public MenuType MenuType { get; set; }

        [ProtoMember(3)]
        public string ChatToken { get; set; }

        [ProtoMember(4)]
        public long? CustomerFromId { get; set; }

        [ProtoMember(5)]
        public long? CustomerToId { get; set; }

        [ProtoMember(6)]
        public long? TargetId { get; set; }

        [ProtoMember(7)]
        public int Amount { get; set; }

        [ProtoMember(8)]
        public string Caption { get; set; }
    }

    /// <summary>
    /// Данные запроса добавления нового чека.
    /// </summary>
    [ProtoContract]
    public class AddReceiptQueryData : IQueryData
    {
        public const int CurrentServerVersion = 1;

        [ProtoMember(1)]
        public int Version { get; set; }

        [ProtoMember(2)]
        public MenuType MenuType { get; set; }

        [ProtoMember(3)]
        public string ChatToken { get; set; }

        [ProtoMember(4)]
        public long ReceiptId { get; set; }

        [ProtoMember(5)]
        public long? TargetId { get; set; }

        [ProtoMember(6)]
        public long? SelectedCustomerId { get; set; }

        [ProtoMember(7)]
        public long[] SelectedConsumerIds { get; set; }
    }
}