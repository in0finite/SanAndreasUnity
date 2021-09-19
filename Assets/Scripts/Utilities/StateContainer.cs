using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SanAndreasUnity.Utilities
{
    /// <summary>
    /// Container for <see cref="IState"/> objects.
    /// </summary>
    public class StateContainer<TState>
        where TState : IState
    {
        private readonly List<TState> _states = new List<TState>();
        public IReadOnlyList<TState> States => _states;


        public TState GetState(System.Type type)
        {
            return this._states.FirstOrDefault (s => s.GetType ().Equals (type));
        }

        public T GetState<T>()
            where T : TState
        {
            return (T) this.GetState(typeof(T));
        }

        public TState GetStateOrLogError(System.Type type)
        {
            var state = this.GetState (type);
            if(null == state)
                Debug.LogErrorFormat ("Failed to find state: {0}", type);
            return state;
        }

        public T GetStateOrLogError<T>()
            where T : TState
        {
            return (T) this.GetStateOrLogError(typeof(T));
        }

        public void AddState(TState stateToAdd)
        {
            _states.Add(stateToAdd);
        }

        public void AddStates(IEnumerable<TState> statesToAdd)
        {
            _states.AddRange(statesToAdd);
        }
    }
}
