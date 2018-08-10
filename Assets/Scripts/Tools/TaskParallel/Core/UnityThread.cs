using System;
using System.Threading;

#if NETFX_CORE
using System.Threading.Tasks;
#endif

namespace CI.TaskParallel.Core
{
    public class UnityThread
    {
#if NETFX_CORE
        private Task _thread;
        private CancellationTokenSource _cancellationTokenSource;
#else
        private Thread _thread;
#endif

        public UnityThread(Action action)
        {
#if NETFX_CORE
            _cancellationTokenSource = new CancellationTokenSource();
            _thread = new Task(action, _cancellationTokenSource.Token);
#else
            _thread = new Thread(new ThreadStart(action));
#endif
        }

        public void Start()
        {
            _thread.Start();
        }

        public void Abort()
        {
#if NETFX_CORE
            _cancellationTokenSource.Cancel();
#else
            _thread.Abort();
#endif
        }

        public void Join()
        {
#if NETFX_CORE
            _thread.Wait();
#else
            _thread.Join();
#endif
        } 
    }
}