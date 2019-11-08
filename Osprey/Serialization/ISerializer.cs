using System;
using System.Collections.Generic;
using System.Text;

namespace Osprey.Serialization
{
    public interface ISerializer
    {
        string Serialize(object obj);
	    T Deserialize<T>(string value);
	    object Deserialize(string value, Type type);
	}
}
