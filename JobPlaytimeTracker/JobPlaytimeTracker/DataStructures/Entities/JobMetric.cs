using JobPlaytimeTracker.JobPlaytimeTracker.Enums;
using JobPlaytimeTracker.Legos.Attributes;
using JobPlaytimeTracker.Legos.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Entities
{
    public class JobMetric : IMetricSource
    {
        public FFXIVJob JobID { get; set; }

        // Displayable Metrics
        [DisplayableMetric("Total Time Played")]
        [DisplayOrder(1)]
        public TimeSpan TimePlayed { get; set; } = TimeSpan.Zero;

        [DisplayableMetric("Time in Combat/Crafting")]
        [DisplayOrder(2)]
        public TimeSpan TimeActive { get; set; } = TimeSpan.Zero;

        [DisplayableMetric("Time AFK")]
        [DisplayOrder(3)]
        public TimeSpan TimeAFK { get; set; } = TimeSpan.Zero;

        [DisplayableMetric("Number of Deaths")]
        [DisplayOrder(4)]
        public uint NumberOfDeaths { get; set; } = 0;

        public JobMetric()
        {
            JobID = FFXIVJob.None;
        }

        public JobMetric(FFXIVJob id)
        {
            JobID = id;
        }

        public void AddTimeAFK(TimeSpan newAFKTime)
        {
            TimeAFK = TimeAFK + newAFKTime;
        }

        public void AddTimeActive(TimeSpan newActiveTime)
        {
            TimeActive = TimeActive + newActiveTime;
        }

        public void AddTimePlayed(TimeSpan newPlayedTime)
        {
            TimePlayed = TimePlayed + newPlayedTime;
        }

        public void AddDeath()
        {
            NumberOfDeaths++;
        }

        /******************************************************************************************************************
         *                                                                                                                *
         * This section of code overrides and defines various operators and equality checks. Equality operators, such as  *
         * == and != were intended to be used to compare every property inside of the JobMetric object. This is done      *
         * using reflection, to avoid having to continuously update the function as I add additional metrics in the       *
         * future. It is not time efficient, but is intended to be used for a recovery/file validation feature I plan to  *
         * add in a future update. It is unlikely that, under normal use, two JobMetric objects would ever have each      *
         * property be equal due to the TimeSpan properties.                                                              *
         *                                                                                                                *
         * If you need to evaluate whether two JobMetric objects reference the same job, use the Equals function. This    *
         * will be maintained to also include comparisons of any future properties that may uniquely identify a           *
         * JobMetric, such as a character or account ID.                                                                  *
         *                                                                                                                *
         ******************************************************************************************************************/

        /// <summary>
        /// <para>Evaluates whether two JobMetric objects are uniquely equal (all unique identifiers are equal).</para>
        /// <para>This does not evaluate property equality. For a full explanation on the intended uses of these functions and operators, please see the documentation inside of the JobMetric class.</para>
        /// </summary>
        /// <param name="obj">Object to compare with.</param>
        /// <returns>True if both objects are uniquely equal.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is not null && obj is JobMetric other)
            {
                return JobID == other.JobID;
            }

            // Only possible to reach if obj is null or not a JobMetric object, making equality impossible.
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(JobID, TimePlayed, TimeActive, TimeAFK);
        }

        /// <summary>
        /// <para>Uses reflection to evaluate whether every property in the objects are equal.</para>
        /// <para>If you are looking to evaluate whether two JobMetrics reference the same unique job, use the Equals function.</para>
        /// <para>This function is not time efficient and is limited in functional scope, since it will almost always return false. Please read the documentation inside of the JobMetric class for a full explanation on its use.</para>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>True if every property is equal.</returns>
        public static bool operator ==(JobMetric left, JobMetric right)
        {
            // Gate functions. Left and Right nullability is ignored with !. It is only possible to pass the gate functions if both left and right are not null.
            if (left is null && right is null) return true;
            if (left is not null && right is null) return false;
            if (left is null && right is not null) return false;

            // Control variables
            PropertyInfo[] properties = left!.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            bool allPropertiesEqual = true;
            int index = 0;

            // Compare properties for equality until a mismatch is found or there are no more properties
            while (index < properties.Length && allPropertiesEqual)
            {
                var leftProperty = properties[index].GetValue(left);
                var rightProperty = properties[index].GetValue(right);

                // Do nothing if both left and right are null. This would result in allPropertiesEqual &= true, which is a wasted operation.
                if (leftProperty is null && rightProperty is not null || leftProperty is not null && rightProperty is null)
                {
                    allPropertiesEqual = false;
                }
                else if (left is not null && right is not null)
                {
                    allPropertiesEqual = allPropertiesEqual && left.Equals(right);
                }
            }

            return allPropertiesEqual;
        }

        /// <summary>
        /// <para>Uses reflection to evaluate whether every property in the objects are equal.</para>
        /// <para>If you are looking to evaluate whether two JobMetrics reference the same unique job, use the Equals function.</para>
        /// <para>This function is not time efficient and is limited in functional scope, since it will almost always return true. Please read the documentation inside of the JobMetric class for a full explanation on its use.</para>
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>True if at least one property is not equal.</returns>
        public static bool operator !=(JobMetric left, JobMetric right)
        {
            return !(left == right);
        }
    }
}
