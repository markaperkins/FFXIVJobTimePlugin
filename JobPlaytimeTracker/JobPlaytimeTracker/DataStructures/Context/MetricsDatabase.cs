using JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Entities;
using JobPlaytimeTracker.Resources.Strings;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;

namespace JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Context
{
    internal class MetricsDatabase : DbContext
    {
        // Database Tables
        public DbSet<JobMetric> JobMetrics { get; set; }

#pragma warning disable CS8618 // CS8618: Entity Framework Core will assign a value to DbSet fields.
        public MetricsDatabase(DbContextOptions<MetricsDatabase> options) : base(options)
#pragma warning restore CS8618
        {
        }
    }
}
