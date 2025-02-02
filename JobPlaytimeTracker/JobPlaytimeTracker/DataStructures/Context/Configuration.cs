using Dalamud.Configuration;
using Dalamud.Plugin;
using JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Entities;
using System;
using System.Collections.Generic;

namespace JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Context
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        // Exposed configuration properties
        public int Version { get; set; } = 0;
        public bool IsConfigWindowMovable { get; set; } = true;
        public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;

        // Instance objects and variables
        private PluginContext? _context;

        public Configuration()
        {
        }

        /// <summary>
        /// If a context has been assigned, save the configuration.
        /// </summary>
        public void Save()
        {
            if (_context is not null)
            {
                _context.PluginInterface.SavePluginConfig(this);
            }
        }

        /// <summary>
        /// Assigns an optional context. This enables saving the configuration.
        /// </summary>
        /// <param name="context"></param>
        public void AssignContext(PluginContext context)
        {
            _context = context;
        }
    }
}
