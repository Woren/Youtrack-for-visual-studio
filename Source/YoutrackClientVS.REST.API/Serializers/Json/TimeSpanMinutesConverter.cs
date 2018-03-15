﻿using System;
using Newtonsoft.Json;

namespace YouTrack.REST.API.Serializers.Json
{
    /// <summary>
    /// A JSON convertor that can convert a timespan in minutes into a <see cref="TimeSpan" /> value and vice-versa.
    /// </summary>
    public class TimeSpanMinutesConverter
        : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return typeof(TimeSpan) == objectType;
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var timeSpan = (TimeSpan)value;
            writer.WriteValue((long)Math.Round(timeSpan.TotalMinutes, 0));
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return null;
                case JsonToken.Integer:
                case JsonToken.Float:
                    return TimeSpan.FromMinutes((long)reader.Value);
                default:
                    throw new FormatException($"Could not parse value \"{reader.Value.ToString()}\" as TimeSpan.");
            }
        }
    }
}