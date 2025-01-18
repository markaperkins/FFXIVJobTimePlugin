using JobPlaytimeTracker.JobPlaytimeTracker.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Entities
{
    internal class JobMetric
    {
        public FFXIVJob JobID { get; set; }
        public int NumberOfDeaths { get; set; }
    }
}
