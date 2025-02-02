using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobPlaytimeTracker.Legos.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class DisplayOrder : Attribute
    {
        public int Order { get; } = 0;

        public DisplayOrder(int order)
        {
            Order = order;
        }
    }
}
