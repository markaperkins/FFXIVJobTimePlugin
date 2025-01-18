using JobPlaytimeTracker.JobPlaytimeTracker;
using JobPlaytimeTracker.JobPlaytimeTracker.Enums;
using JobPlaytimeTracker.Legos.Interfaces;
using Lumina.Excel.Sheets;

namespace JobPlaytimeTracker.Legos.Abstractions
{
    internal class BaseJob : IJob
    {
        public FFXIVJob JobID { get; private set; }

        public BaseJob(FFXIVJob jobID)
        {
            JobID = jobID;
        }

        public string JobName 
        { 
            get
            {
                return Utility.CapitalizeString(JobPlaytimeTrackerPlugin.DataManager.GetExcelSheet<ClassJob>().GetRow((uint)JobID).Name.ToString());
            }
        }

        public string JobAbbreviation 
        { 
            get
            {
                return JobPlaytimeTrackerPlugin.DataManager.GetExcelSheet<ClassJob>().GetRow((uint)JobID).Abbreviation.ToString();
            }
        }

        public override string ToString()
        {
            return $"({JobAbbreviation})   {JobName}";
        }
    }
}
