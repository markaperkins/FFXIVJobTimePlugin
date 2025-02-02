using Dalamud.Interface.Windowing;
using JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Context;
using JobPlaytimeTracker.JobPlaytimeTracker.Windows;
using JobPlaytimeTracker.Legos.Abstractions;
using System.Linq;

namespace JobPlaytimeTracker.JobPlaytimeTracker.Commands
{
    internal class DisplayMainWindow : BaseCommand
    {
        public override string CommandName { get; } = "/jobplaytimetracker";
        public override string HelpMessage { get; } = "Displays the main window for the Job Playtime Tracker plugin.";

        public DisplayMainWindow(PluginContext context) : base(context)
        {
        }

        public override void OnExecuteHandler(string command, string args)
        {
            Window? mainWindow = _context.PluginWindows!.Windows.Where(window => window.GetType() == typeof(MainScreen))
                                                                                       .FirstOrDefault();

            if (mainWindow is not null)
            {
                mainWindow.Toggle();
            }
        }
    }
}
