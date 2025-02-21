using System;
using System.IO;

namespace JobPlaytimeTracker.Resources.Strings
{
    internal class Paths
    {
        /// <summary>
        /// The root directory for the plugin's installation. In a default environment, this maps to %appdata%/Roaming/XIVLauncher/{PluginShortName}/
        /// </summary>
        public static readonly string PluginDirectory  = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                                                      About.PluginShortName);

        /// <summary>
        /// The backup database path, as PluginDirectory/metrics.db
        /// </summary>
        public static readonly string MetricsDirectory = Path.Combine(PluginDirectory, "metrics");
    }
}
