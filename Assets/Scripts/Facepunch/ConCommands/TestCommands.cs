using Facepunch.Networking;
using System;
using System.Linq;
using UnityEngine;
using Object = System.Object;

namespace Facepunch.ConCommands
{
    public static class TestCommands
    {
        [ConCommand(Domain.Shared, "echo", Description = "Responds with the passed arguments")]
        public static Object Echo(ConCommandArgs args)
        {
            return String.Format("\"{0}\"", String.Join("\" \"", args.Values));
        }

        [ConCommand(Domain.Shared, "count", Description = "Responds with the number of passed arguments")]
        public static Object ArgCount(ConCommandArgs args)
        {
            return args.Values.Length;
        }

        [ConCommand(Domain.Shared, "sum", Description = "Responds with the sum of all passed arguments")]
        public static Object Sum(ConCommandArgs args)
        {
            return Enumerable.Range(0, args.ValueCount)
                .Sum(x => args.CanGet<double>(x) ? args.Get<double>(x) : 0d);
        }

        [ConCommand(Domain.Shared, "log", Description = "Emits a message")]
        public static void Log(ConCommandArgs args)
        {
            Debug.Log(args);
        }

        [ConCommand(Domain.Shared, "log", "warning", Description = "Emits a warning")]
        public static void LogWarning(ConCommandArgs args)
        {
            Debug.LogWarning(args);
        }

        [ConCommand(Domain.Shared, "log", "error", Description = "Emits an error")]
        public static void LogError(ConCommandArgs args)
        {
            Debug.LogError(args);
        }
    }
}