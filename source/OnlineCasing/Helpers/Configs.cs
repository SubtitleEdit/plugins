using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nikse.SubtitleEdit.PluginLogic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMDbLib.Objects.Search;

namespace OnlineCasing
{

    public class BoolConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((bool)value) ? 1 : 0);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null || reader.Value.ToString() == "False")
            {
                return false;
            }
            return true;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(bool);
        }
    }

    class CustomConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(value);

            if (t.Type != JTokenType.Object)
            {
                t.WriteTo(writer);
            }
            else
            {
                JObject o = (JObject)t;
                IList<string> propertyNames = o.Properties().Select(p => p.Name).ToList();

                o.AddFirst(new JProperty("Keys", new JArray(propertyNames)));

                o.WriteTo(writer);
            }
        }
    }

    public static class Configs
    {
        public static Settings Settings { get; set; }
        public static readonly string SettingFile = Path.Combine(FileUtils.Plugins, "onlie-casing-setting.json");
        private static JsonSerializerSettings jsonSettings;

        static Configs()
        {
            //TMDbLib.Utilities.Converters.CustomDatetimeFormatConverter
            jsonSettings = new JsonSerializerSettings
            {
                //Converters = {
                //    // note: some how all these converters aren't working..
                //    // solution: create map to SearchMovie
                //new Newtonsoft.Json.Converters.StringEnumConverter(),
                //new TMDbLib.Helpers.TmdbNullIntAsZero(),
                //new TMDbLib.Helpers.TmdbPartialDateConverter(),
                //new TMDbLib.Helpers.TmdbUtcTimeConverter() },
                Formatting = Formatting.Indented,
            };
            //JsonConvert.DefaultSettings.Invoke().Converters.ToList();
            //foreach (var item in JsonConvert.DefaultSettings().Converters)
            //{
            //    jsonSettings.Converters.Add(item);
            //}


            //jsonSettings.Converters.Add(new Newtonsoft.Json.Converters.BinaryConverter());
            //jsonSettings.Converters.Add(new Newtonsoft.Json.Converters.BsonObjectIdConverter());
            //jsonSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            //jsonSettings.Converters.Add(new Newtonsoft.Json.Converters.BinaryConverter());
            //jsonSettings.Converters.Add(new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter());

            // NOTE: NEWTOWNSOFT 9.0.1 IS HAVING TROUBLE SERIALIZING/DESERIALIZING Nullable Types.
            if (File.Exists(SettingFile))
            {
                string json = File.ReadAllText(SettingFile, Encoding.UTF8);
                Settings = JsonConvert.DeserializeObject<Settings>(json, jsonSettings);
            }
            else
            {
                string key = Environment.GetEnvironmentVariable("tmdb_key");
                // default settings
                Settings = new Settings
                {
                    ApiKey = key,
                    Movies = new List<Movie>(),
                    IgnoreWords = new HashSet<string>()
                };
            }
        }

        public static void Save()
        {
            File.WriteAllText(SettingFile, JsonConvert.SerializeObject(Settings, jsonSettings), Encoding.UTF8);
        }
    }
}
