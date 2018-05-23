using System;

namespace Zenject
{
    public abstract class SignalHandlerBinder
    {
        private readonly BindFinalizerWrapper _finalizerWrapper;
        private readonly Type _signalType;
        private readonly DiContainer _container;

        public SignalHandlerBinder(
            DiContainer container, Type signalType, BindFinalizerWrapper finalizerWrapper)
        {
            _container = container;
            _signalType = signalType;
            _finalizerWrapper = finalizerWrapper;
        }

        protected object Identifier
        {
            get;
            set;
        }

        public SignalFromBinder<THandler> To<THandler>(Action<THandler> method)
        {
            // This is just to ensure they don't stop at BindSignal
            _finalizerWrapper.SubFinalizer = new NullBindingFinalizer();

            var lookupId = Guid.NewGuid();

            _container.Bind(typeof(IInitializable), typeof(IDisposable)).To<StaticMethodWithInstanceSignalHandler<THandler>>().AsCached()
                .WithArguments(method, new InjectContext(_container, typeof(THandler), lookupId), new BindingId(_signalType, Identifier));

            var info = new BindInfo(typeof(THandler));

            return new SignalFromBinder<THandler>(
                info, _container.Bind<THandler>(info).WithId(lookupId).To<THandler>());
        }

        public SignalFromBinder<THandler> To<THandler>(Func<THandler, Action> methodGetter)
        {
            // This is just to ensure they don't stop at BindSignal
            _finalizerWrapper.SubFinalizer = new NullBindingFinalizer();

            var lookupId = Guid.NewGuid();

            _container.Bind(typeof(IInitializable), typeof(IDisposable)).To<InstanceMethodSignalHandler<THandler>>().AsCached()
                .WithArguments(methodGetter, new InjectContext(_container, typeof(THandler), lookupId), new BindingId(_signalType, Identifier));

            var info = new BindInfo(typeof(THandler));

            return new SignalFromBinder<THandler>(
                info, _container.Bind<THandler>(info).WithId(lookupId).To<THandler>());
        }

        public void To(Action method)
        {
            // This is just to ensure they don't stop at BindSignal
            _finalizerWrapper.SubFinalizer = new NullBindingFinalizer();

            _container.Bind(typeof(IInitializable), typeof(IDisposable)).To<StaticMethodSignalHandler>().AsCached()
                .WithArguments(method, new BindingId(_signalType, Identifier));
        }
    }

    public class SignalHandlerBinderWithId : SignalHandlerBinder
    {
        public SignalHandlerBinderWithId(
            DiContainer container, Type signalType, BindFinalizerWrapper finalizerWrapper)
            : base(container, signalType, finalizerWrapper)
        {
        }

        public SignalHandlerBinder WithId(object identifier)
        {
            Identifier = identifier;
            return this;
        }
    }

    public abstract class SignalHandlerBinder<TParam1>
    {
        private readonly BindFinalizerWrapper _finalizerWrapper;
        private readonly Type _signalType;
        private readonly DiContainer _container;

        public SignalHandlerBinder(
            DiContainer container, Type signalType, BindFinalizerWrapper finalizerWrapper)
        {
            _container = container;
            _signalType = signalType;
            _finalizerWrapper = finalizerWrapper;
        }

        protected object Identifier
        {
            get;
            set;
        }

        public SignalFromBinder<THandler> To<THandler>(Action<THandler, TParam1> method)
        {
            // This is just to ensure they don't stop at BindSignal
            _finalizerWrapper.SubFinalizer = new NullBindingFinalizer();

            var lookupId = Guid.NewGuid();

            _container.Bind(typeof(IInitializable), typeof(IDisposable)).To<StaticMethodWithInstanceSignalHandler<TParam1, THandler>>().AsCached()
                .WithArguments(method, new InjectContext(_container, typeof(THandler), lookupId), new BindingId(_signalType, Identifier));

            var info = new BindInfo(typeof(THandler));

            return new SignalFromBinder<THandler>(
                info, _container.Bind<THandler>(info).WithId(lookupId).To<THandler>());
        }

        public SignalFromBinder<THandler> To<THandler>(Func<THandler, Action<TParam1>> methodGetter)
        {
            // This is just to ensure they don't stop at BindSignal
            _finalizerWrapper.SubFinalizer = new NullBindingFinalizer();

            var lookupId = Guid.NewGuid();

            _container.Bind(typeof(IInitializable), typeof(IDisposable)).To<InstanceMethodSignalHandler<TParam1, THandler>>().AsCached()
                .WithArguments(methodGetter, new InjectContext(_container, typeof(THandler), lookupId), new BindingId(_signalType, Identifier));

            var info = new BindInfo(typeof(THandler));

            return new SignalFromBinder<THandler>(
                info, _container.Bind<THandler>(info).WithId(lookupId).To<THandler>());
        }

        public void To(Action<TParam1> method)
        {
            // This is just to ensure they don't stop at BindSignal
            _finalizerWrapper.SubFinalizer = new NullBindingFinalizer();

            _container.Bind(typeof(IInitializable), typeof(IDisposable)).To<StaticMethodSignalHandler<TParam1>>().AsCached()
                .WithArguments(method, new BindingId(_signalType, Identifier));
        }
    }

