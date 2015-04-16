using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Facepunch.ConCommands;
using Facepunch.Networking;
using UnityEngine;
using ThreadState = System.Threading.ThreadState;

namespace Facepunch.Utilities
{
    public class PerformanceSampler : SingletonComponent<PerformanceSampler>
    {
        [ConCommand(Domain.Shared, "system", "status")]
        private static String NetStatusCommand(ConCommandArgs args)
        {
            var writer = new StringWriter();

            writer.WriteLine("Processor time: {0:F1}%", Instance.ProcessorTimePercent);
            writer.WriteLine("Total memory: {0:F1} KB", Instance.TotalMemory / 1024d);
            writer.WriteLine("Avg GC period: {0:F2} s", Instance.AverageGarbageCollectionPeriod);
            writer.WriteLine("Network status: {0}", Server.Instance.NetStatus);
            writer.WriteLine("Network thread status: {0}", Server.Instance.Net.NetThread.ThreadState);
            writer.WriteLine("Main thread state: {0}", Instance.LastUpdate.TotalSeconds < 2d * Instance.SamplePeriod
                ? Instance.MainThreadState.ToString() : string.Format("Hanging (since {0:F1} minutes ago)", Instance.LastUpdate.TotalMinutes));

            return writer.ToString();
        }

        private DateTime _lastUpdate;
        private TimeSpan _lastProcessorTime;
        private DateTime _lastGcPass;
        private int _lastGcPasses;

        private Thread _mainThread;

        private readonly Queue<double> _gcPeriods = new Queue<double>();

        private readonly Stopwatch _timer = new Stopwatch();

        public ThreadState MainThreadState
        {
            get { return _mainThread == null ? ThreadState.Unstarted : _mainThread.ThreadState; } 
        }

        public TimeSpan LastUpdate
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
            _lastGcPass = DateTime.UtcNow;
            Sample();
        }

        private void Sample()
        {
            if (_mainThread == null) _mainThread = Thread.CurrentThread;

            _lastUpdate = DateTime.UtcNow;

            var diff = _timer.Elapsed.TotalSeconds;

            var proc = Process.GetCurrentProcess();

            var processorTime = proc.TotalProcessorTime;
            var gcPasses = GC.CollectionCount(0);
            var gcPassDiff = gcPasses - _lastGcPasses;

            _lastGcPasses = gcPasses;

            if (gcPassDiff > 0) {
                var gcTimeDiff = _lastUpdate - _lastGcPass;

                if (gcPassDiff == 1) {
                    _gcPeriods.Enqueue(gcTimeDiff.TotalSeconds);
                } else {
                    var avg = diff / (gcPassDiff - 1) + (gcTimeDiff.TotalSeconds - diff);
                    for (var i = 0; i < gcPassDiff; ++i) {
                        _gcPeriods.Enqueue(avg);
                    }
                }

                _lastGcPass = _lastUpdate;

                while (_gcPeriods.Count > GarbageCollectionSamples) {
                    _gcPeriods.Dequeue();
                }
            }

            TotalMemory = GC.GetTotalMemory(false);

            var processorTimeDiff = processorTime.Subtract(_lastProcessorTime);

            ProcessorTimePercent = (float) ((processorTimeDiff.TotalSeconds * 100d / Environment.ProcessorCount) / diff);

            _lastProcessorTime = processorTime;

            _timer.Reset();
            _timer.Start();
        }

// ReSharper disable once UnusedMember.Local
        private void Update()
        {
            if (!(_timer.Elapsed.TotalSeconds > SamplePeriod)) return;

            Sample();
        }
    }
}
