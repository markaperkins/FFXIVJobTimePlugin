using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using ImGuiNET;
using JobPlaytimeTracker.Legos.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobPlaytimeTracker.JobPlaytimeTracker.Windows
{
    internal class MainScreen : BaseWindow
    {
        // Parameters to initialize the screen through the base constructor
        private const string _windowName = "Job Playtime Tracker##Mainscreen";
        private const ImGuiWindowFlags _windowFlags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        private const bool _forceMainWindow = false;

        // Window configuration
        private int MinimumXValue = 375;
        private int MinimumYValue = 330;
        private int MaximumXValue = 2048;
        private int MaximumYValue = 2048;

        public MainScreen() : base(name: _windowName, 
                                   flags: _windowFlags,
                                   forceMainWindow: _forceMainWindow)
        {
            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new System.Numerics.Vector2(MinimumXValue, MinimumYValue),
                MaximumSize = new System.Numerics.Vector2(MaximumXValue, MaximumYValue)
            };
        }

        public override void Dispose()
        {
        }

        public override void Draw()
        {
            // Capture player information
            IPlayerCharacter player = JobPlaytimeTrackerPlugin.ClientState.LocalPlayer!;
            ImGui.TextUnformatted($"Our current job is ({player.ClassJob.RowId}) \"{player.ClassJob.Value.Abbreviation.ExtractText()}\"");
        }
    }
}
