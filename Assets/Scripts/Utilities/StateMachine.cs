using UnityEngine;

namespace SanAndreasUnity.Utilities
{

	public class StateMachine {

		IState m_currentState;
		bool m_isSwitchingState = false;


		public void SwitchState(IState newState) {

			if(m_isSwitchingState)
				throw new System.Exception("Already switching state");

			m_isSwitchingState = true;

			IState oldState = m_currentState;

			m_currentState = newState;


			if(oldState != null)
				oldState.OnBecameInactive();

			m_isSwitchingState = false;

			if(m_currentState != null)
				m_currentState.OnBecameActive();

		}

	}

}
