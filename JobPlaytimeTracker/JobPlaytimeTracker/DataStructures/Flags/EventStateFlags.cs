using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Flags
{
    [Flags]
    public enum EventStateFlags: long
    {
        None = 0,
        PlayerJobIsActive = 1 << 0,
        PlayerIsAFK = 1 << 1,
        PlayerIsRIP = 1 << 2
    }
}
