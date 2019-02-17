using UnityEngine;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class DefaultState : MonoBehaviour, IPedState
	{

		protected Ped m_ped;
		protected StateMachine m_stateMachine;
		public new Transform transform { get { return m_ped.transform; } }



		public override void UpdateState() {

			// read input

			// call appropriate function for every input action


			this.ConstrainPosition();
			this.ConstrainRotation();

			this.UpdateHeading();
			this.UpdateRotation();
			this.UpdateMovement();

		}

		protected virtual void ConstrainPosition()
		{
			m_ped.ConstrainPosition();
		}

		protected virtual void ConstrainRotation ()
		{
			m_ped.ConstrainRotation();
		}

		protected virtual void UpdateHeading()
		{
			m_ped.UpdateHeading ();
		}

		protected virtual void UpdateRotation()
		{
			m_ped.UpdateRotation ();
		}

		protected virtual void UpdateMovement()
		{
			m_ped.UpdateMovement ();
		}


		public virtual void OnSubmitPressed() {

		}

		public virtual void OnJumpPressed() {

		}

	}

}
