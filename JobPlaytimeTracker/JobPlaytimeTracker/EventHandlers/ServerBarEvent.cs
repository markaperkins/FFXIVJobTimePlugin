using Dalamud.Game.Gui.Dtr;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Context;
using JobPlaytimeTracker.JobPlaytimeTracker.Windows;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobPlaytimeTracker.JobPlaytimeTracker.EventHandlers
{
    internal class ServerBarEvent
    {
        private readonly IDtrBarEntry _serverBarEntry;
        private PluginContext _context;

        public ServerBarEvent(PluginContext context)
        {
            _context = context;
            _serverBarEntry = _context.DtrBar.Get("JobPlaytimeTracker");
            _serverBarEntry.Text = "Loading...";
            _serverBarEntry.Shown = true;
            _serverBarEntry.OnClick += ((_) => OnClick());
        }

        public void OnTick(IFramework framework)
        {
            TimeSpan timeElapsed = _context.CurrentElapsedTime();
            string jobAbbreviation = _context.DataManager.GetExcelSheet<ClassJob>().GetRow((uint)_context.CurrentJob).Abbreviation.ToString();
            _serverBarEntry.Text = $"Time in Job ({jobAbbreviation}): {timeElapsed.ToString(@"hh\:mm\:ss")}";
        }

        private void OnClick()
        {
            // Attempt to find reference to main window
            Window? mainWindow = _context.PluginWindows!.Windows.Where(window => window.GetType() == typeof(MainScreen))
                                                                .FirstOrDefault();

            // If reference is found, set the current selected job and then toggle window to display
            if (mainWindow is not null)
            {
                MainScreen windowAsMainScreen = (MainScreen)mainWindow;

                windowAsMainScreen.CurrentSelectedJobIndex = (int)_context.CurrentJob;
                windowAsMainScreen.Toggle();
            }
        }
    }
}
