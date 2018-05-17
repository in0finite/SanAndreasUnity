using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

//using Facepunch.ConCommands;
//using Facepunch.Networking;
using UnityEngine;
using ThreadState = System.Threading.ThreadState;

namespace Facepunch.Utilities
{
    public interface IGateKeeper : IDisposable
    {
        object Owner { get; }
        String Name { get; }
        int Counter { get; }

        IDisposable Sample();
    }

    public class PerformanceSampler : SingletonComponent<PerformanceSampler>
    {
#pragma warning disable 0618

        /// <remarks>
        /// http://stackoverflow.com/questions/285031/how-to-get-non-current-threads-stacktrace
        /// </remarks>
        private static StackTrace GetStackTrace(Thread targetThread, out Exception error)
        {
            var suspend = targetThread.ThreadState != ThreadState.Suspended;
            error = null;

            StackTrace stackTrace = null;

            Exception ex = null;

            if (suspend)
            {
                var ready = new ManualResetEvent(false);

                new Thread(() =>
                {
                    // Backstop to release thread in case of deadlock:
                    ready.Set();
                    Thread.Sleep(200);
                    try
                    {
                        targetThread.Resume();
                    }
                    catch (Exception e)
                    {
                        ex = e;
                    }
                }).Start();

                ready.WaitOne();
                targetThread.Suspend();
            }

            try
            {
                stackTrace = new StackTrace(targetThread, true);
            }
            catch (Exception e)
            {
                error = e;
            }
            finally
            {
                if (suspend)
                {
                    try
                    {
                        targetThread.Resume();
                    }
                    catch (Exception e)
                    {
                        error = error ?? e;
                    }
                }
            }

            error = error ?? ex;

            return stackTrace;
        }

#pragma warning restore 0618

        /*     [ConCommand(Domain.Shared, "system", "status")]
             private static String SystemStatusCommand(ConCommandArgs args)
             {
                 var writer = new StringWriter();

                 writer.WriteLine("Uptime: {0:F1} minutes", Instance.Uptime.TotalMinutes);
                 writer.WriteLine("Processor time: {0:F1}%", Instance.ProcessorTimePercent);
                 writer.WriteLine("Total memory: {0:F1} KB", Instance.TotalMemory / 1024d);
                 writer.WriteLine("Avg GC period: {0:F2} s", Instance.AverageGarbageCollectionPeriod);
                 writer.WriteLine("Network status: {0}", Server.Instance.NetStatus);
                 writer.WriteLine("Network thread status: {0}", Server.Instance.Net.NetThread.ThreadState);
                 writer.WriteLine("Main thread state: {0}", Instance.MainThreadState);

                 if (!(Instance.SinceLastUpdate.TotalSeconds > 2d * Instance.SamplePeriod)) return writer.ToString();

                 writer.WriteLine("Main thread has been hanging for {0:F1} minutes!", Instance.SinceLastUpdate.TotalMinutes);

                 return writer.ToString();
             }

             [ConCommand(Domain.Shared, "system", "gate-keepers")]
             private static String SystemGateKeepersCommand(ConCommandArgs args)
             {
                 var writer = new StringWriter();

                 foreach (var gateKeeper in GateKeepers) {
                     writer.WriteLine("Owner: {0}, Name: {1}, Count: {2}", gateKeeper.Owner, gateKeeper.Name, gateKeeper.Counter);
                 }

                 return writer.ToString();
             }
         */

        private class GateKeeper : IGateKeeper
        {
            private class Sampler : IDisposable
            {
                private readonly GateKeeper _keeper;

                public Sampler(GateKeeper keeper)
                {
                    _keeper = keeper;
                }

                public void Dispose()
                {
                    --_keeper.Counter;
                }
            }

            private readonly Sampler _sampler;

            public object Owner { get; private set; }
            public string Name { get; private set; }
            public int Counter { get; private set; }

            public GateKeeper(object owner, String name)
            {
                _sampler = new Sampler(this);

                Owner = owner;
                Name = name;
                Counter = 0;
            }

            public IDisposable Sample()
            {
                ++Counter;
                return _sampler;
            }

            public void Dispose()
            {
                _sGateKeepers.Remove(this);
            }
        }

        private static readonly List<GateKeeper> _sGateKeepers = new List<GateKeeper>();

        public static IEnumerable<IGateKeeper> GateKeepers { get { return _sGateKeepers.Cast<IGateKeeper>(); } }

        public static IGateKeeper CreateGatekeeper(object owner, String name)
        {
            var keeper = new GateKeeper(owner, name);
            _sGateKeepers.Add(keeper);
            return keeper;
        }

        private readonly DateTime _startTime;

        private DateTime _lastUpdate;
        private TimeSpan _lastProcessorTime;
        private DateTime _lastGcPass;
        private int _lastGcPasses;

        private Thread _mainThread;
        private Thread _watcherThread;

        private bool _stopWatching;

        private readonly AutoResetEvent _sampleWait;
        private readonly ManualResetEvent _stopWait;

