using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Extensions.Caching
{
    public interface IDataSerializer
    {
        string Serialize(object data);

        T Deserialize<T>(string value);
    }
}
