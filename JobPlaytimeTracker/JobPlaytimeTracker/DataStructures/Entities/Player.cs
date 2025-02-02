using FFXIVClientStructs.FFXIV.Client.Graphics;
using JobPlaytimeTracker.JobPlaytimeTracker.Enums;
using JobPlaytimeTracker.Legos.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentFreeCompanyProfile.FCProfile;

namespace JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Entities
{
    public class Player : IMetricSource
    {
        // Exposed properties
        public string PlayerName { get; set; }
        public List<JobMetric> Jobs { get; set; }
        public TimeSpan TimePlayed { get { return Jobs.Aggregate<JobMetric, TimeSpan>(TimeSpan.Zero, (runningTotal, currentJob) => runningTotal + currentJob.TimePlayed); } }
        public TimeSpan TimeActive { get { return Jobs.Aggregate<JobMetric, TimeSpan>(TimeSpan.Zero, (runningTotal, currenJob) => runningTotal + currenJob.TimeActive); } }
        public TimeSpan TimeAFK { get { return Jobs.Aggregate<JobMetric, TimeSpan>(TimeSpan.Zero, (runningTotal, currentJob) => runningTotal + currentJob.TimeAFK); } }
        public uint NumberOfDeaths { get { return Jobs.Aggregate<JobMetric, uint>(0, (runningTotal, currentJob) => runningTotal + currentJob.NumberOfDeaths); } }

        public Player()
        {
            PlayerName = "Unknown";
            Jobs = new List<JobMetric>();
        }

        /// <summary>
        /// Creates a new Player object, which is the root object that contains all data used to calculate various job and role metrics.
        /// </summary>
        /// <param name="playerName">The player's in-game name.</param>
        public Player(string playerName) 
        {
            PlayerName = playerName;
            Jobs = new List<JobMetric>();
        }

        public void AddAFKTime(TimeSpan afkTime, FFXIVJob activeJob)
        {
            int jobIndex = Jobs.IndexOf(new JobMetric(activeJob));

            if(jobIndex != -1) // A result was found
            {
                Jobs[jobIndex].AddTimeAFK(afkTime);
            }
            else // A result was not found
            {
                JobMetric toAdd = new JobMetric(activeJob);
                toAdd.AddTimeAFK(afkTime);

                Jobs.Add(toAdd);
            }
        }

        public void AddTimeActive(TimeSpan activeTime, FFXIVJob activeJob)
        {
            int jobIndex = Jobs.IndexOf(new JobMetric(activeJob));

            if(jobIndex != -1) // A result was found
            {
                Jobs[jobIndex].AddTimeActive(activeTime);
            }
            else // A result was not found
            {
                JobMetric toAdd = new JobMetric(activeJob);
                toAdd.AddTimeActive(activeTime);

                Jobs.Add(toAdd);
            }
        }

        public void AddTimePlayed(TimeSpan playedTime, FFXIVJob activeJob)
        {
            int jobIndex = Jobs.IndexOf(new JobMetric(activeJob));

            if (jobIndex != -1) // A result was found
            {
                Jobs[jobIndex].AddTimePlayed(playedTime);
            }
            else // A result was not found
            {
                JobMetric toAdd = new JobMetric(activeJob);
                toAdd.AddTimePlayed(playedTime);

                Jobs.Add(toAdd);
            }
        }

        public void AddDeath(FFXIVJob activeJob)
        {
            int jobIndex = Jobs.IndexOf(new JobMetric(activeJob));

            if(jobIndex != -1) // A result was found
            {
                Jobs[jobIndex].AddDeath();
            }
            else // A result was not found
            {
                JobMetric toAdd = new JobMetric(activeJob);
                toAdd.AddDeath();

                Jobs.Add(toAdd);
            }
        }
    }

    public class PlayerComparer : IEqualityComparer<Player>
    {
        public bool Equals(Player? x, Player? y)
        {
            if (x is null || y is null) return false;

            return x.PlayerName.Equals(y.PlayerName);
        }

        public int GetHashCode([DisallowNull] Player obj)
        {
            return obj.GetHashCode();
        }
    }
}
