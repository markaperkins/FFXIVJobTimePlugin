using Dalamud.Interface.Windowing;
using ImGuiNET;
using JobPlaytimeTracker.Legos.Interfaces;

namespace JobPlaytimeTracker.Legos.Abstractions
{
    internal abstract class BaseWindow : Window, IWindow
    {
        protected BaseWindow(string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(name, flags, forceMainWindow)
        {
        }

        public abstract void Dispose();
    }
}
