using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Amazon.CloudWatch.EMF.Model
{

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Unit
    {
        [Description("None")]
        NONE,
        [Description("Seconds")]
        SECONDS,
        [Description("Microseconds")]
        MICROSECONDS,
        [Description("Milliseconds")]
        MILLISECONDS,
        [Description("Bytes")]
        BYTES,
        [Description("Kilobytes")]
        KILOBYTES,
        [Description("Megabytes")]
        MEGABYTES,
        [Description("Gigabytes")]
        GIGABYTES,
        [Description("Terabytes")]
        TERABYTES,
        [Description("Bits")]
        BITS,
        [Description("Kilobits")]
        KILOBITS,
        [Description("Megabits")]
        MEGABITS,
        [Description("Gigabits")]
        GIGABITS,
        [Description("Terabits")]
        TERABITS,
        [Description("Percent")]
        PERCENT,
        [Description("Count")]
        COUNT,
        [Description("Bytes/Second")]
        BYTES_PER_SECOND,
        [Description("Kilobytes/Second")]
        KILOBYTES_PER_SECOND,
        [Description("Megabytes/Second")]
        MEGABYTES_PER_SECOND,
        [Description("Gigabytes/Second")]
        GIGABYTES_PER_SECOND,
        [Description("Terabytes/Second")]
        TERABYTES_PER_SECOND,
        [Description("Bits/Second")]
        BITS_PER_SECOND,
        [Description("Kilobits/Second")]
        KILOBITS_PER_SECOND,
        [Description("Megabits/Second")]
        MEGABITS_PER_SECOND,
        [Description("Gigabits/Second")]
        GIGABITS_PER_SECOND,
        [Description("Terabits/Second")]
        TERABITS_PER_SECOND,
        [Description("Count/Second")]
        COUNT_PER_SECOND
    }
}