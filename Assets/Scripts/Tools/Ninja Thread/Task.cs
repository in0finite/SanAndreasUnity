using UnityEngine;

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CielaSpike
{
    /// <summary>
    /// Represents an async task.
    /// </summary>
    public class Task : IEnumerator
    {
        // implements IEnumerator to make it usable by StartCoroutine;
        #region IEnumerator Interface
        /// <summary>
        /// The current iterator yield return value.
        /// </summary>
        public object Current { get; private set; }

        /// <summary>
        /// Runs next iteration.
        /// </summary>
        /// <returns><code>true</code> for continue, otherwise <code>false</code>.</returns>
        public bool MoveNext()
        {
            return OnMoveNext();
        }

        public void Reset()
        {
            // Reset method not supported by iterator;
            throw new System.NotSupportedException(
                "Not support calling Reset() on iterator.");
        }
        #endregion

        // inner running state used by state machine;
        private enum RunningState
        {
            Init,
            RunningAsync,
            PendingYield,
            ToBackground,
            RunningSync,
            CancellationRequested,
            Done,
            Error
        }

        // routine user want to run;
        private readonly IEnumerator _innerRoutine;

        // current running state;
        private RunningState _state;
        // last running state;
        private RunningState _previousState;
        // temporary stores current yield return value
        // until we think Unity coroutine engine is OK to get it;
        private object _pendingCurrent;

        /// <summary>
        /// Gets state of the task.
        /// </summary>
        public TaskState State
        {
            get
            {
                switch (_state)
                {
                    case RunningState.CancellationRequested:
                        return TaskState.Cancelled;
                    case RunningState.Done:
                        return TaskState.Done;
                    case RunningState.Error:
                        return TaskState.Error;
                    case RunningState.Init:
                        return TaskState.Init;
                    default:
                        return TaskState.Running;
                }
            }
        }

        /// <summary>
        /// Gets exception during running.
        /// </summary>
        public System.Exception Exception { get; private set; }

        public Task(IEnumerator routine)
        {
            _innerRoutine = routine;
            // runs into background first;
            _state = RunningState.Init;
        }

        /// <summary>
        /// Cancel the task till next iteration;
        /// </summary>
        public void Cancel()
        {
            if (State == TaskState.Running)
            {
                GotoState(RunningState.CancellationRequested);
            }
        }

        /// <summary>
        /// A co-routine that waits the task.
        /// </summary>
        public IEnumerator Wait()
        {
            while (State == TaskState.Running)
                yield return null;
        }

        // thread safely switch running state;
        private void GotoState(RunningState state)
        {
            if (_state == state) return;

            lock (this)
            {
                // maintainance the previous state;
                _previousState = _state;
                _state = state;
            }
        }

        // thread safely save yield returned value;
        private void SetPendingCurrentObject(object current)
        {
            lock (this)
            {
                _pendingCurrent = current;
            }
        }

        // actual MoveNext method, controls running state;
        private bool OnMoveNext()
        {
            // no running for null;
            if (_innerRoutine == null)
                return false;

            // set current to null so that Unity not get same yield value twice;
            Current = null;

            // loops until the inner routine yield something to Unity;
            while (true)
            {
                // a simple state machine;
                switch (_state)
                {
                    // first, goto background;
                    case RunningState.Init:
                        GotoState(RunningState.ToBackground);
                        break;
                    // running in background, wait a frame;
                    case RunningState.RunningAsync:
                        return true;

                    // runs on main thread;
                    case RunningState.RunningSync:
                        MoveNextUnity();
                        break;

                    // need switch to background;
                    case RunningState.ToBackground:
                        GotoState(RunningState.RunningAsync);
                        // call the thread launcher;
                        MoveNextAsync();
                        return true;

                    // something was yield returned;
                    case RunningState.PendingYield:
                        if (_pendingCurrent == Ninja.JumpBack)
                        {
                            // do not break the loop, switch to background;
                            GotoState(RunningState.ToBackground);
                        }
                        else if (_pendingCurrent == Ninja.JumpToUnity)
                        {
                            // do not break the loop, switch to main thread;
                            GotoState(RunningState.RunningSync);
                        }
                        else
                        {
                            // not from the Ninja, then Unity should get noticed,
                            // Set to Current property to achieve this;
                            Current = _pendingCurrent;

                            // yield from background thread, or main thread?
                            if (_previousState == RunningState.RunningAsync)
                            {
                                // if from background thread, 
                                // go back into background in the next loop;
                                _pendingCurrent = Ninja.JumpBack;
                            }
                            else
                            {
                                // otherwise go back to main thread the next loop;
                                _pendingCurrent = Ninja.JumpToUnity;
                            }

                            // end this iteration and Unity get noticed;
                            return true;
                        }
                        break;

                    // done running, pass false to Unity;
                    case RunningState.Done:
                    case RunningState.CancellationRequested:
                    default:
                        return false;
                }
            }
        }

        // background thread launcher;
        private void MoveNextAsync()
        {
            ThreadPool.QueueUserWorkItem(
                new WaitCallback(BackgroundRunner));
        }

        // background thread function;
        private void BackgroundRunner(object state)
        {
            // just run the sync version on background thread;
            MoveNextUnity();
        }

        // run next iteration on main thread;
        private void MoveNextUnity()
        {
            try
            {
                // run next part of the user routine;
                var result = _innerRoutine.MoveNext();

                if (result)
                {
                    // something has been yield returned, handle it;
                    SetPendingCurrentObject(_innerRoutine.Current);
                    GotoState(RunningState.PendingYield);
                }
                else
                {
                    // user routine simple done;
                    GotoState(RunningState.Done);
                }
            }
            catch (System.Exception ex)
            {
                // exception handling, save & log it;
                this.Exception = ex;
                Debug.LogError(string.Format("{0}\n{1}", ex.Message, ex.StackTrace));
                // then terminates the task;
                GotoState(RunningState.Error);
            }
        }
    }
}