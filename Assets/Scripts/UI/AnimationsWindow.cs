using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Importing.Animation;
using SanAndreasUnity.Behaviours;
using System.Linq;

namespace SanAndreasUnity.UI {

	public class AnimationsWindow : PauseMenuWindow {

		private	Vector2	m_scrollViewPos = Vector2.zero;
		private	float m_lastContentHeight = 0;
		private bool m_displayWalkcycleAnims = false;
		private bool m_displayAnimStats = false;



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


			float headerHeight = m_displayAnimStats ? 200 : 60;

			GUILayout.BeginArea (new Rect (0, 0, this.windowRect.width, headerHeight));

			if (playerExists)
				Player.Instance.shouldPlayAnims = !GUILayout.Toggle( !Player.Instance.shouldPlayAnims, "Override player anims" );

			m_displayWalkcycleAnims = GUILayout.Toggle( m_displayWalkcycleAnims, "Display walkcycle anims");

			m_displayAnimStats = GUILayout.Toggle( m_displayAnimStats, "Display anim stats");

			// display anim stats
			if (m_displayAnimStats && playerExists) {
				DisplayAnimStats ();
			}

			GUILayout.EndArea ();


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
						string animName = animGroup.Animations[i];

						rect.position = new Vector2 (rect.position.x, rect.position.y + labelHeight);

						if (playerExists) {
							// display button which will play the anim
							if (GUI.Button (rect, animName)) {
								Player.Instance.PlayerModel.ResetModelState ();
								Player.Instance.PlayerModel.PlayAnim( animGroup.Type, AnimIndexUtil.Get(i) );
							}
						} else {
							GUI.Label (rect, animName);
						}
					}
				}
			}

			GUI.EndScrollView ();

			m_lastContentHeight = rect.yMax;

		}

		private void DisplayAnimStats ()
		{

			GUILayout.Space (5);

			var model = Player.Instance.PlayerModel;

			int numActiveClips = model.AnimComponent.OfType<AnimationState>().Where(a => a.enabled).Count();
			GUILayout.Label("Currently played clips [" + numActiveClips + "] :");

			// display all currently played clips

			foreach (AnimationState animState in model.AnimComponent) {

				if (!animState.enabled)
					continue;

			//	GUILayout.BeginHorizontal ();

				var clip = animState.clip;

				GUILayout.Label (string.Format ("name: {0}, length: {1}, frame rate: {2}, wrap mode: {3}, speed: {4}, time: {5}", 
					clip.name, animState.length, clip.frameRate, animState.wrapMode, animState.speed, animState.normalizedTime));

			//	GUILayout.EndHorizontal ();
			}

			GUILayout.Space (7);

			GUILayout.Label ("Root frame velocity: " + model.RootFrame.LocalVelocity);

		}

	}

}
