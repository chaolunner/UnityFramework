using UnityEngine;

namespace UniEasy.Console
{
    public struct LogData
    {
        public LogType LogType;
        public string Message;

        public LogData(string msg, LogType logType)
        {
            Message = msg;
            LogType = logType;
        }

        public override int GetHashCode()
        {
            return Message.GetHashCode() ^ LogType.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is LogData && ((LogData)obj) == this;
        }

        public static bool operator ==(LogData lhs, LogData rhs)
        {
            return lhs.Message == rhs.Message && lhs.LogType == rhs.LogType;
        }

        public static bool operator !=(LogData lhs, LogData rhs)
        {
            return lhs.Message != rhs.Message || lhs.LogType != rhs.LogType;
        }
    }
}