        private readonly Queue<double> _gcPeriods = new Queue<double>();

        private readonly Stopwatch _timer = new Stopwatch();

        public TimeSpan Uptime
        {
            get { return DateTime.UtcNow - _startTime; }
        }

        public ThreadState MainThreadState
        {
            get { return _mainThread == null ? ThreadState.Unstarted : _mainThread.ThreadState; }
        }

        public TimeSpan SinceLastUpdate
        {
            get { return DateTime.UtcNow - _lastUpdate; }
        }

        public double ProcessorTimePercent { get; private set; }

        public double AverageGarbageCollectionPeriod
        {
            get
            {
                return _gcPeriods.Count == 0
                    ? float.PositiveInfinity
                    : (_gcPeriods.Sum() + (DateTime.UtcNow - _lastGcPass).TotalSeconds)
                    / (_gcPeriods.Count + 1);
            }
        }

        public long TotalMemory { get; private set; }

        [Range(1f, 60f)]
        public float SamplePeriod = 2f;

        [Range(1, 200)]
        public int GarbageCollectionSamples = 10;

        public PerformanceSampler()
        {
            _startTime = _lastGcPass = DateTime.UtcNow;

            _sampleWait = new AutoResetEvent(false);
            _stopWait = new ManualResetEvent(true);

            Sample(false);
        }

        private static void SampleFailed()
        {
            UnityEngine.Debug.LogErrorFormat("!!! PerformanceSampler has failed to sample (time: {0}) !!!", DateTime.UtcNow);
            //    UnityEngine.Debug.LogFormat("HangNotifyUrl: {0}", NetConfig.HangNotifyUrl);

            /*
            if (String.IsNullOrEmpty(NetConfig.HangNotifyUrl)) return;

            using (var client = new WebClient()) {
                client.UploadString(NetConfig.HangNotifyUrl, "POST", new JObject {
                    {"status", "hanging"},
                    {"time", DateTime.UtcNow.ToBinary()},
                    {"last_state", SystemStatusCommand(null)},
                    {"port", NetConfig.Port}
                }.ToString());
            }
            */
        }

        private void WatcherEntry()
        {
            _stopWait.Reset();

            while (!_stopWatching)
            {
                if (_sampleWait.WaitOne(TimeSpan.FromSeconds(SamplePeriod * 2d + 10d))) continue;
                if (_stopWatching) return;
                SampleFailed();
                break;
            }

            _stopWait.Set();
        }

        private void Sample(bool watch)
        {
            if (watch && _watcherThread == null)
            {
                _watcherThread = new Thread(WatcherEntry);
                _watcherThread.Start();
            }

            if (_mainThread == null) _mainThread = Thread.CurrentThread;

            _lastUpdate = DateTime.UtcNow;
            _sampleWait.Set();

            var diff = _timer.Elapsed.TotalSeconds;

            var proc = Process.GetCurrentProcess();

            var processorTime = proc.TotalProcessorTime;
            var gcPasses = GC.CollectionCount(0);
            var gcPassDiff = gcPasses - _lastGcPasses;

            _lastGcPasses = gcPasses;

            if (gcPassDiff > 0)
            {
                var gcTimeDiff = _lastUpdate - _lastGcPass;

                if (gcPassDiff == 1)
                {
                    _gcPeriods.Enqueue(gcTimeDiff.TotalSeconds);
                }
                else
                {
                    var avg = diff / (gcPassDiff - 1) + (gcTimeDiff.TotalSeconds - diff);
                    for (var i = 0; i < gcPassDiff; ++i)
                    {
                        _gcPeriods.Enqueue(avg);
                    }
                }

                _lastGcPass = _lastUpdate;

                while (_gcPeriods.Count > GarbageCollectionSamples)
                {
                    _gcPeriods.Dequeue();
                }
            }

            TotalMemory = GC.GetTotalMemory(false);

            var processorTimeDiff = processorTime.Subtract(_lastProcessorTime);

            ProcessorTimePercent = (float)((processorTimeDiff.TotalSeconds * 100d / Environment.ProcessorCount) / diff);

            _lastProcessorTime = processorTime;

            _timer.Reset();
            _timer.Start();
        }

        public StackTrace GetMainThreadStackTrace(out Exception ex)
        {
            if (_mainThread.ThreadState == ThreadState.Stopped)
            {
                ex = new Exception("Thread stopped");
                return null;
            }

            return GetStackTrace(_mainThread, out ex);
        }

        // ReSharper disable once UnusedMember.Local
        private void Update()
        {
            if (!(_timer.Elapsed.TotalSeconds > SamplePeriod)) return;

            Sample(true);
        }

        private void OnDestroy()
        {
            UnityEngine.Debug.Log("Destroying PerformanceSampler");

            _stopWatching = true;
            _sampleWait.Set();

            if (_watcherThread != null && !_stopWait.WaitOne(1000))
            {
                UnityEngine.Debug.LogWarning("Timeout while stopping watcher thread!");
                _watcherThread.Abort();
            }

            _watcherThread = null;
        }
    }
}