using JobPlaytimeTracker.JobPlaytimeTracker;
using JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Context;
using JobPlaytimeTracker.JobPlaytimeTracker.Enums;
using JobPlaytimeTracker.Legos.Interfaces;
using Lumina.Excel.Sheets;

namespace JobPlaytimeTracker.Legos.Abstractions
{
    internal class BaseJob : IJob
    {
        /// <summary>
        /// The job ID queried in the Dalamud Lumina system for additional information.
        /// </summary>
        public FFXIVJob JobID { get; private set; }

        // Instance objects and variables
        private PluginContext _context;

        /// <summary>
        /// Initializes a new instance of the BaseJob class. This is a primordial class that serves as a wrapper around dalamud API
        /// elements. I initially made it to practice using the DataManager, however it remains as a potential base class for
        /// future development. Elements that were previously abstract have now been defined, but future development may introduce
        /// new abstract elements.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="jobID"></param>
        public BaseJob(PluginContext context, FFXIVJob jobID)
        {
            _context = context;
            JobID = jobID;
        }

        public string JobName 
        { 
            get
            {
                return Utility.CapitalizeString(_context.DataManager.GetExcelSheet<ClassJob>().GetRow((uint)JobID).Name.ToString());
            }
        }

        public string JobAbbreviation 
        { 
            get
            {
                return _context.DataManager.GetExcelSheet<ClassJob>().GetRow((uint)JobID).Abbreviation.ToString();
            }
        }

        public override string ToString()
        {
            return $"({JobAbbreviation})   {JobName}";
        }
    }
}
