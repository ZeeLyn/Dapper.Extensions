using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Example
{
    public class DITest
    {
        public bool Value { get; }

        public DITest(bool b = false)
        {
            Value = b;
        }
    }
}
