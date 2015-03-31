using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ProtoBuf;
using UnityEngine;

namespace ProtoBuf
{
    /// <summary>
    /// Interface for classes that describe sampled states of a
    /// networkable object for sending to peers.
    /// </summary>
    public interface ISnapshot
    {
        /// <summary>
        /// Estimated server time when this snapshot was sampled.
        /// </summary>
        long Timestamp { get; set; }
    }

    /// <summary>
    /// Hints for snapshot property interpolation.
    /// </summary>
    public enum InterpolationFlags
    {
        /// <summary>
        /// Use the default interpolation method.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Values are angles in degrees, using rotational distance as a metric.
        /// </summary>
        Angle = 1
    }

    /// <summary>
    /// Properties marked with this attribute in a class extending ISnapshot
    /// will be interpolated when used in a SnapshotBuffer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class InterpolateAttribute : Attribute
    {
        /// <summary>
        /// Hints for snapshot property interpolation.
        /// </summary>
        public InterpolationFlags Flags { get; set; }

        /// <summary>
        /// How gradually the value of this property changes.
        /// A value of 0 corresponds to immediate changes, and
        /// a value of 1 corresponds to changing by half of the
        /// difference between the current and destination values
        /// each second.
        /// </summary>
        public float Smoothing { get; set; }

        public InterpolateAttribute(InterpolationFlags flags = InterpolationFlags.Default)
        {
            Smoothing = 0f;
            Flags = InterpolationFlags.Default;
        }
    }
}

namespace Facepunch.Networking
{
    /// <summary>
    /// Queues added snapshots and attempts to play them back at a
    /// regular rate while maintaining a given buffered duration.
    /// 
    /// Also provides interpolation of properties within each snapshot.
    /// </summary>
    /// <typeparam name="TSnapshot">Type of snapshot to buffer.</typeparam>
    public class SnapshotBuffer<TSnapshot>
        where TSnapshot : class, ISnapshot, new()
    {
        #region Reflection

        private delegate void LerpDelegate(TSnapshot dest, TSnapshot a, TSnapshot b, float t, float dt);
        private delegate TVal LerpDelegate<TVal>(TVal a, TVal b, float t);

        private static LerpDelegate[] _sLerpActions;

        /// For convenience
        private static MethodInfo GetLerpMethod<TVal>(LerpDelegate<TVal> del)
        {
            return del.Method;
        }

        /// For convenience
        private static void AddLerpMethod<TVal>(IDictionary<Type, MethodInfo> dict, LerpDelegate<TVal> del)
        {
            dict.Add(typeof(TVal), GetLerpMethod(del));
        }

        private static double LerpDouble(double a, double b, float t)
        {
            return a * (1f - t) + b * t;
        }

        /// <summary>
        /// Compiles a sequence of delegates that set the properties
        /// of a destination snapshot to be an interpolation between
        /// the values of two other snapshots.
        /// </summary>
        private static void GenerateLerpDelegates()
        {
            var type = typeof(TSnapshot);

            // Delegate parameters

            var dest = Expression.Parameter(type, "dest");
            var a = Expression.Parameter(type, "a");
            var b = Expression.Parameter(type, "b");
            var t = Expression.Parameter(typeof(float), "t");
            var dt = Expression.Parameter(typeof(float), "dt");

            // Interpolation methods

            var lerpMethods = new Dictionary<Type, MethodInfo>();

            AddLerpMethod<float>(lerpMethods, Mathf.Lerp);
            AddLerpMethod<double>(lerpMethods, LerpDouble);
            AddLerpMethod<Vector2>(lerpMethods, Vector2.Lerp);
            AddLerpMethod<Vector3>(lerpMethods, Vector3.Lerp);
            AddLerpMethod<Vector4>(lerpMethods, Vector4.Lerp);
            AddLerpMethod<Quaternion>(lerpMethods, Quaternion.Lerp);

            var lerpAngle = GetLerpMethod<float>(Mathf.LerpAngle);

            // Smoothing snippets

            var pow = typeof(Mathf).GetMethod("Pow");
            var one = Expression.Constant(1f);
            var smoothExponent = Expression.Divide(one, dt);

            var deltaAngle = typeof(Mathf).GetMethod("DeltaAngle");

            // Delegate generation for each public property in TSnapshot

            var actions = new List<LerpDelegate>();

            foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                var get = prop.GetGetMethod(false);
                var set = prop.GetSetMethod(false);

                if (get == null || set == null) continue;

                var aGet = Expression.Call(a, get);
                var bGet = Expression.Call(a, get);

                Expression destVal;

                var attrib = prop.GetAttribute<InterpolateAttribute>(true);

                if (attrib == null) {
                    destVal = aGet;
                } else {
                    var smoothRate = 1f - .5f * Mathf.Pow(Mathf.Clamp01(attrib.Smoothing), 4f);
                    var isAngle = attrib.Flags == InterpolationFlags.Angle;

                    if (!lerpMethods.ContainsKey(prop.PropertyType)) continue;

                    var method = isAngle ? lerpAngle : lerpMethods[prop.PropertyType];

                    destVal = Expression.Call(method, aGet, bGet, t);

                    if (smoothRate < 1d) {
                        // Smoothing
                        
                        var smoothRateConst = Expression.Constant(smoothRate);
                        var smoothing = Expression.Call(pow, smoothRateConst, smoothExponent);

                        var destGet = Expression.Call(dest, get);
                        var diff = isAngle
                            ? (Expression) Expression.Call(deltaAngle, destGet, destVal)
                            : Expression.Subtract(destVal, destGet);
                        var incr = Expression.Multiply(diff, smoothing);

                        destVal = Expression.Add(destGet, incr);
                    }
                }

                var destSet = Expression.Call(dest, set, destVal);
                actions.Add(Expression.Lambda<LerpDelegate>(destSet, dest, a, b, t, dt).Compile());
            }

            _sLerpActions = actions.ToArray();
        }

