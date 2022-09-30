using Mirror;
using UnityEngine;
using UGameCore.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class RollState : BaseMovementState
	{
		public override AnimId movementAnim { get { return m_rollLeft ? new AnimId ("ped", "Crouch_Roll_L") : new AnimId ("ped", "Crouch_Roll_R"); } }
		public override AnimId movementWeaponAnim { get { return this.movementAnim; } }

		private bool m_rollLeft = false;
		private AnimationState m_animState;

		const string kRollDirSyncName = "rollDir";



		public bool CanRoll()
		{
			if( this.IsActiveState )
				return false;
			return true;
		}

		public bool Roll(bool left)
		{
			if( !this.CanRoll() )
				return false;

			m_rollLeft = left;
			m_ped.SwitchState<RollState>();

			return true;
		}

		protected override void Start()
		{
			base.Start();
			
			// subscribe to dictionary event
			if (m_isClientOnly)
				m_ped.syncDictionaryStringUint.Callback += this.OnDictChanged;

		}

		public override void OnBecameActive ()
		{
			base.OnBecameActive();
			
			this.PlayAnim();

			if (m_isServer)
				m_ped.syncDictionaryStringUint[kRollDirSyncName] = m_rollLeft ? (uint) 1 : (uint) 0;
		}

		public override byte[] GetAdditionalNetworkData()
		{
			var writer = new Mirror.NetworkWriter();
			writer.Write("roll");
			writer.Write(m_rollLeft);
			return writer.ToArray();
		}

		public override void OnSwitchedStateByServer(byte[] data)
		{
			var reader = new Mirror.NetworkReader(data);
			string magicWord = reader.ReadString();
			if (magicWord != "roll")
				Debug.LogErrorFormat("wrong magic word when switching to roll state: {0}", magicWord);
			m_rollLeft = reader.ReadBool();

			m_ped.SwitchState(this.GetType());
		}

		void OnDictChanged(SyncDictionary<string, uint>.Operation op, string key, uint value)
		{
			// switch (op)
			// {
			// 	case Ped.SyncDictionaryStringUint.Operation.OP_ADD:
			// 	case Ped.SyncDictionaryStringUint.Operation.OP_DIRTY:
			// 	case Ped.SyncDictionaryStringUint.Operation.OP_SET:
			// 		break;
			// }

			//Debug.LogFormat("OnDictChanged() - op: {0}, key: {1}, value: {2}", op, key, value);

			if (!this.IsActiveState)
				return;

			if (key != kRollDirSyncName)
				return;

			F.RunExceptionSafe( () => {
				if (m_ped.syncDictionaryStringUint.ContainsKey(key))
				{
					// roll direction possibly changed
					m_rollLeft = m_ped.syncDictionaryStringUint[key] == 1;
					//Debug.LogFormat("roll dir changed - old left: {0}, new left: {1}", oldLeft, m_rollLeft);
					this.PlayAnim();
					m_animState.time = 0f;
				}
			});

		}


		protected override void SwitchToMovementState ()
		{
			// don't switch to any state from here - we'll switch when anim finishes
		}

		protected override void SwitchToAimState ()
		{
			// don't switch to any state from here - we'll switch when anim finishes
		}


		protected override void UpdateHeading ()
		{
			if (m_isServer || m_ped.IsControlledByLocalPlayer)
				m_ped.Heading = m_ped.transform.forward;
		}

		protected override void UpdateRotation ()
		{
			// don't change rotation
		}

		protected override void UpdateMovement ()
		{
			// adjust movement input before updating movement
			// because only server performs moving, we'll do this only on server

			// also, we should preserve movement input - this is needed because, if we exit roll state,
			// we could quickly switch back to it, because we overrided client's movement input

			Vector3 originalMovementInput = m_ped.Movement;

			if (m_isServer)
				m_ped.Movement = m_rollLeft ? -m_ped.transform.right : m_ped.transform.right;
			
			base.UpdateMovement();

			m_ped.Movement = originalMovementInput;
		}

		void PlayAnim()
		{
			m_animState = m_model.PlayAnim( this.movementAnim );
			// clients have wrap mode set to 'Loop', because state can be switched very fast between roll and crouchaim, and
			// server will not update current state syncvar, so client will not start the state again,
			// and roll state will remain
			m_animState.wrapMode = WrapMode.Once;
			m_model.VelocityAxis = 0;	// movement will be done along x axis

		}

		protected override void UpdateAnims ()
		{
			// correct local x position of root frame
			Vector3 pos = m_model.RootFrame.transform.localPosition;
			pos.x = 0f;
			m_model.RootFrame.transform.localPosition = pos;
		}

		public override void LateUpdateState ()
		{
			base.LateUpdateState();

			if( !this.IsActiveState )
				return;

			if (m_isServer)
			{
				// check if anim is finished

				if( null == m_animState || !m_animState.enabled )
				{
					// anim finished
					// switch to other state

					// try to switch to crouch aim state
					CrouchState.SwitchToAimState(m_ped);
					if( !this.IsActiveState )
						return;

					// switch to crouch state
					m_ped.SwitchState<CrouchState>();
				}

			}

		}


		public override void OnSubmitPressed ()
		{
			// ignore
		}

		public override void OnJumpPressed ()
		{
			// ignore
		}

		public override void OnCrouchButtonPressed ()
		{
			// ignore
		}

		public override void OnFlyButtonPressed ()
		{
			// ignore
		}

		public override void OnFlyThroughButtonPressed ()
		{
			// ignore
		}

	}

}
