using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using JobPlaytimeTracker.JobPlaytimeTracker.Commands;
using JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Context;
using JobPlaytimeTracker.JobPlaytimeTracker.Exceptions;
using JobPlaytimeTracker.Legos.Abstractions;
using JobPlaytimeTracker.Legos.Interface;
using JobPlaytimeTracker.Resources.Strings;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JobPlaytimeTracker
{
    internal class JobPlaytimeTrackerPlugin : IDalamudPlugin
    {
        // Define entity namespaces
        private List<string> _baseWindowNamespaces =
        [
            "JobPlaytimeTracker.JobPlaytimeTracker.Windows"
        ];

        private List<string> _baseCommandNamespaces =
        [
            "JobPlaytimeTracker.JobPlaytimeTracker.Commands"
        ];

        /*
         * Service classes to access FFXIV elements through Dalamud.
         */
        [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static ITextureProvider TextureProvider { get; private set; } = null!;
        [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] public static IClientState ClientState { get; private set; } = null!;
        [PluginService] public static IDataManager DataManager { get; private set; } = null!;
        [PluginService] public static IPluginLog Log { get; private set; } = null!;
        [PluginService] public static IChatGui ChatGui { get; private set; } = null!;
        public static PluginContext Context { get; } = new();
        public static MetricsDatabase Metrics { get; private set; }

        /// <summary>
        /// Initializes a new instance of the JobPlaytimeTrackerPlugin class.
        /// </summary>
        /// [RequiredVersion("1.0")] IDalamudPluginInterface pluginInterface,
        public JobPlaytimeTrackerPlugin(IDalamudPluginInterface pluginInterface,
                                        ITextureProvider textureProvider,
                                        ICommandManager commandManager,
                                        IClientState clientState,
                                        IDataManager dataManager,
                                        IPluginLog log,
                                        IChatGui chatGui)
        {
            // Initialize objects
            JobPlaytimeTrackerPlugin.Context.Initialize();

            // Register plugin entities
            LoadWindows();
            LoadCommands();

            // Register delegates
            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenMainUi += delegate { new DisplayMainWindow().OnExecuteHandler("", ""); };
            PluginInterface.UiBuilder.OpenConfigUi += delegate { new DisplayConfigurationWindow().OnExecuteHandler("", ""); };
        }

        

        /// <summary>
        /// Uses reflection to identify and load all objects that implement the BaseCommand class or one of its derivatives.
        /// </summary>
        /// <param name="TargetNamespaces">A list of target namespaces to be scanned for commands to include in the plugin.</param>
        private void LoadCommands()
        {
            IEnumerable<ICommand> commandInstances = Assembly.GetExecutingAssembly()
                                                                .GetTypes()
                                                                .Where(type => _baseCommandNamespaces.Contains(type.Namespace ?? "") &&
                                                                               typeof(ICommand).IsAssignableFrom(type) &&
                                                                               type.IsClass &&
                                                                               !type.IsAbstract).Select(type => (ICommand)Activator.CreateInstance(type)!);

            foreach (ICommand command in commandInstances)
            {
                CommandManager.AddHandler(command.CommandName, command.OnExecute);
            }
        }

        /// <summary>
        /// Uses reflection to identify and load all objects that implement the BaseWindow class or one of its derivatives. If exception is thrown, then PluginContext was not initialized. Call PluginContext.Initialize().
        /// </summary>
        /// <exception cref="UninitializedPluginEntityException">Throws when Context.PluginWindows is null.</exception>
        private void LoadWindows()
        {
            if (JobPlaytimeTrackerPlugin.Context.PluginWindows is not null)
            {
                IEnumerable<BaseWindow> windowInstances = Assembly.GetExecutingAssembly()
                                                                  .GetTypes()
                                                                  .Where(type => _baseWindowNamespaces.Contains(type.Namespace ?? "") &&
                                                                                 typeof(BaseWindow).IsAssignableFrom(type) &&
                                                                                 type.IsClass &&
                                                                                 !type.IsAbstract).Select(type => (BaseWindow)Activator.CreateInstance(type)!);

                foreach (BaseWindow window in windowInstances)
                {
                    JobPlaytimeTrackerPlugin.Context.PluginWindows.AddWindow(window);
                }
            }
            else
            {
                throw new UninitializedPluginEntityException("PluginWindows object not initialized in plugin Context.");
            }
        }

        /// <summary>
        /// Draws the plugin UI. If exception is thrown, then PluginContext was not initialized. Call PluginContext.Initialize().
        /// </summary>
        /// <exception cref="UninitializedPluginEntityException">Throws when Context.PluginWindows is null.</exception>
        private void DrawUI()
        {
            if (JobPlaytimeTrackerPlugin.Context.PluginWindows is not null)
            {
                JobPlaytimeTrackerPlugin.Context.PluginWindows.Draw();
            }
            else
            {
                throw new UninitializedPluginEntityException("PluginWindows object not initialized in plugin Context.");
            }
        }

        /// <summary>
        /// Disposes of all plugin entities. If exception is thrown, then PluginContext was not initialized. Call PluginContext.Initialize().
        /// </summary>
        /// <exception cref="UninitializedPluginEntityException">Throws when Context.PluginWindows is null.</exception>
        public void Dispose()
        {
            if (JobPlaytimeTrackerPlugin.Context.PluginWindows is not null)
            {
                // Dispose of all windows and clear window system
                foreach (BaseWindow window in JobPlaytimeTrackerPlugin.Context.PluginWindows.Windows)
                {
                    window.Dispose();
                }
                JobPlaytimeTrackerPlugin.Context.PluginWindows.RemoveAllWindows();

                // Dispose of all commands
                IEnumerable<ICommand> commandInstances = Assembly.GetExecutingAssembly()
                                                        .GetTypes()
                                                        .Where(type => _baseCommandNamespaces.Contains(type.Namespace ?? "") &&
                                                                       typeof(ICommand).IsAssignableFrom(type) &&
                                                                       type.IsClass &&
                                                                       !type.IsAbstract).Select(type => (ICommand)Activator.CreateInstance(type)!);
                foreach (ICommand command in commandInstances)
                {
                    CommandManager.RemoveHandler(command.CommandName);
                }

                // Dispose of in-memory database
                // TODO: SAVE DATABASE BEFORE DISPOSE
                JobPlaytimeTrackerPlugin.Metrics.Dispose();
            }
            else
            {
                throw new UninitializedPluginEntityException("PluginWindows object not initialized in plugin Context.");
            }
        }

        public MetricsDatabase LoadInMemoryDatabase()
        {
            // Initialize connection to in-memory database
            SqliteConnection inMemoryDatabase = new SqliteConnection("Data Source=:memory:");
            inMemoryDatabase.Open();

            // Initialize connection to backup SQLite database file
            SqliteConnection backupDatabase = new SqliteConnection($"Data Source={Paths.DatabaseBackup}");
            backupDatabase.Open();
        }
    }
}
