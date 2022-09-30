using UnityEngine;
using UGameCore.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class RunState : BaseMovementState
	{
		public override AnimId movementAnim { get { return new AnimId (AnimGroup.WalkCycle, AnimIndex.Run); } }
		public override AnimId movementWeaponAnim { get { return m_ped.CurrentWeapon.RunAnim; } }


		protected override void UpdateAnims()
		{
			base.UpdateAnims();

			if (!this.IsActiveState)
				return;
			
			if (m_model.LastAnimId.Equals(new AnimId(AnimGroup.Gun, AnimIndex.run_armed)))
			{
				// set y position of unnamed and root frame to 0

				if (m_model.UnnamedFrame != null)
					m_model.UnnamedFrame.transform.localPosition = m_model.UnnamedFrame.transform.localPosition.WithXAndZ();
				
				if (m_model.RootFrame != null)
					m_model.RootFrame.transform.localPosition = m_model.RootFrame.transform.localPosition.WithXAndZ();
				
			}

		}

	}

}
