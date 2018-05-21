using Cadenza.Collections;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace SanAndreasAPI
{
    /// <summary>
    /// Class Logger.
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// The path
        /// </summary>
        public string path;

        /// <summary>
        /// The save on going
        /// </summary>
        public bool saveOnGoing;

        public bool isUnity;

        private StringBuilder sb;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="sog">if set to <c>true</c> [sog].</param>
        public Logger(string p, bool iu, bool sog = true)
        {
            path = p;
            saveOnGoing = sog;
            isUnity = iu;
        }

        /// <summary>
        /// Logs this instance.
        /// </summary>
        public void Log()
        {
            Log("");
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="pars">The pars.</param>
        public void Log(string message, string[] stack = null, params object[] pars)
        {
            bool stackNotNull = stack != null && stack.Length > 0;
            string str = stackNotNull ? string.Format(message, pars) : message,
                   msg = !stackNotNull ? str.DetailedMessage(null, LogType.Log) : str.DetailedMessage(stack, LogType.Log);

            try
            { //Probamos a hacer un debug...
                if (!isUnity)
                    Console.WriteLine(msg);
                else
                    Debug.Log(str);
            }
            catch
            { //Si hay problemas mostramos el por defecto.
                Console.WriteLine(msg);
            }
            AppendLine(msg);
        }

        /// <summary>
        /// Logs the warning.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="pars">The pars.</param>
        public void LogWarning(string message, string[] stack = null, params object[] pars)
        {
            bool stackNotNull = stack != null && stack.Length > 0;
            string str = stackNotNull ? string.Format(message, pars) : message,
                   msg = !stackNotNull ? str.DetailedMessage(null, LogType.Warning) : str.DetailedMessage(stack, LogType.Warning);

            try
            {
                if (!isUnity)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(msg);
                    Console.ResetColor();
                }
                else
                    Debug.LogWarning(str);
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(msg);
                Console.ResetColor();
            }
            AppendLine(msg);
        }

        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="pars">The pars.</param>
        public void LogError(string message, string[] stack = null, params object[] pars)
        {
            bool stackNotNull = stack != null && stack.Length > 0;
            string str = stackNotNull ? string.Format(message, pars) : message,
                   msg = !stackNotNull ? str.DetailedMessage(null, LogType.Error) : str.DetailedMessage(stack, LogType.Error);

            try
            {
                if (!isUnity)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(msg);
                    Console.ResetColor();
                }
                else
                    Debug.LogError(str);
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(msg);
                Console.ResetColor();
            }
            AppendLine(msg);
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public void Save()
        {
        }

        /// <summary>
        /// Saves to file.
        /// </summary>
        public void SaveToFile()
        {
        }

        internal void AppendLine(string str)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (!File.Exists(path))
                    File.Create(path).Dispose();
                File.AppendAllText(path, str + Environment.NewLine);
            }
        }

        public void SmartLog(ConsoleLog log)
        {
            switch (log.logType)
            {
                case LogType.Log:
                    Log(log.logString, log.stackTrace);
                    break;

                case LogType.Warning:
                    LogWarning(log.logString, log.stackTrace);
                    break;

                case LogType.Error:
                    LogError(log.logString, log.stackTrace);
                    break;
            }
        }
    }

    public static class LoggerExtensions
    {
        public static string DetailedMessage(this ConsoleLog log)
        {
            return DetailedMessage(log.logString, log.stackTrace, log.logType);
        }

        public static string DetailedMessage(this string message, string[] stacktrace, LogType t)
        {
            bool stackNotNull = stacktrace != null && stacktrace.Length > 0 && !stacktrace.All(x => string.IsNullOrEmpty(x));

            string name = Thread.CurrentThread.Name;
            string str = string.Format("[{0}] {1}/{2}: ", DateTime.Now.ToString("hh:mm:ss"), string.IsNullOrEmpty(name) ? "Main" : name, t.ToString());

            bool printStackTrace = stackNotNull && t != LogType.Log;

            string stack = printStackTrace ? IndentLog(stacktrace, str.Length, "StackTrace: ") : "";
            message = IndentLog(message, str.Length);

            return string.Format("{0}{1}{2}", str,
                message.LastIndexOf(Environment.NewLine) == message.Length - 1 ? message.TrimEnd('\n') : message,
                printStackTrace ? Environment.NewLine + stack : "");
        }

        private static string IndentLog(string str, int len)
        {
            if (str.Contains(Environment.NewLine))
            {
                StringBuilder sb = new StringBuilder();

                str.Split('\n').ForEach((x, i) =>
                {
                    if (!string.IsNullOrEmpty(x))
                        sb.AppendLine(string.Format("{0}{1}", i > 0 ? new string(' ', len) : "", x));
                });

                return sb.ToString();
            }
            return str;
        }

        private static string IndentLog(string[] strs, int len, string firstLine = "")
        {
            StringBuilder sb = new StringBuilder();

            strs.ForEach((x, i) =>
            {
                if (!string.IsNullOrEmpty(x))
                    sb.AppendLine(string.Format("{0}{1}", i > 0 ? new string(' ', len) : "", x));
            });

            int l = len - firstLine.Length;
            return (string.IsNullOrEmpty(firstLine) ? "" : ((l > 0 ? new string(' ', l) : "") + firstLine)) + sb.ToString();
        }

        public static string[] GetLines(this string str)
        {
            if (str.Contains(Environment.NewLine))
                return str.Split('\n');
            return new string[1] { str };
        }
    }

    public class LoggerMessage
    {
        public string message;
        public string[] stacktrace;

        public LoggerMessage(string str, string[] stc)
        {
            message = str;
            stacktrace = stc;
        }
    }
}