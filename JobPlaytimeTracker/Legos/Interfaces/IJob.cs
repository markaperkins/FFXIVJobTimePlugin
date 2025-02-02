using JobPlaytimeTracker.JobPlaytimeTracker.Enums;

namespace JobPlaytimeTracker.Legos.Interfaces
{
    internal interface IJob
    {
        public FFXIVJob JobID { get; }
        public string JobName { get; }
        public string JobAbbreviation { get; }
    }
}
