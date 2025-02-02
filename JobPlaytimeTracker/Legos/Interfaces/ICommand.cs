using Dalamud.Game.Command;

namespace JobPlaytimeTracker.Legos.Interface
{
    internal interface ICommand
    {
        public string CommandName { get; }
        public string HelpMessage { get; }
        public CommandInfo OnExecute { get; }
    }
}
