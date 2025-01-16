using Dalamud.Game.Command;
using JobPlaytimeTracker.Legos.Interface;

namespace JobPlaytimeTracker.Legos.Abstractions
{
    internal abstract class BaseCommand : ICommand
    {
        public abstract string CommandName { get; }
        public abstract string HelpMessage { get; }
        public CommandInfo OnExecute { get; }

        public BaseCommand()
        {
            OnExecute = new CommandInfo(OnExecuteHandler);
            OnExecute.HelpMessage = HelpMessage;
        }

        public abstract void OnExecuteHandler(string command, string args);
    }
}
