using UnityEngine;

namespace SanAndreasUnity.Utilities
{
	
	public interface IState {
		
		void OnBecameActive();
		void OnBecameInactive();
		bool RepresentsState(System.Type type);
		void UpdateState();
		void LateUpdateState();
		void FixedUpdateState();

	}

}
