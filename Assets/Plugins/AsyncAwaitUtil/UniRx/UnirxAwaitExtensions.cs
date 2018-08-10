using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UniRx;

public static class UnirxAwaitExtensions
{
    public static TaskAwaiter<T> GetAwaiter<T>(this UniRx.IObservable<T> stream)
    {
        var tcs = new TaskCompletionSource<T>();

        IDisposable subscription = null;

        subscription = stream.Subscribe(
            x =>
            {
                subscription.Dispose();
                tcs.SetResult(x);
            },
            ex =>
            {
                subscription.Dispose();
                tcs.SetException(ex);
            });

        return tcs.Task.GetAwaiter();
    }
}
