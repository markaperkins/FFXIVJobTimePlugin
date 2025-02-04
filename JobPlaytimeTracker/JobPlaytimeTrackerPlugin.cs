using Dalamud.Configuration;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using JobPlaytimeTracker.JobPlaytimeTracker.Commands;
using JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Context;
using JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Entities;
using JobPlaytimeTracker.JobPlaytimeTracker.Enums;
using JobPlaytimeTracker.JobPlaytimeTracker.EventHandlers;
using JobPlaytimeTracker.JobPlaytimeTracker.Exceptions;
using JobPlaytimeTracker.Legos.Abstractions;
using JobPlaytimeTracker.Legos.Interface;
using JobPlaytimeTracker.Resources.Strings;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

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
        [PluginService] public static ICondition Conditions { get; private set; } = null!;
        [PluginService] public static IFramework Framework { get; private set; } = null!;
        [PluginService] public static IDtrBar DtrBar { get; private set; } = null!;

        // Instance objects and variables
#pragma warning disable CS8618 // Due to when Dalamud injects the plugin services, PluginContext cannot be initialized inside of the static constructor. An approximation is made by instead having it initialized first in the instance constructor.
        private static PluginContext Context { get; set; }
#pragma warning restore CS8618
        private Action _displayMainWindow;
        private Action _displayConfigWindow;
        private ServerBarEvent _serverBarEventHandler;

        /// <summary>
        /// Initializes a new instance of the JobPlaytimeTrackerPlugin class.
        /// </summary>
        /// [RequiredVersion("1.0")] IDalamudPluginInterface pluginInterface,
        public JobPlaytimeTrackerPlugin()
        {
            // Initialize plugin context
            Context = new PluginContext(PluginInterface,
                                        TextureProvider,
                                        CommandManager,
                                        ClientState,
                                        DataManager,
                                        Log,
                                        ChatGui,
                                        Conditions,
                                        Framework,
                                        DtrBar);
            Context.UpdatePlayer(Player.LoadPlayer(Context, ClientState.LocalPlayer?.Name.ToString() ?? "Unknown"));

            // Initialize instance objects and variables
            _displayMainWindow = delegate { new DisplayMainWindow(Context).OnExecuteHandler("", ""); };
            _displayConfigWindow = delegate { new DisplayConfigurationWindow(Context).OnExecuteHandler("", ""); };
            _serverBarEventHandler = new ServerBarEvent(Context);

            // Validate plugin setup
            if (Directory.Exists(Paths.MetricsDirectory) == false) Directory.CreateDirectory(Paths.MetricsDirectory);

            // Register plugin entities
            LoadWindows();
            LoadCommands();

            // Register delegates
            Context.PluginInterface.UiBuilder.Draw += DrawUI;
            Context.PluginInterface.UiBuilder.OpenMainUi += _displayMainWindow;
            Context.PluginInterface.UiBuilder.OpenConfigUi += _displayConfigWindow;
            Context.ClientState.ClassJobChanged += Context.PlayerEventHandler.OnJobChange;
            Context.ClientState.Login += Context.UpdatePlayer;
            Context.ClientState.Logout += Player.OnLogout;
            Context.Conditions.ConditionChange += Context.PlayerEventHandler.OnConditionChange;
            Context.Framework.Update += Context.PlayerEventHandler.OnTick;
            Context.Framework.Update += _serverBarEventHandler.OnTick;
        }

        /// <summary>
        /// Allows an instance to request the current context. This is necessary for deserialization constructors.
        /// </summary>
        /// <returns></returns>
        public static PluginContext RequestContext()
        {
            return Context;
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
                                                                               !type.IsAbstract).Select(type => (ICommand)Activator.CreateInstance(type, Context)!);

            foreach (ICommand command in commandInstances)
            {
                Context.CommandManager.AddHandler(command.CommandName, command.OnExecute);
            }
        }

        /// <summary>
        /// Uses reflection to identify and load all objects that implement the BaseWindow class or one of its derivatives. If exception is thrown, then PluginContext was not initialized. Call PluginContext.Initialize().
        /// </summary>
        /// <exception cref="UninitializedPluginEntityException">Throws when Context.PluginWindows is null.</exception>
        private void LoadWindows()
        {
            if (Context.PluginWindows is not null)
            {
                IEnumerable<BaseWindow> windowInstances = Assembly.GetExecutingAssembly()
                                                                  .GetTypes()
                                                                  .Where(type => _baseWindowNamespaces.Contains(type.Namespace ?? "") &&
                                                                                 typeof(BaseWindow).IsAssignableFrom(type) &&
                                                                                 type.IsClass &&
                                                                                 !type.IsAbstract).Select(type => (BaseWindow)Activator.CreateInstance(type, Context)!);

                foreach (BaseWindow window in windowInstances)
                {
                    Context.PluginWindows.AddWindow(window);
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
            if (Context.PluginWindows is not null)
            {
                Context.PluginWindows.Draw();
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
            // Remove delegates
            Context.ClientState.ClassJobChanged -= Context.PlayerEventHandler.OnJobChange;
            Context.ClientState.Login -= Context.UpdatePlayer;
            Context.ClientState.Logout -= Player.OnLogout;
            Context.Conditions.ConditionChange -= Context.PlayerEventHandler.OnConditionChange;
            Context.Framework.Update -= _serverBarEventHandler.OnTick;
            Context.Framework.Update -= Context.PlayerEventHandler.OnTick;
            Context.PluginInterface.UiBuilder.Draw -= DrawUI;
            Context.PluginInterface.UiBuilder.OpenMainUi -= _displayMainWindow;
            Context.PluginInterface.UiBuilder.OpenConfigUi -= _displayConfigWindow;

            // Save TimeSpan metrics
            ClientStateEvent dummyClientStateUpdated = new ClientStateEvent(Context);
            dummyClientStateUpdated.OnJobChange((uint)FFXIVJob.None); // Save played time

            // Save metrics
            Player.SavePlayer(Context.CurrentPlayer!);

            // Save configuration
            Context.PluginConfiguration!.Save();

            if (Context.PluginWindows is not null)
            {
                // Dispose of all windows and clear window system
                foreach (BaseWindow window in Context.PluginWindows.Windows)
                {
                    window.Dispose();
                }
                Context.PluginWindows.RemoveAllWindows();

                // Dispose of all commands
                IEnumerable<ICommand> commandInstances = Assembly.GetExecutingAssembly()
                                                        .GetTypes()
                                                        .Where(type => _baseCommandNamespaces.Contains(type.Namespace ?? "") &&
                                                                       typeof(ICommand).IsAssignableFrom(type) &&
                                                                       type.IsClass &&
                                                                       !type.IsAbstract).Select(type => (ICommand)Activator.CreateInstance(type, Context)!);
                foreach (ICommand command in commandInstances)
                {
                    Context.CommandManager.RemoveHandler(command.CommandName);
                }
            }
            else
            {
                throw new UninitializedPluginEntityException("PluginWindows object not initialized in plugin Context.");
            }
        }
    }
}
