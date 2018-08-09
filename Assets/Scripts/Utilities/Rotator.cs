using UnityEngine;

namespace SanAndreasUnity.Utilities
{
	
	public class Rotator : MonoBehaviour {

		public Vector3 angles = Vector3.zero;
		public bool changeEulers = false;


		void Update () {

			Vector3 delta = this.angles * Time.deltaTime;
			if (delta.sqrMagnitude < float.Epsilon)
				return;

			if (this.changeEulers) {
				Vector3 eulers = this.transform.localEulerAngles;
				eulers += delta;
				this.transform.localEulerAngles = eulers;
			} else {

				this.transform.rotation *= 
					Quaternion.AngleAxis (delta.x, Vector3.right)
					* Quaternion.AngleAxis (delta.y, Vector3.up)
					* Quaternion.AngleAxis (delta.z, Vector3.forward);

			}

		}

	}

}
