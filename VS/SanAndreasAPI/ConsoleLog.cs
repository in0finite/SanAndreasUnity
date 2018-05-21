using System;
using UnityEngine;

namespace SanAndreasAPI
{
    [Serializable]
    public class ConsoleLog
    {
        public string logString;
        public string stackTrace;
        public LogType logType;

        private ConsoleLog()
        {
        }

        public ConsoleLog(string logString, string stackTrace, LogType logType)

        {
            this.logString = logString;
            this.stackTrace = stackTrace;
            this.logType = logType;
        }
    }
}