using System.Collections.Generic;

namespace Dapper.Extensions.MasterSlave
{
    public interface ILoadBalancing
    {
        string NextConnectionString(IReadOnlyList<SlaveConfiguration> slaves);
    }
}
