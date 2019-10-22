using System;

namespace Cashlog.Core.Messengers.Menu
{
    public static class AddReceiptQueryHelper
    {
        public static string ToQuery(this AddReceiptQueryData data) => UrlBase64Serializer.Base64Encode(data);

        public static AddReceiptQueryData ParseQueryData(string query)
        {
            var queryData = UrlBase64Serializer.Base64Decode<AddReceiptQueryData>(query);

            if (queryData.Version != AddReceiptQueryData.CurrentServerVersion)
                throw new ArgumentException("Несоответствие версий в запросе и на сервере");

            return queryData;
        }
    }
}