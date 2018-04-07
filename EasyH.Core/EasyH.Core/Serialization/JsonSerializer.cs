using System;
using Newtonsoft.Json;

namespace EasyH.Core.Serialization
{
    public static class JsonSerializer
    {
        public static string Serialize<T>(this T t) where T : class
        {
            return t.ToJson();
        }

        public static T Deserialize<T>(this string value) where T : class
        {
            return value.FromJson<T>();
        }

        private static JsonSerializerSettings _jsonSettings;

        private static JsonSerializerSettings JsonSettings => _jsonSettings ?? (_jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
            DefaultValueHandling = DefaultValueHandling.Include,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            DateFormatString = "yyyy-MM-ddTHH:mm:sszzz",
            Converters = new JsonConverter[] { new JsGuidConverter() },
            NullValueHandling = NullValueHandling.Ignore,
            ConstructorHandling = ConstructorHandling.Default,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        private static JsonSerializerSettings _jsonSettingsFront;

        private static JsonSerializerSettings JsonSettingsFront => _jsonSettingsFront ?? (_jsonSettingsFront = new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
            DefaultValueHandling = DefaultValueHandling.Include,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            DateFormatString = "yyyy-MM-ddTHH:mm:sszzz",
            Converters = new JsonConverter[] { new JsGuidConverter() },
            NullValueHandling = NullValueHandling.Ignore,
            ConstructorHandling = ConstructorHandling.Default,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });



        public static string ToJson<T>(this T obj)
        {
            return obj != null ? JsonConvert.SerializeObject(obj, JsonSettings) : string.Empty;
        }

        public static string ToJson(this object obj, Type type)
        {
            return obj != null ? JsonConvert.SerializeObject(obj, type, JsonSettings) : string.Empty;
        }

        public static string ToJsonToFront<T>(this T obj)
        {
            return obj != null ? JsonConvert.SerializeObject(obj, JsonSettingsFront) : string.Empty;
        }

        public static T FromJson<T>(this string json)
        {
            return string.IsNullOrEmpty(json) ? default(T) : JsonConvert.DeserializeObject<T>(json, JsonSettings);
        }

        public static object FromJson(this string json, Type type)
        {
            return string.IsNullOrEmpty(json) ? null : JsonConvert.DeserializeObject(json, type, JsonSettings);
        }
    }

    public class JsGuidConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            try
            {
                var type = value.GetType();
                if (typeof(Guid) == type)
                {
                    var item = (Guid)value;
                    writer.WriteValue($"{item:N}");
                    writer.Flush();
                }
                else if (typeof(Guid?) == type)
                {
                    var item = (Guid?)value;
                    writer.WriteValue($"{item.Value:N}");
                    writer.Flush();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {

            if (reader.TokenType == JsonToken.Null)
            {
                return Guid.Empty;
            }
            if (!(typeof(Guid) == objectType || typeof(Guid?) == objectType))
                return Guid.Empty;
            try
            {
                var boolText = reader.Value.ToString();
                if (string.IsNullOrWhiteSpace(boolText))
                {
                    return Guid.Empty;
                }
                return Guid.TryParse(boolText, out Guid result) ? result : Guid.Empty;
            }
            catch (Exception)
            {
                throw new Exception($"Error converting value {reader.Value} to type '{objectType}'");
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Guid);
        }
    }
}
