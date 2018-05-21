using System;
using System.IO;
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
        public void Log(string message, params object[] pars)
        {
            bool parsNotNull = pars != null && pars.Length > 0;
            string str = parsNotNull ? message.TryFormat(pars) : message,
                   msg = !parsNotNull ? str.DetailedMessage("", LogType.Log) : str.DetailedMessage(pars[0].ToString(), LogType.Log);
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
        public void LogWarning(string message, params object[] pars)
        {
            bool parsNotNull = pars != null && pars.Length > 0;
            string str = parsNotNull ? message.TryFormat(pars) : message,
                   msg = !parsNotNull ? str.DetailedMessage("", LogType.Warning) : str.DetailedMessage(pars[0].ToString(), LogType.Warning);
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
        public void LogError(string message, params object[] pars)
        {
            bool parsNotNull = pars != null && pars.Length > 0;
            string str = parsNotNull ? message.TryFormat(pars) : message,
                   msg = !parsNotNull ? str.DetailedMessage("", LogType.Error) : str.DetailedMessage(pars[0].ToString(), LogType.Error);
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

        public static string DetailedMessage(this string message, string stacktrace, LogType t)
        {
            string name = Thread.CurrentThread.Name;
            return string.Format("[{0}] {1}/{2}: {3}{4}", DateTime.Now.ToString("hh:mm:ss"), string.IsNullOrEmpty(name) ? "Main" : name, t.ToString(), message, !string.IsNullOrEmpty(stacktrace) && t != LogType.Log ? string.Format("\n{0}\n", stacktrace) : "");
        }

        public static string TryFormat(this string message, params object[] pars)
        {
            return message.Contains("{0}") ? string.Format(message, pars) : message;
        }
    }
}