    public class SignalHandlerBinderWithId<TParam1> : SignalHandlerBinder<TParam1>
    {
        public SignalHandlerBinderWithId(
            DiContainer container, Type signalType, BindFinalizerWrapper finalizerWrapper)
            : base(container, signalType, finalizerWrapper)
        {
        }

        public SignalHandlerBinder<TParam1> WithId(object identifier)
        {
            Identifier = identifier;
            return this;
        }
    }

    public abstract class SignalHandlerBinder<TParam1, TParam2>
    {
        private readonly BindFinalizerWrapper _finalizerWrapper;
        private readonly Type _signalType;
        private readonly DiContainer _container;

        public SignalHandlerBinder(
            DiContainer container, Type signalType, BindFinalizerWrapper finalizerWrapper)
        {
            _container = container;
            _signalType = signalType;
            _finalizerWrapper = finalizerWrapper;
        }

        protected object Identifier
        {
            get;
            set;
        }

        public SignalFromBinder<THandler> To<THandler>(Action<THandler, TParam1, TParam2> method)
        {
            // This is just to ensure they don't stop at BindSignal
            _finalizerWrapper.SubFinalizer = new NullBindingFinalizer();

            var lookupId = Guid.NewGuid();

            _container.Bind(typeof(IInitializable), typeof(IDisposable)).To<StaticMethodWithInstanceSignalHandler<TParam1, TParam2, THandler>>().AsCached()
                .WithArguments(method, new InjectContext(_container, typeof(THandler), lookupId), new BindingId(_signalType, Identifier));

            var info = new BindInfo(typeof(THandler));

            return new SignalFromBinder<THandler>(
                info, _container.Bind<THandler>(info).WithId(lookupId).To<THandler>());
        }

        public SignalFromBinder<THandler> To<THandler>(Func<THandler, Action<TParam1, TParam2>> methodGetter)
        {
            // This is just to ensure they don't stop at BindSignal
            _finalizerWrapper.SubFinalizer = new NullBindingFinalizer();

            var lookupId = Guid.NewGuid();

            _container.Bind(typeof(IInitializable), typeof(IDisposable)).To<InstanceMethodSignalHandler<TParam1, TParam2, THandler>>().AsCached()
                .WithArguments(methodGetter, new InjectContext(_container, typeof(THandler), lookupId), new BindingId(_signalType, Identifier));

            var info = new BindInfo(typeof(THandler));

            return new SignalFromBinder<THandler>(
                info, _container.Bind<THandler>(info).WithId(lookupId).To<THandler>());
        }

        public void To(Action<TParam1, TParam2> method)
        {
            // This is just to ensure they don't stop at BindSignal
            _finalizerWrapper.SubFinalizer = new NullBindingFinalizer();

            _container.Bind(typeof(IInitializable), typeof(IDisposable)).To<StaticMethodSignalHandler<TParam1, TParam2>>().AsCached()
                .WithArguments(method, new BindingId(_signalType, Identifier));
        }
    }

    public class SignalHandlerBinderWithId<TParam1, TParam2> : SignalHandlerBinder<TParam1, TParam2>
    {
        public SignalHandlerBinderWithId(
            DiContainer container, Type signalType, BindFinalizerWrapper finalizerWrapper)
            : base(container, signalType, finalizerWrapper)
        {
        }

        public SignalHandlerBinderWithId<TParam1, TParam2> WithId(object identifier)
        {
            Identifier = identifier;
            return this;
        }
    }

    public abstract class SignalHandlerBinder<TParam1, TParam2, TParam3>
    {
        private readonly BindFinalizerWrapper _finalizerWrapper;
        private readonly Type _signalType;
        private readonly DiContainer _container;

        public SignalHandlerBinder(
            DiContainer container, Type signalType, BindFinalizerWrapper finalizerWrapper)
        {
            _container = container;
            _signalType = signalType;
            _finalizerWrapper = finalizerWrapper;
        }

        protected object Identifier
        {
            get;
            set;
        }

        public SignalFromBinder<THandler> To<THandler>(Action<THandler, TParam1, TParam2, TParam3> method)
        {
            // This is just to ensure they don't stop at BindSignal
            _finalizerWrapper.SubFinalizer = new NullBindingFinalizer();

            var lookupId = Guid.NewGuid();

            _container.Bind(typeof(IInitializable), typeof(IDisposable)).To<StaticMethodWithInstanceSignalHandler<TParam1, TParam2, TParam3, THandler>>().AsCached()
                .WithArguments(method, new InjectContext(_container, typeof(THandler), lookupId), new BindingId(_signalType, Identifier));

            var info = new BindInfo(typeof(THandler));

            return new SignalFromBinder<THandler>(
                info, _container.Bind<THandler>(info).WithId(lookupId).To<THandler>());
        }

