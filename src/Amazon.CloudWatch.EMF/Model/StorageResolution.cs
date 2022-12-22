using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Amazon.CloudWatch.EMF.Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StorageResolution
    {
        [EnumMember(Value = "60")]
        STANDARD,

        [EnumMember(Value = "1")]
        HIGH,

        [EnumMember(Value = "-1")]
        UNKNOWN_TO_SDK_VERSION
    }
}