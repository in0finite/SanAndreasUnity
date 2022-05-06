using UnityEngine;

namespace SanAndreasUnity.Utilities
{
	
	public interface IState {
		
		void OnBecameActive();
		void OnBecameInactive();
		bool RepresentsState(System.Type type); // TODO: should be removed
		bool RepresentsState<T>() where T : IState; // TODO: should be removed
		void UpdateState();
		void LateUpdateState();
		void FixedUpdateState();

		object ParameterForEnteringState { set; }

		double LastTimeWhenActivated { get; set; }
		double LastTimeWhenDeactivated { get; set; }
	}

}
