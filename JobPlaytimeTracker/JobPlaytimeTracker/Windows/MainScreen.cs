using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Context;
using JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Entities;
using JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Flags;
using JobPlaytimeTracker.JobPlaytimeTracker.Enums;
using JobPlaytimeTracker.Legos.Abstractions;
using JobPlaytimeTracker.Legos.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace JobPlaytimeTracker.JobPlaytimeTracker.Windows
{
    internal class MainScreen : BaseWindow
    {
        // Parameters to initialize the screen through the base constructor
        private const string _windowName = "Job Playtime Tracker##Mainscreen";
        private const ImGuiWindowFlags _windowFlags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        private const bool _forceMainWindow = true;

        // Window configuration
        private int WindowWidth = 700;
        private int WindowHeight = 500;
        string TimeSpanFormat = @"hh\:mm\:ss";

        // Instance variables
        public int CurrentSelectedJobIndex;

        public MainScreen(PluginContext context) : base(context: context,
                                                        name: _windowName, 
                                                        flags: _windowFlags,
                                                        forceMainWindow: _forceMainWindow)
        {
            // Initialize variables
            CurrentSelectedJobIndex = 0;

            // Configure window display
            Size = new Vector2(WindowWidth, WindowHeight);
            SizeCondition = ImGuiCond.Always;
        }

        public override void Dispose()
        {
        }

        public override void Draw()
        {
            /***************************************************************************************************
             * START OF HEADER SECTION OF UI                                                                   *
             ***************************************************************************************************/
            // Create a label for the job selection combo box
            ImGui.SetCursorPos(new Vector2(15, 30));
            ImGui.Text("Select a Job:");

            // Create job selection combo box
            ImGui.SetCursorPos(new Vector2(95, 27));
            ImGui.SetNextItemWidth(590);
            string[] jobList = GenerateJobList();
            ImGui.Combo("##cmbJobSelector-Mainscreen", ref CurrentSelectedJobIndex, jobList, jobList.Count());

            // Draw a seperator to split the UI between the selection box and display
            ImGui.SetCursorPos(new Vector2(0, 55));
            ImGui.Separator();

            /***************************************************************************************************
             * Create metric table based on selection.                                                         *
             ***************************************************************************************************/
            ImGui.SetCursorPos(new Vector2(5, 65));
            ImGui.SetNextItemWidth(215);
            if(ImGui.BeginTable("##tblLayout", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersOuterV, new Vector2(450, 325)))
            {
                // Configure table
                ImGui.TableSetupColumn("Metric", ImGuiTableColumnFlags.WidthFixed, 165.0f);
                ImGui.TableSetupColumn("Stat", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableHeadersRow();

                // Populate table with metrics
                Player currentPlayer = (Context.CurrentPlayer ?? new Player(Context, "Unknown"));
                JobMetric selectedJob = currentPlayer.Jobs.FirstOrDefault<JobMetric>(metric => (int)metric.JobID == CurrentSelectedJobIndex) ?? new JobMetric((FFXIVJob)CurrentSelectedJobIndex);
                foreach (PropertyInfo property in GetPropertiesToDisplay())
                {
                    // Resolve metric display values
                    string metricName = property.GetCustomAttribute<DisplayableMetric>()?.DisplayName ?? property.Name;
                    string metricValue = FormatProperty(property, selectedJob);

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(metricName);
                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text(metricValue);
                }
            } 
            ImGui.EndTable();

            /***************************************************************************************************
             * Create seperator underneath metric table                                                        *
             ***************************************************************************************************/
            ImGui.SetCursorPos(new Vector2(0, 400));
            ImGui.Separator();
        }

        /// <summary>
        /// Formats the property, applying any custom filtering or updates in the process.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="metric"></param>
        /// <returns></returns>
        private string FormatProperty(PropertyInfo property, JobMetric metric)
        {
            if (property.PropertyType == typeof(TimeSpan)) return FormatTimeSpanProperty(property, metric);

            // Generic output
            return property.GetValue(metric)?.ToString() ?? "N/A";
        }

        /// <summary>
        /// Helper function that formats TimeSpan metrics for display. Handles special conditions like live updates on certain metrics.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="metric"></param>
        /// <returns></returns>
        private string FormatTimeSpanProperty(PropertyInfo property, JobMetric metric)
        {
            if(metric.JobID == Context.CurrentJob) // Display live total time if viewing current job
            {
                if (property.Name.Equals("TimePlayed"))
                {
                    TimeSpan liveElapsedTime = ((TimeSpan?)property.GetValue(metric) ?? TimeSpan.Zero) + Context.CurrentElapsedTime();
                    return liveElapsedTime.ToString(TimeSpanFormat);
                }
                else if (property.Name.Equals("TimeActive") && Context.PlayerEventHandler.EventStates.HasFlag(EventStateFlags.PlayerJobIsActive))
                {
                    // Determine event flag to correlate with
                    ConditionFlag? eventFlag = null;
                    if (Context.IsPlayerDiscipleOfHand())
                        eventFlag = ConditionFlag.Crafting;
                    else if (Context.IsPlayerDiscipleOfLand())
                        eventFlag = ConditionFlag.Gathering;
                    else
                        eventFlag = ConditionFlag.InCombat;

                    if (eventFlag is not null && Context.PlayerEventHandler.EventStartTimes.ContainsKey(eventFlag.ToString()!))
                    {
                        DateTime startTime = Context.PlayerEventHandler.EventStartTimes[eventFlag.ToString()!];
                        TimeSpan elapsedTime = DateTime.Now - startTime;
                        TimeSpan totalTime = ((TimeSpan?)property.GetValue(metric) ?? TimeSpan.Zero) + elapsedTime;

                        return totalTime.ToString(TimeSpanFormat);
                    }
                }
                else if (property.Name.Equals("TimeAFK") && 
                         Context.PlayerEventHandler.EventStates.HasFlag(EventStateFlags.PlayerIsAFK) &&
                         Context.PlayerEventHandler.EventStartTimes.ContainsKey(EventStateFlags.PlayerIsAFK.ToString()))
                {
                    DateTime startTime = Context.PlayerEventHandler.EventStartTimes[EventStateFlags.PlayerIsAFK.ToString()];
                    TimeSpan elapsedTime = DateTime.Now - startTime;
                    TimeSpan liveElapsedTime = ((TimeSpan?)property.GetValue(metric) ?? TimeSpan.Zero) + elapsedTime;

                    return liveElapsedTime.ToString(TimeSpanFormat);
                }
            }

            // Generic TimeSpan output
            return ((TimeSpan?)property.GetValue(metric))?.ToString(TimeSpanFormat) ?? "N/A";
        }

        private IOrderedEnumerable<PropertyInfo> GetPropertiesToDisplay()
        {
            Player currentPlayer = Context.CurrentPlayer ?? new Player(Context, "Unknown");

            JobMetric selectedJob = currentPlayer.Jobs.FirstOrDefault<JobMetric>(metric => (int)metric.JobID == CurrentSelectedJobIndex) ?? new JobMetric((FFXIVJob)CurrentSelectedJobIndex);

            return selectedJob.GetType().GetProperties().Where(p => p.GetCustomAttribute<DisplayableMetric>() != null)
                                                                     .OrderBy(p => p.GetCustomAttribute<DisplayAttribute>()?.Order ?? int.MaxValue);
        }

        private string[] GenerateJobList()
        {
            List<string> JobList = new List<string>();
            for (int jobID = 0; jobID < Enum.GetValues(typeof(FFXIVJob)).Length; jobID++)
            {
                // Create BaseJob object to access name and abbreviation for display
                BaseJob toList = new BaseJob(Context, (FFXIVJob)jobID);
                JobList.Add(toList.ToString()); 
            }

            return JobList.ToArray();
        }
    }
}
