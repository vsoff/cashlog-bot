using System;
using ProtoBuf.Meta;

namespace Cashlog.Core.Messengers
{
    public static class UrlBase64Serializer
    {
        public static string Base64Encode(object obj)
        {
            var plainTextBytes = ProtoSerializerHelper.Serialize(obj, RuntimeTypeModel.Default);
            string base64 = Convert.ToBase64String(plainTextBytes);
            return base64.Replace('+', '.').Replace('/', '_').Replace('=', '-');
        }

        public static T Base64Decode<T>(string base64EncodedData)
        {
            string base64Normalize = base64EncodedData.Replace('.', '+').Replace('_', '/').Replace('-', '=');
            var base64EncodedBytes = Convert.FromBase64String(base64Normalize);
            var data = ProtoSerializerHelper.Deserialize<T>(base64EncodedBytes, RuntimeTypeModel.Default);
            return data;
        }
    }
}