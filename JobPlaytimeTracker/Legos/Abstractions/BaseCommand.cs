using Dalamud.Game.Command;
using JobPlaytimeTracker.JobPlaytimeTracker.DataStructures.Context;
using JobPlaytimeTracker.Legos.Interface;

namespace JobPlaytimeTracker.Legos.Abstractions
{
    internal abstract class BaseCommand : ICommand
    {
        public abstract string CommandName { get; }
        public abstract string HelpMessage { get; }
        public CommandInfo OnExecute { get; }
        protected PluginContext Context { get; }

        public BaseCommand(PluginContext context)
        {
            OnExecute = new CommandInfo(OnExecuteHandler);
            OnExecute.HelpMessage = HelpMessage;

            Context = context;
        }

        public abstract void OnExecuteHandler(string command, string args);
    }
}
