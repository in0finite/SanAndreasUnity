using ModestTree;
using System;

namespace Zenject
{
    public abstract class InstanceMethodSignalHandlerBase<THandler> : SignalHandlerBase
    {
        private readonly InjectContext _lookupContext;

        [Inject]
        public InstanceMethodSignalHandlerBase(
            BindingId signalId, SignalManager manager,
            InjectContext lookupContext)
            : base(signalId, manager)
        {
            Assert.IsEqual(lookupContext.MemberType, typeof(THandler));

            _lookupContext = lookupContext;
        }

        public override void Validate()
        {
            _lookupContext.Container.ResolveAll(_lookupContext);
        }

        public override void Execute(object[] args)
        {
            foreach (var match in _lookupContext.Container.ResolveAll(_lookupContext))
            {
                InternalExecute((THandler)match, args);
            }
        }

        protected abstract void InternalExecute(THandler handler, object[] args);
    }

    public class InstanceMethodSignalHandler<THandler> : InstanceMethodSignalHandlerBase<THandler>
    {
        private readonly Func<THandler, Action> _methodGetter;

        [Inject]
        public InstanceMethodSignalHandler(
            BindingId signalId, SignalManager manager, InjectContext lookupContext,
            Func<THandler, Action> methodGetter)
            : base(signalId, manager, lookupContext)
        {
            _methodGetter = methodGetter;
        }

        protected override void InternalExecute(THandler handler, object[] args)
        {
            Assert.That(args.IsEmpty());

            var method = _methodGetter(handler);
#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
            using (ProfileBlock.Start(method.ToDebugString()))
#endif
            {
                method();
            }
        }
    }

    public class InstanceMethodSignalHandler<TParam1, THandler> : InstanceMethodSignalHandlerBase<THandler>
    {
        private readonly Func<THandler, Action<TParam1>> _methodGetter;

        [Inject]
        public InstanceMethodSignalHandler(
            BindingId signalId, SignalManager manager, InjectContext lookupContext,
            Func<THandler, Action<TParam1>> methodGetter)
            : base(signalId, manager, lookupContext)
        {
            _methodGetter = methodGetter;
        }

        protected override void InternalExecute(THandler handler, object[] args)
        {
            Assert.That(args.IsLength(1));
            ValidateParameter<TParam1>(args[0]);

            var method = _methodGetter(handler);
#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
            using (ProfileBlock.Start(method.ToDebugString()))
#endif
            {
                method((TParam1)args[0]);
            }
        }
    }

    public class InstanceMethodSignalHandler<TParam1, TParam2, THandler> : InstanceMethodSignalHandlerBase<THandler>
    {
        private readonly Func<THandler, Action<TParam1, TParam2>> _methodGetter;

        [Inject]
        public InstanceMethodSignalHandler(
            BindingId signalId, SignalManager manager, InjectContext lookupContext,
            Func<THandler, Action<TParam1, TParam2>> methodGetter)
            : base(signalId, manager, lookupContext)
        {
            _methodGetter = methodGetter;
        }

        protected override void InternalExecute(THandler handler, object[] args)
        {
            Assert.That(args.IsLength(2));
            ValidateParameter<TParam1>(args[0]);
            ValidateParameter<TParam2>(args[1]);

            var method = _methodGetter(handler);
#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
            using (ProfileBlock.Start(method.ToDebugString()))
#endif
            {
                method((TParam1)args[0], (TParam2)args[1]);
            }
        }
    }

    public class InstanceMethodSignalHandler<TParam1, TParam2, TParam3, THandler> : InstanceMethodSignalHandlerBase<THandler>
    {
        private readonly Func<THandler, Action<TParam1, TParam2, TParam3>> _methodGetter;

        [Inject]
        public InstanceMethodSignalHandler(
            BindingId signalId, SignalManager manager, InjectContext lookupContext,
            Func<THandler, Action<TParam1, TParam2, TParam3>> methodGetter)
            : base(signalId, manager, lookupContext)
        {
            _methodGetter = methodGetter;
        }

        protected override void InternalExecute(THandler handler, object[] args)
        {
            Assert.That(args.IsLength(3));
            ValidateParameter<TParam1>(args[0]);
            ValidateParameter<TParam2>(args[1]);
            ValidateParameter<TParam3>(args[2]);

            var method = _methodGetter(handler);
#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
            using (ProfileBlock.Start(method.ToDebugString()))
#endif
            {
                method((TParam1)args[0], (TParam2)args[1], (TParam3)args[2]);
            }
        }
    }

    public class InstanceMethodSignalHandler<TParam1, TParam2, TParam3, TParam4, THandler> : InstanceMethodSignalHandlerBase<THandler>
    {
        private readonly Func<THandler, Action<TParam1, TParam2, TParam3, TParam4>> _methodGetter;

        [Inject]
        public InstanceMethodSignalHandler(
            BindingId signalId, SignalManager manager, InjectContext lookupContext,
            Func<THandler, Action<TParam1, TParam2, TParam3, TParam4>> methodGetter)
            : base(signalId, manager, lookupContext)
        {
            _methodGetter = methodGetter;
        }

        protected override void InternalExecute(THandler handler, object[] args)
        {
            Assert.That(args.IsLength(4));
            ValidateParameter<TParam1>(args[0]);
            ValidateParameter<TParam2>(args[1]);
            ValidateParameter<TParam3>(args[2]);
            ValidateParameter<TParam4>(args[3]);

            var method = _methodGetter(handler);
#if UNITY_EDITOR && ZEN_PROFILING_ENABLED
            using (ProfileBlock.Start(method.ToDebugString()))
#endif
            {
                method((TParam1)args[0], (TParam2)args[1], (TParam3)args[2], (TParam4)args[3]);
            }
        }
    }
}