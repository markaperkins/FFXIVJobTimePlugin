using ImGuiNET;
using JobPlaytimeTracker.JobPlaytimeTracker.Enums;
using JobPlaytimeTracker.Legos.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace JobPlaytimeTracker.JobPlaytimeTracker.Windows
{
    internal class MainScreen : BaseWindow
    {
        // Parameters to initialize the screen through the base constructor
        private const string _windowName = "Job Playtime Tracker##Mainscreen";
        private const ImGuiWindowFlags _windowFlags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        private const bool _forceMainWindow = true;

        // Window configuration
        private int WindowWidth = 700;
        private int WindowHeight = 500;

        // Instance variables
        private int _currentSelectedJobIndex;

        public MainScreen() : base(name: _windowName, 
                                   flags: _windowFlags,
                                   forceMainWindow: _forceMainWindow)
        {
            // Initialize variables
            _currentSelectedJobIndex = 0;

            // Configure window display
            Size = new Vector2(WindowWidth, WindowHeight);
            SizeCondition = ImGuiCond.Always;
        }

        public override void Dispose()
        {
        }

        public override void Draw()
        {
            /***************************************************************************************************
             * START OF HEADER SECTION OF UI                                                                   *
             ***************************************************************************************************/
            // Create a label for the job selection combo box
            ImGui.SetCursorPos(new Vector2(15, 30));
            ImGui.Text("Select a Job:");

            // Create job selection combo box
            ImGui.SetCursorPos(new Vector2(95, 27));
            ImGui.SetNextItemWidth(590);
            string[] jobList = GenerateJobList();
            ImGui.Combo("##cmbJobSelector-Mainscreen", ref _currentSelectedJobIndex, jobList, jobList.Count());

            // Draw a seperator to split the UI between the selection box and display
            ImGui.SetCursorPos(new Vector2(0, 55));
            ImGui.Separator();

            /***************************************************************************************************
             * Create metric table based on selection.                                                         *
             ***************************************************************************************************/
            ImGui.SetCursorPos(new Vector2(5, 65));
            ImGui.SetNextItemWidth(215);
            if(ImGui.BeginTable("##tblLayout", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersOuterV, new Vector2(450, 325)))
            {
                // Configure table
                ImGui.TableSetupColumn("Metric", ImGuiTableColumnFlags.WidthFixed, 165.0f);
                ImGui.TableSetupColumn("Stat", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableHeadersRow();

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text("Total Job Playtime");
                ImGui.TableSetColumnIndex(1);
                ImGui.Text("4 seconds");

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text("Total Active Playtime");
                ImGui.TableSetColumnIndex(1);
                ImGui.Text("1 seconds");

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text("Total AFK");
                ImGui.TableSetColumnIndex(1);
                ImGui.Text("3 seconds");

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text("Total Deaths");
                ImGui.TableSetColumnIndex(1);
                ImGui.Text("12");
            } 
            ImGui.EndTable();

            /***************************************************************************************************
             * Create seperator underneath metric table                                                        *
             ***************************************************************************************************/
            ImGui.SetCursorPos(new Vector2(0, 400));
            ImGui.Separator();
        }

        private string[] GenerateJobList()
        {
            List<string> JobList = new List<string>();
            for (int jobID = 0; jobID < Enum.GetValues(typeof(FFXIVJob)).Length; jobID++)
            {
                // Create BaseJob object to access name and abbreviation for display
                BaseJob toList = new BaseJob((FFXIVJob)jobID);
                JobList.Add(toList.ToString()); 
            }

            return JobList.ToArray();
        }
    }
}
