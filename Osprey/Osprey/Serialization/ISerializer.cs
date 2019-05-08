using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Osprey.Serialization
{
    public interface ISerializer
    {
        string Serialize(object obj);
        T Deserialize<T>(string value);
    }

    public class JsonSerializer : ISerializer
    {
        public string Serialize(object obj) => JsonConvert.SerializeObject(obj);

        public T Deserialize<T>(string value) => JsonConvert.DeserializeObject<T>(value);
    }
}
