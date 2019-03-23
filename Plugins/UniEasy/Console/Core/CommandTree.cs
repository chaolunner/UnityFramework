using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace UniEasy.Console
{
    public class CommandTree : IEnumerable<CommandAttribute>
    {
        private Dictionary<string, CommandTree> SubCommands;
        private List<string> RemoveCommands;
        private CommandAttribute Command;

        private static string CommandNotFound = "command not found";

        public CommandTree()
        {
            SubCommands = new Dictionary<string, CommandTree>();
            RemoveCommands = new List<string>();
        }

        public void Add(CommandAttribute cmd)
        {
            Add(cmd.Command.ToLower().Split(' '), 0, cmd);
        }

        private void Add(string[] commands, int index, CommandAttribute cmd)
        {
            if (commands.Length == index)
            {
                Command = cmd;
                return;
            }

            string token = commands[index];
            if (!SubCommands.ContainsKey(token))
            {
                SubCommands[token] = new CommandTree();
            }
            SubCommands[token].Add(commands, index + 1, cmd);
        }

        public void Remove(string commandStr)
        {
            Remove(commandStr.ToLower().Split(' '), 0);
        }

        private void Remove(string[] commands, int index)
        {
            if (commands.Length == index)
            {
                Command = null;
                return;
            }

            string token = commands[index];
            if (SubCommands.ContainsKey(token))
            {
                SubCommands[token].Remove(commands, index + 1);
            }
        }

        public void RemoveEmptyCommands()
        {
            foreach (KeyValuePair<string, CommandTree> entry in SubCommands)
            {
                if (entry.Value.Command == null && entry.Value.SubCommands.Count == 0)
                {
                    RemoveCommands.Add(entry.Key);
                }
                foreach (KeyValuePair<string, CommandTree> subEntry in entry.Value.SubCommands)
                {
                    subEntry.Value.RemoveEmptyCommands();
                }
            }
            foreach (string key in RemoveCommands)
            {
                SubCommands.Remove(key);
            }
            RemoveCommands.Clear();
        }

        public IEnumerator<CommandAttribute> GetEnumerator()
        {
            if (Command != null && Command.Command != null)
            {
                yield return Command;
            }
            foreach (KeyValuePair<string, CommandTree> entry in SubCommands)
            {
                foreach (CommandAttribute cmd in entry.Value)
                {
                    if (cmd != null && cmd.Command != null)
                    {
                        yield return cmd;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string Complete(string partialCommand)
        {
            return Complete(partialCommand.Split(' '), 0, "");
        }

        public string Complete(string[] partialCommands, int index, string result)
        {
            if (partialCommands.Length == index && Command != null)
            {
                return result;
            }
            else if (partialCommands.Length == index)
            {
                Console.Log(result);
                foreach (string key in SubCommands.Keys.OrderBy(m => m))
                {
                    Console.Log(result + " " + key);
                }
                return result + " ";
            }
            else if (partialCommands.Length == (index + 1))
            {
                string partial = partialCommands[index];
                if (SubCommands.ContainsKey(partial))
                {
                    result += partial;
                    return SubCommands[partial].Complete(partialCommands, index + 1, result);
                }

                List<string> matches = new List<string>();
                foreach (string key in SubCommands.Keys.OrderBy(m => m))
                {
                    if (key.StartsWith(partial))
                    {
                        matches.Add(key);
                    }
                }

                if (matches.Count == 1)
                {
                    return result + matches[0] + " ";
                }
                else if (matches.Count > 1)
                {
                    Console.Log(result + partial);
                    foreach (string match in matches)
                    {
                        Console.Log(result + match);
                    }
                }
                return result + partial;
            }

            string token = partialCommands[index];
            if (!SubCommands.ContainsKey(token))
            {
                return result;
            }
            result += token + " ";
            return SubCommands[token].Complete(partialCommands, index + 1, result);
        }

        public string Run(string commandStr)
        {
            Regex regex = new Regex(@""".*?""|[^\s]+");
            MatchCollection matches = regex.Matches(commandStr);
            string[] tokens = new string[matches.Count];
            for (int i = 0; i < tokens.Length; ++i)
            {
                tokens[i] = matches[i].Value.Replace("\"", "");
            }
            string result = Run(tokens, 0);
            if (result == CommandNotFound)
            {
                result = "Command " + commandStr + " not found.";
            }
            return result;
        }

        static string[] emptyArgs = new string[0] { };

        private string Run(string[] commands, int index)
        {
            if (commands.Length == index)
            {
                return RunCommand(emptyArgs);
            }

            string token = commands[index].ToLower();
            if (!SubCommands.ContainsKey(token))
            {
                return RunCommand(commands.Skip(index).ToArray());
            }
            return SubCommands[token].Run(commands, index + 1);
        }

        private string RunCommand(string[] args)
        {
            if (Command == null)
            {
                return CommandNotFound;
            }
            else if (Command.RunOnMainThread)
            {
                Console.Queue(Command, args);
                return null;
            }
            else
            {
                return Command.Callback(args);
            }
        }
    }
}
