using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Extensions.MasterSlave
{
    public interface ILoadBalancing
    {
        string NextConnectionString(IReadOnlyList<SlaveConfiguration> slaves);
    }
}
