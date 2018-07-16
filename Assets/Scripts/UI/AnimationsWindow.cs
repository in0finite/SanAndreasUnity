using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.UI {

	public class AnimationsWindow : PauseMenuWindow {



		AnimationsWindow() {

			// set default parameters

			this.windowName = "Animations";
			this.useScrollView = true;

		}

		void Start () {

			this.RegisterButtonInPauseMenu ();

			// adjust rect
			this.windowRect = Utilities.GUIUtils.GetCenteredRect( new Vector2( 300, 400 ) );
		}


		protected override void OnWindowGUI ()
		{

//			if (null == Behaviours.Player.Instance) {
//				GUILayout.Label ("Player object not found");
//				return;
//			}


			// display all anim groups and their anims

			foreach (var pair in Importing.Animation.AnimationGroup.AllLoadedGroups) {
				GUILayout.Label ("Name: " + pair.Key);

				foreach (var pair2 in pair.Value) {
					GUILayout.Label ("\tType: " + pair.Key);

					var animGroup = pair2.Value;

					foreach (var anim in animGroup.Animations) {
						GUILayout.Label ("\t\t" + anim);
					}
				}
			}

		}

	}

}
