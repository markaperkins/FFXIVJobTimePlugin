using Dalamud.Configuration;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs;
using JobPlaytimeTracker.JobPlaytimeTracker.Commands;
using JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Context;
using JobPlaytimeTracker.Legos.Abstractions;
using JobPlaytimeTracker.Legos.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
        public static WindowSystem PluginWindows = new(Assembly.GetExecutingAssembly().GetName().Name!);
        public static Configuration PluginConfiguation { get; } = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // Instance objects
        private PluginContext _context;

        /// <summary>
        /// Initializes a new instance of the JobPlaytimeTrackerPlugin class.
        /// </summary>
        public JobPlaytimeTrackerPlugin()
        {
            // Initialize objects
            _context = new PluginContext();

            // Register plugin entities
            LoadWindows();
            LoadCommands();

            // Register delegates
            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += delegate { new DisplayMainWindow(_context).OnExecuteHandler("", ""); };
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
        /// Uses reflection to identify and load all objects that implement the BaseWindow class or one of its derivatives.
        /// </summary>
        /// <param name="TargetNamespaces">A list of target namespaces to be scanned for commands to include in the plugin.</param>
        private void LoadWindows()
        {
            IEnumerable<BaseWindow> windowInstances = Assembly.GetExecutingAssembly()
                                                              .GetTypes()
                                                              .Where(type => _baseWindowNamespaces.Contains(type.Namespace ?? "") &&
                                                                             typeof(BaseWindow).IsAssignableFrom(type) &&
                                                                             type.IsClass &&
                                                                             !type.IsAbstract).Select(type => (BaseWindow)Activator.CreateInstance(type)!);

            foreach (BaseWindow window in windowInstances)
            {
                PluginWindows.AddWindow(window);
            }
        }

        /// <summary>
        /// Wrapper for the WindowSystem Draw function.
        /// </summary>
        private void DrawUI()
        {
            PluginWindows.Draw();
        }

        /// <summary>
        /// Disposes of all plugin entities and services as the plugin closes.
        /// </summary>
        public void Dispose()
        {
            // Dispose of all windows and clear window system
            foreach (BaseWindow window in PluginWindows.Windows)
            {
                window.Dispose();
            }
            PluginWindows.RemoveAllWindows();

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
        }
    }
}
