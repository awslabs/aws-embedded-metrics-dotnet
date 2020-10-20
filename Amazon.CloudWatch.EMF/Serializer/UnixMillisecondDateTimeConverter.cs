using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Globalization;

namespace Amazon.CloudWatch.EMF.Serializer
{
    internal class UnixMillisecondDateTimeConverter : DateTimeConverterBase
    {
        internal static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            long milliseconds;
            if (value is DateTime dateTime)
            {
                milliseconds = (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalMilliseconds;
            }
            else
            {
                throw new JsonSerializationException("Expected date object value.");
            }

            if (milliseconds < 0)
            {
                throw new JsonSerializationException("Cannot convert date value that is before Unix epoch of 00:00:00 UTC on 1 January 1970.");
            }

            writer.WriteValue(milliseconds);
        }
    }
}