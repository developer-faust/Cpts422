using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS422
{
    public enum ValidationState
    {
        Method = 0,
        Url = 1,
        Version = 2,
        Headers = 3,
        Validated = 4,
        Invalidated = 5
    }
}
