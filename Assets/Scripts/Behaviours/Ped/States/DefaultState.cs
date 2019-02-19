using UnityEngine;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	/// <summary>
	/// Base class for all MonoBehaviour states.
	/// </summary>
	public class DefaultState : MonoBehaviour, IPedState
	{

		protected Ped m_ped;
	//	protected StateMachine m_stateMachine;
		protected new Transform transform { get { return m_ped.transform; } }
		public bool IsActiveState { get { return m_ped.CurrentState == this; } }



		protected virtual void Awake ()
		{
			m_ped = this.GetComponentInParent<Ped> ();
		}

		public virtual void OnBecameActive ()
		{
			
		}

		public virtual void OnBecameInactive ()
		{
			
		}

		public virtual bool RepresentsState (System.Type type)
		{
			return type.Equals (this.GetType ()) || type.IsSubclassOf (this.GetType ());
		}

		public virtual void UpdateState() {

			// read input

			// call appropriate function for every input action


			this.ConstrainPosition();
			this.ConstrainRotation();

		}

		public virtual void LateUpdateState()
		{

			if (m_ped.shouldPlayAnims)
				this.UpdateAnims ();

			this.RotateSpine ();

		}

		public virtual void FixedUpdateState()
		{

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

		protected virtual void UpdateAnims()
		{
			
		}

		protected virtual void RotateSpine()
		{

		}


		public virtual void OnSubmitPressed() {

		}

		public virtual void OnJumpPressed() {

		}

		public virtual void OnDamaged(DamageInfo info)
		{

		}

	}

}
