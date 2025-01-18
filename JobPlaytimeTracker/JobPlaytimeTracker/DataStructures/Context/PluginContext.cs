using Dalamud.Interface.Windowing;
using JobPlaytimeTracker.Resources.Strings;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;

namespace JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Context
{
    internal class PluginContext : IDisposable
    {
        // Fields to expose to any entity that is in scope of a reference to the PluginContext.
        public WindowSystem? PluginWindows { get; private set; }
        public Configuration? PluginConfiguration { get; private set; }
#pragma warning disable CS8618 // Initialized in the Initialize() function, which is called during plugin initialization.
        public MetricsDatabase DatabaseConnection { get; private set; }
#pragma warning restore CS8618

        // Instance objects and variables
#pragma warning disable CS8618 // Initialized in the Initialize() function, which is called during plugin initialization.
        private SqliteConnection _inMemoryDatabase;
        private SqliteConnection _backupDatabase;
#pragma warning restore CS8618



        public void Initialize()
        {
            // Initialize the plugin window system
            PluginWindows = new(Assembly.GetExecutingAssembly().GetName().Name!);

            // Restore or initialize the plugin configuration
            PluginConfiguration = JobPlaytimeTrackerPlugin.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

            // Initialize connection to in-memory database
            _inMemoryDatabase = new SqliteConnection("Data Source=:memory:");
            _inMemoryDatabase.Open();

            // Initialize connection to backup database
            _backupDatabase = new SqliteConnection($"Data Source={Paths.DatabaseBackup}");
            _backupDatabase.Open();

            // Create Entity Framework database connection
            DbContextOptions<MetricsDatabase> connectionOptions = new DbContextOptionsBuilder<MetricsDatabase>().UseSqlite(_inMemoryDatabase)
                                                                                                                .EnableSensitiveDataLogging()
                                                                                                                .EnableDetailedErrors()
                                                                                                                .Options;
            DatabaseConnection = new MetricsDatabase(connectionOptions);
        }

        public void Dispose()
        {
            _inMemoryDatabase.Close();
            _backupDatabase.Close();
        }

        /// <summary>
        /// Backs up the in-memory database to a file inside of the plugin's installation directory.
        /// </summary>
        public void BackupDatabase()
        {
            _inMemoryDatabase.BackupDatabase(_backupDatabase);
        }
    }
}
