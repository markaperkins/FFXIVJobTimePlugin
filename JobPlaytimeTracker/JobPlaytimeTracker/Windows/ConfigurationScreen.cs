using ImGuiNET;
using JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Context;
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

        public ConfigurationScreen(PluginContext context) : base(context: context,
                                                                 name: _windowName, 
                                                                 flags: _windowFlags, 
                                                                 forceMainWindow: _forceMainWindow)
        {
            Context = context;

            // Configure window
            Size = new Vector2(xWidth, yWidth);
            SizeCondition = ImGuiCond.Always;
        }

        public override void Dispose() { }

        public override void PreDraw()
        {
            if (Context.PluginConfiguration!.IsConfigWindowMovable)
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
            bool dummyConfigValue = Context.PluginConfiguration!.SomePropertyToBeSavedAndWithADefault;
            if (ImGui.Checkbox("Random Config Bool", ref dummyConfigValue))
            {
                Context.PluginConfiguration!.SomePropertyToBeSavedAndWithADefault = dummyConfigValue;
                Context.PluginConfiguration!.Save();
            }

            bool movableConfigurationWindow = Context.PluginConfiguration!.IsConfigWindowMovable;
            if (ImGui.Checkbox("Movable Config Window", ref movableConfigurationWindow))
            {
                Context.PluginConfiguration!.IsConfigWindowMovable = movableConfigurationWindow;
                Context.PluginConfiguration!.Save();
            }
        }
    }
}
