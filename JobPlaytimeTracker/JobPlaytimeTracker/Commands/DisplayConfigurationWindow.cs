using JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Context;
using JobPlaytimeTracker.JobPlaytimeTracker.Windows;
using JobPlaytimeTracker.Legos.Abstractions;
using System.Linq;

namespace JobPlaytimeTracker.JobPlaytimeTracker.Commands
{
    internal class DisplayConfigurationWindow : BaseCommand
    {
        public override string CommandName { get; } = "/playtimeconfig";

        public override string HelpMessage { get; } = "Displays the configuration window for the Job Playtime Tracker plugin.";

        public DisplayConfigurationWindow(PluginContext context) : base(context)
        {
        }

        public override void OnExecuteHandler(string command, string args)
        {
            ConfigurationScreen? configWindow = _context.PluginWindows!.Windows.OfType<ConfigurationScreen>().FirstOrDefault();

            if (configWindow is not null)
            {
                configWindow.Toggle();
            }
        }
    }
}
