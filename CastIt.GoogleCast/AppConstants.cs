using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CastIt.GoogleCast
{
    internal static class AppConstants
    {
        public const string SENDER_ID = "sender-0";
        public const string DESTINATION_ID = "receiver-0";

        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };
    }
}
