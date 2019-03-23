using System.Linq;
using System.Text;

namespace UniEasy.Console
{
    public class HelpCommand
    {
        public static readonly string Name = "Help";
        public static readonly string Description = "Display the list of available commands or details about a specific command.";
        public static readonly string Usage = "Help [command]";

        private static StringBuilder commandList = new StringBuilder();

        [Command("Help", "Display the list of available commands or details about a specific command.", "Help [command]")]
        public static string Execute(params string[] args)
        {
            if (args.Length == 0)
            {
                return DisplayAvailableCommands();
            }
            else
            {
                return DisplayCommandDetails(args[0]);
            }
        }

        private static string DisplayAvailableCommands()
        {
            commandList.Length = 0;
            commandList.Append("<b>Available Commands</b>\n");

            foreach (CommandAttribute cmd in Console.GetCommands())
            {
                commandList.Append(string.Format("    <b>{0}</b> - {1}\n", cmd.Command, cmd.Description));
            }

            commandList.Append("To display details about a specific command, type 'HELP' followed by the command name.");
            return commandList.ToString();
        }

        private static string DisplayCommandDetails(string command)
        {
            var formatting = @"<b>{0} Command</b> <b>Description:</b> {1} <b>Usage:</b> {2}";

            CommandAttribute cmd = Console.GetCommands().Where(m => m.Command.Equals(command, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (cmd != null)
            {
                return string.Format(formatting, cmd.Command, cmd.Description, cmd.Usage);
            }
            return string.Format("Cannot find help information about {0}. Are you sure it is a valid command?", command);
        }
    }
}
