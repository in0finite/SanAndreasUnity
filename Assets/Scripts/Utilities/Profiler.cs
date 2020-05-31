using System;
using System.Diagnostics;

namespace SanAndreasUnity.Utilities
{
    public static class Profiler
    {
        private class ProfileFrame : IDisposable
        {
            private readonly string _name;
            private readonly Stopwatch _timer;

            public ProfileFrame(string name)
            {
                _name = name;
                _timer = new Stopwatch();
                _timer.Start();
            }

            public void Dispose()
            {
                _timer.Stop();

                UnityEngine.Debug.LogFormat("{0}: {1:F2} ms", _name, _timer.Elapsed.TotalMilliseconds);
            }
        }

        public static IDisposable Start(string name)
        {
            return new ProfileFrame(name);
        }
    }
}