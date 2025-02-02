using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobPlaytimeTracker.Legos.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class DisplayableMetric : Attribute
    {
        public string DisplayName { get; }

        public DisplayableMetric(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
