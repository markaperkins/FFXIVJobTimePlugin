using Dalamud.Interface.Windowing;
using JobPlaytimeTracker.JobPlaytimeTracker.Windows;
using JobPlaytimeTracker.Legos.Abstractions;
using System.Linq;

namespace JobPlaytimeTracker.JobPlaytimeTracker.Commands
{
    internal class DisplayConfigurationWindow : BaseCommand
    {
        public override string CommandName { get; } = "/playtimeconfig";

        public override string HelpMessage { get; } = "Displays the configuration window for the Job Playtime Tracker plugin.";

        public override void OnExecuteHandler(string command, string args)
        {
            Window? configWindow = JobPlaytimeTrackerPlugin.Context.PluginWindows!.Windows.Where(window => window.GetType() == typeof(ConfigurationScreen))
                                                                                          .FirstOrDefault();

            if (configWindow is not null)
            {
                configWindow.Toggle();
            }
        }
    }
}
