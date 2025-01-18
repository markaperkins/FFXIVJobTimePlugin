using ImGuiNET;
using JobPlaytimeTracker.Legos.Abstractions;
using System.Numerics;

namespace JobPlaytimeTracker.JobPlaytimeTracker.Windows
{
    internal class ConfigurationScreen : BaseWindow
    {
        // Parameters to initialize the screen through the base constructor
        private const string _windowName = "Job Playtime Tracker Configuration##ConfigurationScreen";
        private const ImGuiWindowFlags _windowFlags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        private const bool _forceMainWindow = true;

        // Window configuration
        private int xWidth = 232;
        private int yWidth = 90;

        public ConfigurationScreen() : base(name: _windowName, 
                                            flags: _windowFlags, 
                                            forceMainWindow: _forceMainWindow)
        {
            Size = new Vector2(xWidth, yWidth);
            SizeCondition = ImGuiCond.Always;
        }

        public override void Dispose() { }

        public override void PreDraw()
        {
            if (JobPlaytimeTrackerPlugin.Context.PluginConfiguration!.IsConfigWindowMovable)
            {
                Flags &= ~ImGuiWindowFlags.NoMove;
            }
            else
            {
                Flags |= ImGuiWindowFlags.NoMove;
            }
        }

        public override void Draw()
        {
            bool dummyConfigValue = JobPlaytimeTrackerPlugin.Context.PluginConfiguration!.SomePropertyToBeSavedAndWithADefault;
            if(ImGui.Checkbox("Random Config Bool", ref dummyConfigValue))
            {
                JobPlaytimeTrackerPlugin.Context.PluginConfiguration!.SomePropertyToBeSavedAndWithADefault = dummyConfigValue;
                JobPlaytimeTrackerPlugin.Context.PluginConfiguration!.Save();
            }

            bool movableConfigurationWindow = JobPlaytimeTrackerPlugin.Context.PluginConfiguration!.IsConfigWindowMovable;
            if (ImGui.Checkbox("Movable Config Window", ref movableConfigurationWindow))
            {
                JobPlaytimeTrackerPlugin.Context.PluginConfiguration!.IsConfigWindowMovable = movableConfigurationWindow;
                JobPlaytimeTrackerPlugin.Context.PluginConfiguration!.Save();
            }
        }
    }
}