        #endregion

        private readonly LinkedList<TSnapshot> _snapshots;

        private long _idealBufferedTime;
        private readonly long _timestampResolution;

        private long _lastUpdateTime;
        private long _playbackTime;

        private readonly TSnapshot _current;
        private TSnapshot _last;

        /// <summary>
        /// Current interpolated state.
        /// </summary>
        public TSnapshot Current { get { return _current; } }

        /// <summary>
        /// Time, in seconds, to attempt to buffer snapshots before playback.
        /// </summary>
        public double IdealBufferedTime
        {
            get { return _idealBufferedTime / (double) _timestampResolution; }
            set { _idealBufferedTime = (long) (_timestampResolution * value); }
        }

        /// <summary>
        /// Creates a new SnapshotBuffer that attempts to maintain
        /// the specified buffered duration.
        /// </summary>
        public SnapshotBuffer(double idealBufferedTime = 0.1d, long timestampResolution = 1000000)
        {
            _snapshots = new LinkedList<TSnapshot>();

            _timestampResolution = timestampResolution;
            IdealBufferedTime = idealBufferedTime;

            _current = new TSnapshot();

            Reset();
        }

        /// <summary>
        /// Adds a snapshot to the buffer if it isn't too late.
        /// </summary>
        public void Add(TSnapshot snapshot)
        {
            if (snapshot.Timestamp <= _last.Timestamp) return;

            var next = _snapshots.First;
            while (next != null) {
                if (next.Value.Timestamp == snapshot.Timestamp) return;
                if (next.Value.Timestamp > snapshot.Timestamp) {
                    _snapshots.AddBefore(next, snapshot);
                    return;
                }

                next = next.Next;
            }

            _snapshots.AddLast(snapshot);
        }

        /// <summary>
        /// Adds a collection of snapshots to the buffer.
        /// </summary>
        public void AddRange(IEnumerable<TSnapshot> snapshots)
        {
            foreach (var snapshot in snapshots) {
                Add(snapshot);
            }
        }

        /// <summary>
        /// Updates the properties of Current to contain
        /// values interpolated between the previous snapshot
        /// and the next one in the buffer.
        /// </summary>
        private void LerpCurrent(TSnapshot next, float t, float dt)
        {
            if (_sLerpActions == null) {
                GenerateLerpDelegates();
            }

            t = Mathf.Clamp01(t);

            foreach (var action in _sLerpActions) {
                action(_current, _last, next, t, dt);
            }
        }

        public void Reset()
        {
            _snapshots.Clear();

            _last = new TSnapshot();
            LerpCurrent(_last, 0f, float.PositiveInfinity);

            _playbackTime = -1;
        }

        /// <summary>
        /// Moves playback forward through the buffer, updating
        /// the properties in Current to contain values
        /// interpolated between two values in the buffer.
        /// </summary>
        /// <param name="time">
        /// Current server coordinated time in microseconds,
        /// usually accessed with `Networkable.ServerTime`.
        /// </param>
        public void Update()
        {
            var time = (Stopwatch.GetTimestamp() * _timestampResolution) / Stopwatch.Frequency;
            var dt = time - _lastUpdateTime;

            _lastUpdateTime = time;

            if (_playbackTime == -1) {
                if (_snapshots.Count == 0) return;

                _playbackTime = _snapshots.First.Value.Timestamp - _idealBufferedTime;

                _last = _snapshots.First.Value;
                LerpCurrent(_last, 0.0f, float.PositiveInfinity);

                return;
            }

            // Playback rate depends on the duration of buffered
            // snapshots, in an attempt to maintain a buffer
            // of duration `_idealBufferedTime`.

            if (_snapshots.Count == 0) return;

            var max = _snapshots.Count == 0 ? _playbackTime : _snapshots.Max(x => x.Timestamp);
            var rate = (max - _playbackTime) / (double) _idealBufferedTime;

            if (rate <= 0d) return;

            _playbackTime += (long) (dt * rate);

            var next = _snapshots.First.Value;

            var dtFloat = (float) ((double) dt / _timestampResolution);

            while (_playbackTime > next.Timestamp) {
                _last = next;
                _snapshots.RemoveFirst();

                if (_snapshots.Count == 0) {
                    LerpCurrent(_last, 0f, dtFloat);
                    return;
                }

                next = _snapshots.First.Value;
            }

            var t = (float) ((_playbackTime - _last.Timestamp)
                / (double) (next.Timestamp - _last.Timestamp));

            LerpCurrent(next, t, dtFloat);
        }
    }
}
