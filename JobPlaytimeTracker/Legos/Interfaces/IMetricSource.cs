using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobPlaytimeTracker.Legos.Interfaces
{
    public interface IMetricSource
    {
        public TimeSpan TimePlayed { get; }
        public TimeSpan TimeActive { get; }
        public TimeSpan TimeAFK { get; }
        public uint NumberOfDeaths { get; }
    }
}
