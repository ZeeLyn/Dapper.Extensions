using System;
using System.Collections.Generic;
using System.Linq;

namespace Dapper.Extensions.MasterSlave
{
    public class WeightedPolling : ILoadBalancing
    {
        private static readonly object LockObject = new object();

        public string NextConnectionString(IReadOnlyList<SlaveConfiguration> slaves)
        {
            if (slaves == null || !slaves.Any())
                throw new ArgumentNullException(nameof(slaves));
            if (slaves.Count == 1)
                return slaves.First().ConnectionString;
            lock (LockObject)
            {
                var index = -1;
                var total = 0;
                for (var i = 0; i < slaves.Count; i++)
                {
                    slaves[i].Attach += slaves[i].Weight;
                    total += slaves[i].Weight;
                    if (index == -1 || slaves[index].Attach < slaves[i].Attach)
                    {
                        index = i;
                    }
                }
                slaves[index].Attach -= total;
                return slaves[index].ConnectionString;
            }
        }
    }
}
