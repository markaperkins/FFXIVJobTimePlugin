using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Context;
using JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Flags;
using JobPlaytimeTracker.JobPlaytimeTracker.Enums;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobPlaytimeTracker.JobPlaytimeTracker.EventHandlers
{
    public class ClientStateEvent
    {
        private PluginContext _context;
        public EventStateFlags EventStates { get; private set; }
        public Dictionary<string, DateTime> EventStartTimes { get; private set; }

        private string tempHolder = "";

        public ClientStateEvent(PluginContext context) 
        {
            _context = context;
            EventStartTimes = new Dictionary<string, DateTime>();

            // Set current states
            EventStates = EventStateFlags.None;
            if (_context.ClientState.LocalPlayer is not null)
            {
                if (_context.ClientState.LocalPlayer.IsDead) EventStates |= EventStateFlags.PlayerIsRIP;

                if (_context.ClientState.LocalPlayer.OnlineStatus.Value.Name.Equals("AFK")) EventStates |= EventStateFlags.PlayerIsAFK;

                if (_context.Conditions[ConditionFlag.InCombat] ||
                    _context.Conditions[ConditionFlag.Crafting] || 
                    _context.Conditions[ConditionFlag.Gathering]) 
                        EventStates |= EventStateFlags.PlayerJobIsActive;
            }
        }
                                        
        /// <summary>
        /// Updates the player's current job and logs the time that they were playing the previous job.
        /// </summary>
        /// <param name="classJobId">The new ClassJob id.</param>
        public void OnJobChange(uint classJobId)
        {
            // Register update and job change
            TimeSpan elapsedTime = _context.ReceivedUpdate();
            FFXIVJob previousJob = _context.UpdateCurrentJob((FFXIVJob)classJobId);

            // Update metrics
            _context.CurrentPlayer.AddTimePlayed(elapsedTime, previousJob);
        }

        /// <summary>
        /// Handles condition change events. IE: Updating job states when entering/leaving combat.
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="value"></param>
        public void OnConditionChange(ConditionFlag flag, bool value)
        {
            if (flag == ConditionFlag.InCombat ||
                flag == ConditionFlag.Crafting ||
                flag == ConditionFlag.Gathering)
            {
                HandleJobStateChange(flag, value);
            }
            else if(flag == ConditionFlag.Unconscious)
            {
                RegisterPlayerDeath();
            }
        }

        /// <summary>
        /// If the player is entering combat, sets/assigns a combat start time and sets the player job active flag to true.
        /// If the player is leaving combat, updates job with new combat time length.
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="value"></param>
        private void HandleJobStateChange(ConditionFlag flag, bool value)
        {
            if (value == true)
            {
                if (EventStartTimes.ContainsKey(flag.ToString()))
                {
                    EventStartTimes[flag.ToString()] = DateTime.Now;
                }
                else
                {
                    EventStartTimes.Add(flag.ToString(), DateTime.Now);
                }

                EventStates |= EventStateFlags.PlayerJobIsActive;
            }
            else
            {
                if(EventStartTimes.ContainsKey(flag.ToString()))
                {
                    // Calculate time spent in combat
                    DateTime combatStarted = EventStartTimes[flag.ToString()];
                    TimeSpan timeInCombat = DateTime.Now - combatStarted;

                    // Register time with player
                    _context.CurrentPlayer.AddTimeActive(timeInCombat, _context.CurrentJob);

                    // Remove last start time
                    EventStartTimes.Remove(flag.ToString());
                }
                // else: do nothing, there wasn't a start time to calculate a TimeSpan with.

                EventStates &= ~EventStateFlags.PlayerJobIsActive;
            }
        }

        public void OnTick(IFramework framework)
        {
            IPlayerCharacter? currentPlayer = _context.ClientState.LocalPlayer;
            if (currentPlayer is null) return; // Player is not logged in

            CheckOnlineStatus(currentPlayer.OnlineStatus.Value.Name.ToString());
        }

        private void CheckOnlineStatus(string onlineStatus)
        {
            // Store evaluation results that are used multiple times
            bool evalResult_IsStatusAFK = onlineStatus.Equals("Away from Keyboard");
            bool evalResult_IsAFKFlagSet = EventStates.HasFlag(EventStateFlags.PlayerIsAFK);

            if (evalResult_IsStatusAFK == true && evalResult_IsAFKFlagSet == false)
            {
                if(EventStartTimes.ContainsKey(EventStateFlags.PlayerIsAFK.ToString()))
                {
                    EventStartTimes[EventStateFlags.PlayerIsAFK.ToString()] = DateTime.Now;
                }
                else
                {
                    EventStartTimes.Add(EventStateFlags.PlayerIsAFK.ToString(), DateTime.Now);
                }

                EventStates |= EventStateFlags.PlayerIsAFK;
            }
            else if (evalResult_IsStatusAFK == false && evalResult_IsAFKFlagSet == true)
            {
                if(EventStartTimes.ContainsKey(EventStateFlags.PlayerIsAFK.ToString()))
                {
                    // Calculate time spent AFK
                    DateTime AFKStarted = EventStartTimes[EventStateFlags.PlayerIsAFK.ToString()];
                    TimeSpan timeSpentAFK = DateTime.Now - AFKStarted;

                    // Register time with player
                    _context.CurrentPlayer.AddAFKTime(timeSpentAFK, _context.CurrentJob);

                    // Remove last start time
                    EventStartTimes.Remove(EventStateFlags.PlayerIsAFK.ToString());
                }
                // else: do nothing, there wasn't a start time to calculate a TimeSpan with.

                EventStates &= ~EventStateFlags.PlayerIsAFK;
            }
        }

        private void RegisterPlayerDeath()
        {
            // Gate function to return if this function is somehow called while LocalPlayer is null (logged out)
            if (_context.ClientState.LocalPlayer is null) return;

            // Gate function to avoid processing multiple deaths. The RIP flag is set at the conclusion of the first
            // function call, and is unset on the first tick when the player is no longer dead.
            if (EventStates.HasFlag(EventStateFlags.PlayerIsRIP)) return;

            _context.CurrentPlayer.AddDeath((FFXIVJob)_context.ClientState.LocalPlayer.ClassJob.RowId);
        }
    }
}
