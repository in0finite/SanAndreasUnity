using UnityEngine;

namespace SanAndreasUnity.Utilities
{

	public class StateMachine {

		IState m_currentState;
		public IState CurrentState { get { return m_currentState; } }
		bool m_isSwitchingState = false;
		public float TimeWhenSwitchedState { get; private set; }
		public long FrameWhenSwitchedState { get; private set; }


		public void SwitchStateWithParameter(IState newState, object parameterForEnteringState) {

			if(m_isSwitchingState)
				throw new System.Exception("Already switching state");

			if (newState == m_currentState)
				return;

			m_isSwitchingState = true;

			IState oldState = m_currentState;

			m_currentState = newState;


			if (oldState != null)
			{
				// need to catch exception here, because otherwise it would freeze the state machine - it would
				// no longer be possible to switch states, because 'm_isSwitchingState' is true
				F.RunExceptionSafe(() =>
                {
					oldState.LastTimeWhenDeactivated = Time.time;
                    oldState.OnBecameInactive();
                });
			}

			m_isSwitchingState = false;

			this.TimeWhenSwitchedState = Time.time;
			this.FrameWhenSwitchedState = Time.frameCount;

			if (m_currentState != null)
			{
				m_currentState.ParameterForEnteringState = parameterForEnteringState;
				m_currentState.LastTimeWhenActivated = Time.time;
				m_currentState.OnBecameActive();
			}
		}

		public void SwitchState(IState newState)
		{
			this.SwitchStateWithParameter(newState, null);
		}

	}

}
