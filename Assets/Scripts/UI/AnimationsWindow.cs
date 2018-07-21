using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Importing.Animation;
using SanAndreasUnity.Behaviours;

namespace SanAndreasUnity.UI {

	public class AnimationsWindow : PauseMenuWindow {

		private	Vector2	m_scrollViewPos = Vector2.zero;
		private	float m_lastContentHeight = 0;
		private bool m_displayWalkcycleAnims = false;



		AnimationsWindow() {

			// set default parameters

			this.windowName = "Animations";
			this.useScrollView = false;

		}

		void Start () {
			
			this.RegisterButtonInPauseMenu ();

			// adjust rect
			this.windowRect = Utilities.GUIUtils.GetCenteredRect( new Vector2( 500, 400 ) );
		}


		protected override void OnWindowGUI ()
		{

//			if (null == Player.Instance) {
//				GUILayout.Label ("Player object not found");
//				return;
//			}


			bool playerExists = Player.Instance != null;


			float headerHeight = 40;

			if (playerExists)
				Player.Instance.shouldPlayAnims = !GUI.Toggle( new Rect( 0, 0, 150, 20 ), !Player.Instance.shouldPlayAnims, "Override player anims" );

			m_displayWalkcycleAnims = GUI.Toggle( new Rect(0, 20, 150, 20), m_displayWalkcycleAnims, "Display walkcycle anims");


			// display anim groups and their anims

			Rect scrollViewRect = this.windowRect;
			m_scrollViewPos = GUI.BeginScrollView (new Rect (new Vector2(0, headerHeight), scrollViewRect.size), m_scrollViewPos, 
				new Rect (Vector2.zero, new Vector2(scrollViewRect.width, m_lastContentHeight) ));

			float labelWidth = 150;
			float labelHeight = 20;
			Rect rect = new Rect (new Vector2(0, headerHeight), new Vector2(labelWidth, labelHeight));
		//	m_lastContentHeight = 0;


			foreach (var pair in Importing.Animation.AnimationGroup.AllLoadedGroups) {
				
			//	rect.xMin = 0;
			//	rect.yMin += labelHeight;
				rect.position = new Vector2( 0, rect.position.y + labelHeight );
				GUI.Label (rect, "Name: " + pair.Key);

				foreach (var pair2 in pair.Value) {

					if (!m_displayWalkcycleAnims && pair2.Key == AnimGroup.WalkCycle)
						continue;

				//	rect.xMin = labelWidth;
				//	rect.yMin += labelHeight;
					rect.position = new Vector2 (labelWidth, rect.position.y + labelHeight);
					GUI.Label (rect, "Type: " + pair2.Key);

					var animGroup = pair2.Value;

					rect.position = new Vector2 (labelWidth * 2, rect.position.y);
					for (int i=0; i < animGroup.Animations.Length; i++) {
						var anim = animGroup.Animations[i];

						rect.position = new Vector2 (rect.position.x, rect.position.y + labelHeight);

						if (playerExists) {
							// display button which will play the anim
							if (GUI.Button (rect, anim)) {
								Player.Instance.PlayerModel.PlayAnim( animGroup.Type, AnimIndexUtil.Get(i), PlayMode.StopAll );
							}
						} else {
							GUI.Label (rect, anim);
						}
					}
				}
			}

			GUI.EndScrollView ();

			m_lastContentHeight = rect.yMax;

		}

	}

}
