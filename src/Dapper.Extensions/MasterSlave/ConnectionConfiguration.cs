using System.Collections.Generic;

namespace Dapper.Extensions.MasterSlave
{

    public class ConnectionConfiguration
    {
        public string Master { get; set; }

        public List<SlaveConfiguration> Slaves { get; set; }
    }

    public class SlaveConfiguration
    {
        public string ConnectionString { get; set; }

        public int Weight { get; set; }

        public int Attach { get; set; }
    }
}
