using System;
using Newtonsoft.Json;

namespace Osprey.Serialization
{
    public class JsonSerializer : ISerializer
    {
        public JsonSerializer()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
        }

        public string Serialize(object obj) => JsonConvert.SerializeObject(obj);

        public T Deserialize<T>(string value) => JsonConvert.DeserializeObject<T>(value);

        public object Deserialize(string value, Type type) => JsonConvert.DeserializeObject(value, type);
    }
}