using JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Context;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobPlaytimeTracker.JobPlaytimeTracker.Watchers
{
    internal class PlayerHealthWatcher : INotifyPropertyChanged
    {
        private PluginContext _pluginContext;

        public PlayerHealthWatcher(PluginContext context) 
        { 
            _pluginContext = context;
        }

        public uint Health
        {
            get
            {
                return _pluginContext.ClientState.LocalPlayer?.CurrentHp ?? 0;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
