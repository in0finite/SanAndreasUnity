using ModestTree;
using System;

namespace Zenject
{
    public class StaticMethodSignalHandler : SignalHandlerBase
    {
        private readonly Action _method;

        public StaticMethodSignalHandler(
            BindingId signalId, SignalManager manager, Action method)
            : base(signalId, manager)
        {
            _method = method;
        }

        public override void Execute(object[] args)
        {
            Assert.That(args.IsEmpty());

#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
            using (ProfileBlock.Start(_method.ToDebugString()))
#endif
            {
                _method();
            }
        }
    }

    public class StaticMethodSignalHandler<TParam1> : SignalHandlerBase
    {
        private readonly Action<TParam1> _method;

        public StaticMethodSignalHandler(
            BindingId signalId, SignalManager manager, Action<TParam1> method)
            : base(signalId, manager)
        {
            _method = method;
        }

        public override void Execute(object[] args)
        {
            Assert.That(args.IsLength(1));
            ValidateParameter<TParam1>(args[0]);
#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
            using (ProfileBlock.Start(_method.ToDebugString()))
#endif
            {
                _method((TParam1)args[0]);
            }
        }
    }

    public class StaticMethodSignalHandler<TParam1, TParam2> : SignalHandlerBase
    {
        private readonly Action<TParam1, TParam2> _method;

        public StaticMethodSignalHandler(
            BindingId signalId, SignalManager manager, Action<TParam1, TParam2> method)
            : base(signalId, manager)
        {
            _method = method;
        }

        public override void Execute(object[] args)
        {
            Assert.That(args.IsLength(2));
            ValidateParameter<TParam1>(args[0]);
            ValidateParameter<TParam2>(args[1]);
#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
            using (ProfileBlock.Start(_method.ToDebugString()))
#endif
            {
                _method((TParam1)args[0], (TParam2)args[1]);
            }
        }
    }

    public class StaticMethodSignalHandler<TParam1, TParam2, TParam3> : SignalHandlerBase
    {
        private readonly Action<TParam1, TParam2, TParam3> _method;

        public StaticMethodSignalHandler(
            BindingId signalId, SignalManager manager, Action<TParam1, TParam2, TParam3> method)
            : base(signalId, manager)
        {
            _method = method;
        }

        public override void Execute(object[] args)
        {
            Assert.That(args.IsLength(3));
            ValidateParameter<TParam1>(args[0]);
            ValidateParameter<TParam2>(args[1]);
            ValidateParameter<TParam3>(args[2]);
#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
            using (ProfileBlock.Start(_method.ToDebugString()))
#endif
            {
                _method((TParam1)args[0], (TParam2)args[1], (TParam3)args[2]);
            }
        }
    }

    public class StaticMethodSignalHandler<TParam1, TParam2, TParam3, TParam4> : SignalHandlerBase
    {
        private readonly Action<TParam1, TParam2, TParam3, TParam4> _method;

        public StaticMethodSignalHandler(
            BindingId signalId, SignalManager manager, Action<TParam1, TParam2, TParam3, TParam4> method)
            : base(signalId, manager)
        {
            _method = method;
        }

        public override void Execute(object[] args)
        {
            Assert.That(args.IsLength(4));
            ValidateParameter<TParam1>(args[0]);
            ValidateParameter<TParam2>(args[1]);
            ValidateParameter<TParam3>(args[2]);
            ValidateParameter<TParam4>(args[3]);
#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
            using (ProfileBlock.Start(_method.ToDebugString()))
#endif
            {
                _method((TParam1)args[0], (TParam2)args[1], (TParam3)args[2], (TParam4)args[3]);
            }
        }
    }
}