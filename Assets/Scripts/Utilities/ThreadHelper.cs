using System;
using System.Threading;

namespace SanAndreasUnity.Utilities
{
    public static class ThreadHelper
    {
        public static void ThrowIfNotOnMainThread()
        {
            if (Thread.CurrentThread.ManagedThreadId != 1)
                throw new Exception("This can only be executed on the main thread");
        }
    }
}
