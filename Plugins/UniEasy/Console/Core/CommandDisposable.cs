using System;

namespace UniEasy.Console
{
    public sealed class CommandDisposable : IDisposable
    {
        public string Command { get; private set; }

        internal CommandDisposable(string cmd)
        {
            Command = cmd;
        }

        public void Dispose()
        {
            Console.DeregisterCommands(Command);
        }
    }
}
