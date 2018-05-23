using System;

namespace Zenject
{
    public static class SignalExtensions
    {
        public static SignalBinderWithId DeclareSignal<T>(this DiContainer container)
            where T : ISignalBase
        {
            var info = new BindInfo(typeof(T));
            var signalSettings = new SignalSettings();
            container.Bind<T>(info).AsCached().WithArguments(signalSettings, info);
            return new SignalBinderWithId(info, signalSettings);
        }

        public static SignalBinderWithId DeclareSignal(this DiContainer container, Type type)
        {
            var info = new BindInfo(type);
            var signalSettings = new SignalSettings();
            container.Bind(type).AsCached().WithArguments(signalSettings, info);
            return new SignalBinderWithId(info, signalSettings);
        }

        public static SignalHandlerBinderWithId BindSignal<TSignal>(this DiContainer container)
            where TSignal : ISignal
        {
            var binder = container.StartBinding();
            return new SignalHandlerBinderWithId(
                container, typeof(TSignal), binder);
        }

        public static SignalHandlerBinderWithId<TParam1> BindSignal<TParam1, TSignal>(this DiContainer container)
            where TSignal : ISignal<TParam1>
        {
            var binder = container.StartBinding();
            return new SignalHandlerBinderWithId<TParam1>(
                container, typeof(TSignal), binder);
        }

        public static SignalHandlerBinderWithId<TParam1, TParam2> BindSignal<TParam1, TParam2, TSignal>(this DiContainer container)
            where TSignal : ISignal<TParam1, TParam2>
        {
            var binder = container.StartBinding();
            return new SignalHandlerBinderWithId<TParam1, TParam2>(
                container, typeof(TSignal), binder);
        }

        public static SignalHandlerBinderWithId<TParam1, TParam2, TParam3> BindSignal<TParam1, TParam2, TParam3, TSignal>(this DiContainer container)
            where TSignal : ISignal<TParam1, TParam2, TParam3>
        {
            var binder = container.StartBinding();
            return new SignalHandlerBinderWithId<TParam1, TParam2, TParam3>(
                container, typeof(TSignal), binder);
        }

        public static SignalHandlerBinderWithId<TParam1, TParam2, TParam3, TParam4> BindSignal<TParam1, TParam2, TParam3, TParam4, TSignal>(this DiContainer container)
            where TSignal : ISignal<TParam1, TParam2, TParam3, TParam4>
        {
            var binder = container.StartBinding();
            return new SignalHandlerBinderWithId<TParam1, TParam2, TParam3, TParam4>(
                container, typeof(TSignal), binder);
        }
    }
}