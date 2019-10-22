using System;
using ProtoBuf;

namespace Cashlog.Core.Messengers.Menu
{
    [ProtoContract]
    public class AddReceiptQueryData
    {
        public const int CurrentServerVersion = 1;

        [ProtoMember(1)]
        public int Version { get; set; }

        [ProtoMember(2)]
        public MenuType MenuType { get; set; }

        [ProtoMember(3)]
        public long ReceiptId { get; set; }

        [ProtoMember(4)]
        public long? TargetId { get; set; }

        [ProtoMember(5)]
        public long? SelectedCustomerId { get; set; }

        [ProtoMember(6)]
        public long[] SelectedConsumerIds { get; set; }

        [Obsolete("Необходимо удалить, так как потеряло свою актуальность")]
        [ProtoMember(7)]
        public bool IsApply { get; set; }

        [ProtoMember(8)]
        public string ChatToken { get; set; }
    }
}