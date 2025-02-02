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
        protected PluginContext _context;

        public BaseCommand(PluginContext context)
        {
            _context = context;
            OnExecute = new CommandInfo(OnExecuteHandler);
            OnExecute.HelpMessage = HelpMessage;
        }

        public abstract void OnExecuteHandler(string command, string args);
    }
}
