using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Context;
using JobPlaytimeTracker.Legos.Interfaces;

namespace JobPlaytimeTracker.Legos.Abstractions
{
    internal abstract class BaseWindow : Window, IWindow
    {
        internal PluginContext Context { get; set; }

        protected BaseWindow(PluginContext context, string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(name, flags, forceMainWindow)
        {
            Context = context;
        }

        public abstract void Dispose();
    }
}
