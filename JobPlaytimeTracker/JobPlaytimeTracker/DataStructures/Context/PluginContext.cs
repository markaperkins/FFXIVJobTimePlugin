using Dalamud.Interface.Windowing;
using System.Reflection;

namespace JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Context
{
    internal class PluginContext
    {
        public WindowSystem? PluginWindows { get; private set; }
        public Configuration? PluginConfiguration { get; private set; }

        public void Initialize()
        {
            PluginWindows = new(Assembly.GetExecutingAssembly().GetName().Name!);
            PluginConfiguration = JobPlaytimeTrackerPlugin.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        }
    }
}
