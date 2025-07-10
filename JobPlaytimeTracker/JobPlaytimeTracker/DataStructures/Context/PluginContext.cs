using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Plugin;
using JobPlaytimeTracker.Resources.Strings;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;
using JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Entities;
using JobPlaytimeTracker.JobPlaytimeTracker.Enums;
using System.Linq;
using System.IO;
using System.Text.Json;
using JobPlaytimeTracker.JobPlaytimeTracker.EventHandlers;
using System.Collections.Generic;

namespace JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Context
{
    public class PluginContext : IDisposable
    {
        // Fields to expose to any entity that is in scope of a reference to the PluginContext.
        public WindowSystem? PluginWindows { get; private set; }
        public Configuration? PluginConfiguration { get; private set; }
        public IDalamudPluginInterface PluginInterface { get; private set; }
        public ITextureProvider TextureProvider { get; private set; }
        public ICommandManager CommandManager { get; private set; }
        public IClientState ClientState { get; private set; }
        public IDataManager DataManager { get; private set; }
        public IPluginLog Log { get; private set; }
        public IChatGui ChatGUI { get; private set; }
        public Player? CurrentPlayer { get; private set; }
        public DateTime LastUpdate { get; private set; }
        public FFXIVJob CurrentJob { get; private set; }
        public ICondition Conditions { get; private set; }
        public ClientStateEvent PlayerEventHandler { get; private set; }
        public IFramework Framework { get; private set; }
        public IDtrBar DtrBar { get; private set; }

        public PluginContext(IDalamudPluginInterface pluginInterface,
                             ITextureProvider textureProvider,
                             ICommandManager commandManager,
                             IClientState clientState,
                             IDataManager dataManager,
                             IPluginLog log,
                             IChatGui chatGUI,
                             ICondition conditions,
                             IFramework framework,
                             IDtrBar dtrBar)
        {
            // Store references to plugin services
            PluginInterface = pluginInterface;
            TextureProvider = textureProvider;
            CommandManager = commandManager;
            ClientState = clientState;
            DataManager = dataManager;
            Log = log;
            ChatGUI = chatGUI;
            DtrBar = dtrBar;

            // Seed update time and job
            _ = ReceivedUpdate();

            // Initialize the plugin window system
            PluginWindows = new(Assembly.GetExecutingAssembly().GetName().Name!);

            // Restore or initialize the plugin configuration
            PluginConfiguration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Conditions = conditions;
            Framework = framework;

            // Initialize event handler
            PlayerEventHandler = new ClientStateEvent(this);
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Updates the LastUpdate field to DateTime.Now.
        /// </summary>
        /// <returns>TimeSpan of the elapsed time between the previous update and DateTime.Now</returns>
        public TimeSpan ReceivedUpdate()
        {
            TimeSpan elapsedTime = DateTime.Now - LastUpdate;
            LastUpdate = DateTime.Now;

            return elapsedTime;
        }

        /// <summary>
        /// Updates the CurrentJob field.
        /// </summary>
        /// <returns>The player's previous job.</returns>
        public FFXIVJob UpdateCurrentJob(FFXIVJob newJob)
        {
            FFXIVJob previousJob = CurrentJob;
            CurrentJob = newJob;

            return previousJob;
        }

        /// <summary>
        /// Calculates the current elapsed time without updating the last update time.
        /// </summary>
        /// <returns>TimeSpan of the elapsed time between the previous update and DateTime.Now</returns>
        public TimeSpan CurrentElapsedTime()
        {
            return DateTime.Now - LastUpdate;
        }

        /// <summary>
        /// Updates the current active player.
        /// </summary>
        /// <param name="newPlayer"></param>
        public void UpdatePlayer(Player newPlayer)
        {
            CurrentPlayer = newPlayer;
        }

        public void UpdatePlayer()
        {
            // Load current player's metrics
            string playerName = ClientState.LocalPlayer?.Name.ToString() ?? "Unknown";
            Player loadedPlayer = Player.LoadPlayer(this, playerName);

            // Save previous metrics
            if(CurrentPlayer is not null) Player.SavePlayer(CurrentPlayer);

            // Swap current character
            CurrentPlayer = loadedPlayer;
        }

        public bool IsPlayerDiscipleOfHand()
        {
            if (ClientState.LocalPlayer == null) return false;

            var craftingJobs = new HashSet<FFXIVJob>
            {
                FFXIVJob.Carpenter,
                FFXIVJob.Blacksmith,
                FFXIVJob.Armorer,
                FFXIVJob.Goldsmith,
                FFXIVJob.Leatherworker,
                FFXIVJob.Weaver,
                FFXIVJob.Alchemist,
                FFXIVJob.Culinarian
            };
            return craftingJobs.Contains((FFXIVJob)ClientState.LocalPlayer.ClassJob.RowId);
        }

        public bool IsPlayerDiscipleOfLand()
        {
            if (ClientState.LocalPlayer == null) return false;

            var gatheringJobs = new HashSet<FFXIVJob>
            {
                FFXIVJob.Miner,
                FFXIVJob.Botanist,
                FFXIVJob.Fisher
            };
            return gatheringJobs.Contains((FFXIVJob)ClientState.LocalPlayer.ClassJob.RowId);
        }

        public bool IsPlayerDiscipleOfWarMagic()
        {
            return IsPlayerDiscipleOfHand() == false && IsPlayerDiscipleOfLand() == false;
        }
    }
}
