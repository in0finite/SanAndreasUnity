using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using ProtoBuf;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Facepunch.Networking
{
    public interface IEndPoint
    {
        double Time { get; }
    }

    public abstract class EndPoint<TComponent, TServer> : SingletonComponent<TComponent>, IEndPoint
        where TComponent : EndPoint<TComponent, TServer>
        where TServer : class, IServer
    {
        protected virtual bool ShouldActivate { get { return false; } }

        protected virtual int Protocol { get { return 0x0001; } }

        protected virtual String LogPrefix { get { throw new NotImplementedException(); } }

        protected virtual int UpdateRate { get { throw new NotImplementedException(); } }

        public TServer Net { get; private set; }

        public NetStatus NetStatus { get { return Net == null ? NetStatus.NotRunning : Net.NetStatus; } }

        private delegate void LogFormatDelegate(String format, params object[] args);

        private long _nextTick;

        /// <summary>
        /// Time since the server started, in microseconds.
        /// </summary>
        public abstract double Time { get; }

        public double SystemTime
        {
            get { return Stopwatch.GetTimestamp() / (double) Stopwatch.Frequency; }
        }

        private static UnityEngine.Application.LogCallback LogHandler(String prefix)
        {
            return (condition, stackTrace, type) => {
                LogFormatDelegate log;

                switch (type) {
                    case LogType.Log:
                        log = Debug.LogFormat;
                        break;
                    case LogType.Warning:
                        log = Debug.LogWarningFormat;
                        break;
                    case LogType.Error:
                        log = Debug.LogErrorFormat;
                        break;
                    default:
                        return;
                }

                log("[{0}] {1}", prefix, condition);
            };
        }

        protected virtual TServer CreateInstance() { throw new NotImplementedException(); }

        // ReSharper disable once UnusedMember.Local
        protected override void OnAwake()
        {
            if (!ShouldActivate) {
                gameObject.SetActive(false);
                return;
            }

            _nextTick = Stopwatch.GetTimestamp();

            Net = CreateInstance();
            Net.PopulateMessageTable(Assembly.GetExecutingAssembly());
            Net.LogMessage += LogHandler(LogPrefix);

            Net.RegisterHandler<INetworkableMessage>(OnReceiveMessage);

            OnNetworkingAwake();
        }

        protected virtual void OnNetworkingAwake() { }

        protected virtual void OnReceiveMessage(IRemote sender, INetworkableMessage message) { }

        // ReSharper disable once UnusedMember.Local
        private void Update()
        {
            if (Net == null) return;

            var now = Stopwatch.GetTimestamp();
            if (_nextTick > now) return;

            _nextTick += Stopwatch.Frequency / UpdateRate;

            Profiler.BeginSample(String.Format("{0}.Update", GetType().Name), this);
            Net.CheckForMessages();

            OnUpdate();

            Net.FlushSentMessages();
            Profiler.EndSample();
        }

        protected virtual void OnUpdate() { }

        public abstract Networkable GetNetworkable(uint id);

        public TNetworkable GetNetworkable<TNetworkable>(uint id)
            where TNetworkable : Networkable
        {
            return (TNetworkable) GetNetworkable(id);
        }

        public TNetworkable GetNetworkable<TNetworkable>(NetworkableInfo info)
            where TNetworkable : Networkable
        {
            return info == null ? null : GetNetworkable<TNetworkable>(info.Ident);
        }

        public abstract IEnumerable<Networkable> GetNetworkables();

        // ReSharper disable once UnusedMember.Local
        private void OnDestroy()
        {
            OnDestroyed();

            if (Net != null) {
                Net.Dispose();
            }
        }

        protected virtual void OnDestroyed() { }
    }
}
