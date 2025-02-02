using System;

namespace JobPlaytimeTracker.JobPlaytimeTracker.Exceptions
{
    internal class UninitializedPluginEntityException : Exception
    {
        public UninitializedPluginEntityException() { }
        public UninitializedPluginEntityException(string message) : base(message) { }
        public UninitializedPluginEntityException(string message, Exception inner) : base(message, inner) { }
    }
}
