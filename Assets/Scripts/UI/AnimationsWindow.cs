using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.UI {

	public class AnimationsWindow : PauseMenuWindow {

		private	Vector2	m_scrollViewPos = Vector2.zero;
		private	float m_lastContentHeight = 0;



		AnimationsWindow() {

			// set default parameters

			this.windowName = "Animations";
			this.useScrollView = false;

		}

		void Start () {

			if (null == Behaviours.World.Cell.Instance) {
				// world is not loaded
				// we will use this window
				this.RegisterButtonInPauseMenu ();
			}

			// adjust rect
			this.windowRect = Utilities.GUIUtils.GetCenteredRect( new Vector2( 500, 400 ) );
		}


		protected override void OnWindowGUI ()
		{

//			if (null == Behaviours.Player.Instance) {
//				GUILayout.Label ("Player object not found");
//				return;
//			}


			// display all anim groups and their anims

			Rect scrollViewRect = this.windowRect;
			m_scrollViewPos = GUI.BeginScrollView (new Rect (Vector2.zero, scrollViewRect.size), m_scrollViewPos, 
				new Rect (Vector2.zero, new Vector2(scrollViewRect.width, m_lastContentHeight) ), false, false);

			float labelWidth = 150;
			float labelHeight = 20;
			Rect rect = new Rect (Vector2.zero, new Vector2(labelWidth, labelHeight));
		//	m_lastContentHeight = 0;

			bool playerExists = Behaviours.Player.Instance != null;


			foreach (var pair in Importing.Animation.AnimationGroup.AllLoadedGroups) {
				
			//	rect.xMin = 0;
			//	rect.yMin += labelHeight;
				rect.position = new Vector2( 0, rect.position.y + labelHeight );
				GUI.Label (rect, "Name: " + pair.Key);

				foreach (var pair2 in pair.Value) {

				//	rect.xMin = labelWidth;
				//	rect.yMin += labelHeight;
					rect.position = new Vector2 (labelWidth, rect.position.y + labelHeight);
					GUI.Label (rect, "Type: " + pair.Key);

					var animGroup = pair2.Value;

					rect.position = new Vector2 (labelWidth * 2, rect.position.y);
					for (int i=0; i < animGroup.Animations.Length; i++) {
						var anim = animGroup.Animations[i];

						rect.position = new Vector2 (rect.position.x, rect.position.y + labelHeight);

						if (playerExists) {
							// display button which will play the anim
							if (GUI.Button (rect, anim)) {
								Behaviours.Player.Instance.PlayerModel.PlayAnim( animGroup.Type, AnimIndexUtil.Get(i), PlayMode.StopAll );
							}
						} else {
							GUI.Label (rect, anim);
						}
					}
				}
			}

			GUI.EndScrollView ();

			m_lastContentHeight = rect.position.y;

		}

	}

}
