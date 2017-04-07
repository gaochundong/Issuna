using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Issuna.Core
{
    /// <summary>
    /// LongId is a 64-bit(long/int64) ID consists of:
    /// 63-61-bit as R-bit, 3 bits reserved for extensions, 0 by default.
    /// 60-51-bit as M-bits, 10 bits represents machine identifier, 10 bits covers 1-1024 servers.
    /// 50-bit as P-bit, time precision for using second(0) or millisecond(1), 0 by default.
    /// 49-20-bit as T-bits, timestamp if the P-bit is second(0), 30 bits covers more than 30 years.
    /// 49-10-bit as T-bits, timestamp if the P-bit is millisecond(1), 40 bits covers more than 30 years.
    /// 19-0-bit as Q-bits, sequence if the P-bit is second(0), 20 bits covers 1048576/second.
    /// 9-0-bit as Q-bits, sequence if the P-bit is millisecond(1), 10 bits covers 1024/millisecond.
    /// </summary>
    [Serializable]
    public struct LongId
    {
    }
}
