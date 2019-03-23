using System;

namespace UniEasy.Console
{
    public delegate string CommandCallback(params string[] args);

    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public string Command;
        public string Description;
        public string Usage;
        public bool RunOnMainThread;
        public CommandCallback Callback;
        public CommandDisposable Disposer;

        public CommandAttribute(string cmd, string description = "", string usage = "", bool runOnMainThread = true)
        {
            Command = cmd;
            Description = (string.IsNullOrEmpty(description.Trim()) ? "No description provided" : description);
            Usage = (string.IsNullOrEmpty(usage.Trim()) ? "No usage information provided" : usage);
            RunOnMainThread = runOnMainThread;
            Disposer = new CommandDisposable(Command);
        }
    }
}
