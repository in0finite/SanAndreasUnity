using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class WalkAimState : BaseAimState
	{

		public override void OnBecameActive ()
		{
			base.OnBecameActive ();
			m_ped.PlayerModel.PlayAnim (AnimGroup.MyWalkCycle, AnimIndex.WALK_armed);
		}

	}

}
