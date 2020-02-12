using System;
using System.Collections.Generic;
using Cashlog.Core.Common;
using ProtoBuf;
using ProtoBuf.Meta;

namespace Cashlog.Core.Messengers.Menu
{
    public interface IQueryDataSerializer
    {
        string EncodeBase64(IQueryData data, MenuType type);
        IQueryData DecodeBase64(string base64);
    }

    public class QueryDataSerializer : IQueryDataSerializer
    {
        // Todo Проверка №1. Надо перенести в одно место.
        private readonly Dictionary<MenuType, Type> _typeMap = new Dictionary<MenuType, Type>
        {
            {MenuType.NewReceiptSelectConsumers, typeof(AddReceiptQueryData)},
            {MenuType.NewReceiptSelectCustomer, typeof(AddReceiptQueryData)},
            {MenuType.NewReceiptCancel, typeof(AddReceiptQueryData)},
            {MenuType.NewReceiptAdd, typeof(AddReceiptQueryData)},

            {MenuType.MoneyTransferSelectFrom, typeof(MoneyTransferQueryData)},
            {MenuType.MoneyTransferSelectTo, typeof(MoneyTransferQueryData)},
            {MenuType.MoneyTransferAdd, typeof(MoneyTransferQueryData)},
            {MenuType.MoneyTransferCancel, typeof(MoneyTransferQueryData)},
        };

        public string EncodeBase64(IQueryData data, MenuType type)
        {
            return UrlBase64Serializer.Base64Encode(new QueryDataRaw
            {
                Type = type,
                Data = ProtoSerializerHelper.Serialize(data, RuntimeTypeModel.Default)
            });
        }

        public IQueryData DecodeBase64(string base64)
        {
            var rawData = UrlBase64Serializer.Base64Decode<QueryDataRaw>(base64);
            if (_typeMap.ContainsKey(rawData.Type))
                return (IQueryData) ProtoSerializerHelper.Deserialize(_typeMap[rawData.Type], rawData.Data, RuntimeTypeModel.Default);
            throw new Exception($"Для типа {rawData.Type} не указан тип декодинга");
        }
    }

    [ProtoContract]
    public class QueryDataRaw
    {
        [ProtoMember(1)]
        public MenuType Type { get; set; }

        [ProtoMember(2)]
        public byte[] Data { get; set; }
    }

    public interface IQueryData
    {
        int Version { get; set; }
        MenuType MenuType { get; set; }
        string ChatToken { get; set; }
    }
}