        public SignalFromBinder<THandler> To<THandler>(Func<THandler, Action<TParam1, TParam2, TParam3>> methodGetter)
        {
            // This is just to ensure they don't stop at BindSignal
            _finalizerWrapper.SubFinalizer = new NullBindingFinalizer();

            var lookupId = Guid.NewGuid();

            _container.Bind(typeof(IInitializable), typeof(IDisposable)).To<InstanceMethodSignalHandler<TParam1, TParam2, TParam3, THandler>>().AsCached()
                .WithArguments(methodGetter, new InjectContext(_container, typeof(THandler), lookupId), new BindingId(_signalType, Identifier));

            var info = new BindInfo(typeof(THandler));

            return new SignalFromBinder<THandler>(
                info, _container.Bind<THandler>(info).WithId(lookupId).To<THandler>());
        }

        public void To(Action<TParam1, TParam2, TParam3> method)
        {
            // This is just to ensure they don't stop at BindSignal
            _finalizerWrapper.SubFinalizer = new NullBindingFinalizer();

            _container.Bind(typeof(IInitializable), typeof(IDisposable)).To<StaticMethodSignalHandler<TParam1, TParam2, TParam3>>().AsCached()
                .WithArguments(method, new BindingId(_signalType, Identifier));
        }
    }

    public class SignalHandlerBinderWithId<TParam1, TParam2, TParam3> : SignalHandlerBinder<TParam1, TParam2, TParam3>
    {
        public SignalHandlerBinderWithId(
            DiContainer container, Type signalType, BindFinalizerWrapper finalizerWrapper)
            : base(container, signalType, finalizerWrapper)
        {
        }

        public SignalHandlerBinderWithId<TParam1, TParam2, TParam3> WithId(object identifier)
        {
            Identifier = identifier;
            return this;
        }
    }

    public abstract class SignalHandlerBinder<TParam1, TParam2, TParam3, TParam4>
    {
        private readonly BindFinalizerWrapper _finalizerWrapper;
        private readonly Type _signalType;
        private readonly DiContainer _container;

        public SignalHandlerBinder(
            DiContainer container, Type signalType, BindFinalizerWrapper finalizerWrapper)
        {
            _container = container;
            _signalType = signalType;
            _finalizerWrapper = finalizerWrapper;
        }

        protected object Identifier
        {
            get;
            set;
        }

        public SignalFromBinder<THandler> To<THandler>(ModestTree.Util.Action<THandler, TParam1, TParam2, TParam3, TParam4> method)
        {
            // This is just to ensure they don't stop at BindSignal
            _finalizerWrapper.SubFinalizer = new NullBindingFinalizer();

            var lookupId = Guid.NewGuid();

            _container.Bind(typeof(IInitializable), typeof(IDisposable)).To<StaticMethodWithInstanceSignalHandler<TParam1, TParam2, TParam3, TParam4, THandler>>().AsCached()
                .WithArguments(method, new InjectContext(_container, typeof(THandler), lookupId), new BindingId(_signalType, Identifier));

            var info = new BindInfo(typeof(THandler));

            return new SignalFromBinder<THandler>(
                info, _container.Bind<THandler>(info).WithId(lookupId).To<THandler>());
        }

        public SignalFromBinder<THandler> To<THandler>(Func<THandler, Action<TParam1, TParam2, TParam3, TParam4>> methodGetter)
        {
            // This is just to ensure they don't stop at BindSignal
            _finalizerWrapper.SubFinalizer = new NullBindingFinalizer();

            var lookupId = Guid.NewGuid();

            _container.Bind(typeof(IInitializable), typeof(IDisposable)).To<InstanceMethodSignalHandler<TParam1, TParam2, TParam3, TParam4, THandler>>().AsCached()
                .WithArguments(methodGetter, new InjectContext(_container, typeof(THandler), lookupId), new BindingId(_signalType, Identifier));

            var info = new BindInfo(typeof(THandler));

            return new SignalFromBinder<THandler>(
                info, _container.Bind<THandler>(info).WithId(lookupId).To<THandler>());
        }

        public void To(Action<TParam1, TParam2, TParam3, TParam4> method)
        {
            // This is just to ensure they don't stop at BindSignal
            _finalizerWrapper.SubFinalizer = new NullBindingFinalizer();

            _container.Bind(typeof(IInitializable), typeof(IDisposable)).To<StaticMethodSignalHandler<TParam1, TParam2, TParam3, TParam4>>().AsCached()
                .WithArguments(method, new BindingId(_signalType, Identifier));
        }
    }

    public class SignalHandlerBinderWithId<TParam1, TParam2, TParam3, TParam4> : SignalHandlerBinder<TParam1, TParam2, TParam3, TParam4>
    {
        public SignalHandlerBinderWithId(
            DiContainer container, Type signalType, BindFinalizerWrapper finalizerWrapper)
            : base(container, signalType, finalizerWrapper)
        {
        }

        public SignalHandlerBinderWithId<TParam1, TParam2, TParam3, TParam4> WithId(object identifier)
        {
            Identifier = identifier;
            return this;
        }
    }
}