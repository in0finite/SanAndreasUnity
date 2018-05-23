using System;
using System.Diagnostics;

namespace ModestTree
{
    // Simple wrapper around unity's logging system
    public static class Log
    {
        // Strip out debug logs outside of unity
        [Conditional("UNITY_EDITOR")]
        public static void Debug(string message, params object[] args)
        {
#if NOT_UNITY3D
            //Console.WriteLine(string.Format(message, args));
#else
            //UnityEngine.Debug.Log(string.Format(message, args));
#endif
        }

        /////////////

        public static void Info(string message, params object[] args)
        {
#if NOT_UNITY3D
            Console.WriteLine(string.Format(message, args));
#else
            UnityEngine.Debug.Log(string.Format(message, args));
#endif
        }

        /////////////

        public static void Warn(string message, params object[] args)
        {
#if NOT_UNITY3D
            Console.WriteLine(string.Format(message, args));
#else
            UnityEngine.Debug.LogWarning(string.Format(message, args));
#endif
        }

        /////////////

        public static void Trace(string message, params object[] args)
        {
#if NOT_UNITY3D
            Console.WriteLine(string.Format(message, args));
#else
            UnityEngine.Debug.Log(string.Format(message, args));
#endif
        }

        /////////////

        public static void ErrorException(Exception e)
        {
#if NOT_UNITY3D
            Console.WriteLine(e.ToString());
#else
            UnityEngine.Debug.LogException(e);
#endif
        }

        public static void ErrorException(string message, Exception e)
        {
#if NOT_UNITY3D
            Console.WriteLine(message);
#else
            UnityEngine.Debug.LogError(message);
            UnityEngine.Debug.LogException(e);
#endif
        }

        public static void Error(string message, params object[] args)
        {
#if NOT_UNITY3D
            Console.WriteLine(string.Format(message, args));
#else
            UnityEngine.Debug.LogError(string.Format(message, args));
#endif
        }
    }
}