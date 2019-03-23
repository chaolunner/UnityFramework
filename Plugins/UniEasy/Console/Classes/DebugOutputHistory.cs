using System.Collections.Generic;

namespace UniEasy.Console
{
    public class DebugOutputHistory
    {
        private int MaxCapacity;
        private List<string> OutputHistory;

        public DebugOutputHistory(int maxCapacity)
        {
            OutputHistory = new List<string>(maxCapacity);
            MaxCapacity = maxCapacity;
        }

        public void Add(string output)
        {
            if (OutputHistory.Count == MaxCapacity)
            {
                OutputHistory.RemoveAt(0);
            }

            OutputHistory.Add(output);
        }

        public void Clear()
        {
            OutputHistory.Clear();
        }

        public string Output()
        {
            return string.Join("\n", OutputHistory.ToArray());
        }
    }
}
