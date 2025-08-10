using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Context;
using JobPlaytimeTracker.JobPlaytimeTracker.Enums;
using JobPlaytimeTracker.Legos.Interfaces;
using JobPlaytimeTracker.Resources.Strings;
using Lumina.Excel.Sheets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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

        // Private objects and variables
        private PluginContext _context;

        public Player()
        {
            PlayerName = "Unknown";
            Jobs = new List<JobMetric>();
            _context = JobPlaytimeTrackerPlugin.RequestContext();
        }

        /// <summary>
        /// Creates a new Player object, which is the root object that contains all data used to calculate various job and role metrics.
        /// </summary>
        /// <param name="playerName">The player's in-game name.</param>
        public Player(PluginContext context, string playerName)
        {
            PlayerName = playerName;
            Jobs = new List<JobMetric>();
            _context = context;
        }

        public void AddAFKTime(TimeSpan afkTime, FFXIVJob activeJob)
        {
            int jobIndex = Jobs.IndexOf(new JobMetric(activeJob));

            if (jobIndex != -1) // A result was found
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

            if (jobIndex != -1) // A result was found
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

            if (jobIndex != -1) // A result was found
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

        /// <summary>
        /// Saves the given player to the plugin's metrics directory.
        /// </summary>
        /// <param name="toSave"></param>
        public static void SavePlayer(Player toSave)
        {
            string jsonData = JsonSerializer.Serialize<Player>(toSave, new JsonSerializerOptions { WriteIndented = true });
            string filePath = Path.Combine(Paths.MetricsDirectory, $"{toSave.PlayerName}.json");
            File.WriteAllText(filePath, jsonData);
        }

        /// <summary>
        /// Loads the given player's data from storage if it exists. If not, initializes a new
        /// instance to collect player metric data.
        /// </summary>
        /// <param name="playerName">The logged in player's name.</param>
        /// <returns></returns>
        public static Player LoadPlayer(PluginContext context, string playerName)
        {
            string filePath = Path.Combine(Paths.MetricsDirectory, $"{playerName}.json");
            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                Player? loadedPlayer = JsonSerializer.Deserialize<Player>(jsonData);

                if (loadedPlayer is not null)
                    return loadedPlayer;
            }

            return new Player(context, playerName);
        }

        public static void OnLogout(int type, int code)
        {
            PluginContext currentContext = JobPlaytimeTrackerPlugin.RequestContext();

            // Save the player's current time before it is erased
            if(currentContext.CurrentPlayer is not null)
            {
                bool evalResult_IsStatusAFK = currentContext.ClientState.LocalPlayer.OnlineStatus.Value.Name.ToString().Equals("Away from Keyboard");

                TimeSpan elapsedTime = currentContext.ReceivedUpdate();
                if (evalResult_IsStatusAFK == true)
                {
                    currentContext.CurrentPlayer.AddAFKTime(elapsedTime, currentContext.CurrentJob);
                }
                else
                {
                    currentContext.CurrentPlayer.AddTimePlayed(elapsedTime, currentContext.CurrentJob);
                }
            }

            Player? toSave = currentContext.CurrentPlayer;
            if (toSave is not null)
                Player.SavePlayer(toSave);

            currentContext.UpdatePlayer(Player.LoadPlayer(currentContext, "Unknown"));
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
