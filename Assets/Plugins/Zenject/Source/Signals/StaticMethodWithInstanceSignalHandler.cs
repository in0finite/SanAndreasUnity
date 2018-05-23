using ModestTree;
using System;

namespace Zenject
{
    public class StaticMethodWithInstanceSignalHandler<THandler> : InstanceMethodSignalHandlerBase<THandler>
    {
        private readonly Action<THandler> _method;

        [Inject]
        public StaticMethodWithInstanceSignalHandler(
            BindingId signalId, SignalManager manager, InjectContext lookupContext,
            Action<THandler> method)
            : base(signalId, manager, lookupContext)
        {
            _method = method;
        }

        protected override void InternalExecute(THandler handler, object[] args)
        {
            Assert.That(args.IsEmpty());

#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
            using (ProfileBlock.Start(_method.ToDebugString()))
#endif
            {
                _method(handler);
            }
        }
    }

    public class StaticMethodWithInstanceSignalHandler<TParam1, THandler> : InstanceMethodSignalHandlerBase<THandler>
    {
        private readonly Action<THandler, TParam1> _method;

        [Inject]
        public StaticMethodWithInstanceSignalHandler(
            BindingId signalId, SignalManager manager, InjectContext lookupContext,
            Action<THandler, TParam1> method)
            : base(signalId, manager, lookupContext)
        {
            _method = method;
        }

        protected override void InternalExecute(THandler handler, object[] args)
        {
            Assert.That(args.IsLength(1));
            ValidateParameter<TParam1>(args[0]);

#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
            using (ProfileBlock.Start(_method.ToDebugString()))
#endif
            {
                _method(handler, (TParam1)args[0]);
            }
        }
    }

    public class StaticMethodWithInstanceSignalHandler<TParam1, TParam2, THandler> : InstanceMethodSignalHandlerBase<THandler>
    {
        private readonly Action<THandler, TParam1, TParam2> _method;

        [Inject]
        public StaticMethodWithInstanceSignalHandler(
            BindingId signalId, SignalManager manager, InjectContext lookupContext,
            Action<THandler, TParam1, TParam2> method)
            : base(signalId, manager, lookupContext)
        {
            _method = method;
        }

        protected override void InternalExecute(THandler handler, object[] args)
        {
            Assert.That(args.IsLength(2));
            ValidateParameter<TParam1>(args[0]);
            ValidateParameter<TParam2>(args[1]);

#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
            using (ProfileBlock.Start(_method.ToDebugString()))
#endif
            {
                _method(handler, (TParam1)args[0], (TParam2)args[1]);
            }
        }
    }

    public class StaticMethodWithInstanceSignalHandler<TParam1, TParam2, TParam3, THandler> : InstanceMethodSignalHandlerBase<THandler>
    {
        private readonly Action<THandler, TParam1, TParam2, TParam3> _method;

        [Inject]
        public StaticMethodWithInstanceSignalHandler(
            BindingId signalId, SignalManager manager, InjectContext lookupContext,
            Action<THandler, TParam1, TParam2, TParam3> method)
            : base(signalId, manager, lookupContext)
        {
            _method = method;
        }

        protected override void InternalExecute(THandler handler, object[] args)
        {
            Assert.That(args.IsLength(3));
            ValidateParameter<TParam1>(args[0]);
            ValidateParameter<TParam2>(args[1]);
            ValidateParameter<TParam3>(args[2]);

#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
            using (ProfileBlock.Start(_method.ToDebugString()))
#endif
            {
                _method(handler, (TParam1)args[0], (TParam2)args[1], (TParam3)args[2]);
            }
        }
    }

    public class StaticMethodWithInstanceSignalHandler<TParam1, TParam2, TParam3, TParam4, THandler> : InstanceMethodSignalHandlerBase<THandler>
    {
        private readonly ModestTree.Util.Action<THandler, TParam1, TParam2, TParam3, TParam4> _method;

        [Inject]
        public StaticMethodWithInstanceSignalHandler(
            BindingId signalId, SignalManager manager, InjectContext lookupContext,
            ModestTree.Util.Action<THandler, TParam1, TParam2, TParam3, TParam4> method)
            : base(signalId, manager, lookupContext)
        {
            _method = method;
        }

        protected override void InternalExecute(THandler handler, object[] args)
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
                _method(handler, (TParam1)args[0], (TParam2)args[1], (TParam3)args[2], (TParam4)args[3]);
            }
        }
    }
}