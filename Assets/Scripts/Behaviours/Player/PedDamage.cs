using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours
{
	
	public partial class Player : MonoBehaviour {

		public float Health { get { return this.Damageable.Health; } set { this.Damageable.Health = value; } }
		[SerializeField] private float m_maxHealth = 100f;
		public float MaxHealth { get { return m_maxHealth; } set { m_maxHealth = value; } }



		public void DrawHealthBar ()
		{
			if (null == this.PlayerModel.Head)
				return;

			Vector3 pos = this.PlayerModel.Head.position;
			pos += this.transform.up * PedManager.Instance.healthBarVerticalOffset;

			Rect rect = GUIUtils.GetRectForBarAsBillboard (pos, PedManager.Instance.healthBarWorldWidth, 
				PedManager.Instance.healthBarWorldHeight, Camera.main);

			// limit height
			rect.height = Mathf.Min (rect.height, PedManager.Instance.healthBarMaxScreenHeight);

			float borderWidth = Mathf.Min( 2f, rect.height / 4f );
			GUIUtils.DrawBar( rect, this.Health / this.MaxHealth, UI.HUD.Instance.healthColor, UI.HUD.Instance.healthBackgroundColor, borderWidth );

		}


	}

}
