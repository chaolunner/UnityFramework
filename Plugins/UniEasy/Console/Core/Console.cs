using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;
using System;

namespace UniEasy.Console
{
    public class Console
    {
        private ConsoleInputHistory ConsoleInputHistory;
        private Queue<QueuedCommand> CommandQueue;
        private CommandTree Commands;

        private static int InputHistoryCapacity = 100;
        private static string CommandOutputPrefix = "> ";

        public delegate void CallbackHandler(params string[] args);

        private event CallbackHandler OnLog;

        private static Console instance;

        private Console()
        {
            ConsoleInputHistory = new ConsoleInputHistory(InputHistoryCapacity);
            CommandQueue = new Queue<QueuedCommand>();
            Commands = new CommandTree();
            RegisterAttributes();
        }

        public static Console Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Console();
                }
                return instance;
            }
        }

        public static ConsoleInputHistory InputHistory
        {
            get
            {
                return Instance.ConsoleInputHistory;
            }
        }

        public static void Update()
        {
            while (Instance.CommandQueue.Count > 0)
            {
                QueuedCommand cmd = Instance.CommandQueue.Dequeue();
                string result = cmd.Command.Callback(cmd.Args);
                if (!string.IsNullOrEmpty(result))
                {
                    Log(result);
                }
            }
        }

        public static void Queue(CommandAttribute command, string[] args)
        {
            QueuedCommand queuedCommand = new QueuedCommand();
            queuedCommand.Command = command;
            queuedCommand.Args = args;
            Instance.CommandQueue.Enqueue(queuedCommand);
        }

        public static IOrderedEnumerable<CommandAttribute> GetCommands()
        {
            return Instance.Commands.OrderBy(m => m.Command);
        }

        public static void Run(string str)
        {
            if (str.Length > 0)
            {
                LogCommand(str);
                string result = Instance.Commands.Run(str);
                if (!string.IsNullOrEmpty(result))
                {
                    Log(result);
                }
                Instance.ConsoleInputHistory.AddNewInputEntry(str);
            }
        }

        public static string Complete(string partialCommand)
        {
            return Instance.Commands.Complete(partialCommand);
        }

        public static void RegisterLog(CallbackHandler callback)
        {
            Instance.OnLog += callback;
        }

        public static void DeregisterLog(CallbackHandler callback)
        {
            Instance.OnLog -= callback;
        }

        public static void LogCommand(string command)
        {
            Log(CommandOutputPrefix + command);
        }

        public static void Log(string str)
        {
            if (Instance.OnLog != null)
            {
                Instance.OnLog(str);
            }
        }

        public static CommandDisposable RegisterCommand(string command, string description, string usage, CommandCallback callback, bool runOnMainThread = true)
        {
            if (command == null || command.Length == 0)
            {
                throw new Exception("Command String cannot be empty");
            }

            CommandAttribute cmd = new CommandAttribute(command, description, usage, runOnMainThread);
            cmd.Callback = callback;

            Instance.Commands.Add(cmd);
            return cmd.Disposer;
        }

        private void RegisterAttributes()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // HACK: IL2CPP crashes if you attempt to get the methods of some classes in these assemblies.
                if (assembly.FullName.StartsWith("System") || assembly.FullName.StartsWith("mscorlib"))
                {
                    continue;
                }
                foreach (var type in assembly.GetTypes())
                {
                    // FIXME add support for non-static methods (FindObjectByType?)
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    {
                        var attrs = method.GetCustomAttributes(typeof(CommandAttribute), true) as CommandAttribute[];
                        if (attrs.Length == 0)
                        {
                            continue;
                        }

                        var cb = Delegate.CreateDelegate(typeof(CommandCallback), method, false) as CommandCallback;
                        if (cb == null)
                        {
                            Debug.LogError(string.Format("Method {0}.{1} takes the wrong arguments for a console command.", type, method.Name));
                            continue;
                        }

                        // try with a bare action
                        foreach (var cmd in attrs)
                        {
                            if (string.IsNullOrEmpty(cmd.Command))
                            {
                                Debug.LogError(string.Format("Method {0}.{1} needs a valid command name.", type, method.Name));
                                continue;
                            }

                            cmd.Callback = cb;
                            Commands.Add(cmd);
                        }
                    }
                }
            }
        }

        [Command("Deregister", "Deregister a specific command.", "Deregister commands")]
        public static string DeregisterCommands(params string[] args)
        {
            string result = "";
            for (int i = 0; i < args.Length; i++)
            {
                Instance.Commands.Remove(args[i]);
                if (string.IsNullOrEmpty(result))
                {
                    result += string.Format("Try to remove command [{0}]", args[i]);
                }
                else
                {
                    result += Environment.NewLine + string.Format("Try to remove command [{0}]", args[i]);
                }
            }
            Instance.Commands.RemoveEmptyCommands();
            return result;
        }

        [Route("^/console/out$")]
        public static void Output(RequestContext context)
        {
            context.Response.WriteString(Debugger.History.Output());
        }

        [Route("^/console/run$")]
        public static void Run(RequestContext context)
        {
            string command = Uri.UnescapeDataString(context.Request.QueryString.Get("command"));

            if (!string.IsNullOrEmpty(command))
            {
                Console.Run(command);
            }
            context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
            context.Response.StatusDescription = "OK";
        }

        [Route("^/console/commandHistory$")]
        public static void History(RequestContext context)
        {
            string index = context.Request.QueryString.Get("index");
            string previous = null;

            if (!string.IsNullOrEmpty(index))
            {
                previous = Console.InputHistory.PreviousCommand(System.Int32.Parse(index));
            }
            context.Response.WriteString(previous);
        }

        [Route("^/console/complete$")]
        public static void Complete(RequestContext context)
        {
            string partialCommand = context.Request.QueryString.Get("command");

            string found = null;
            if (partialCommand != null)
            {
                found = Console.Complete(partialCommand);
            }
            context.Response.WriteString(found);
        }
    }
}
