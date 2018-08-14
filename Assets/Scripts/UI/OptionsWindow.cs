using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.UI {

	public class OptionsWindow : PauseMenuWindow {

		/// <summary>
		/// Subscribe to this event to draw gui inside options window.
		/// </summary>
		public	static	event System.Action	onGUI = delegate {};

		public class FloatInput
		{
			public string description = "";
			public float value;
			public float OldValue { get; internal set; }
			public float minValue;
			public float maxValue;
			public bool IsChanged { get; internal set; }

			public FloatInput (string description, float minValue, float maxValue)
			{
				this.description = description;
				this.minValue = minValue;
				this.maxValue = maxValue;
			}
		}



		OptionsWindow() {

			// set default parameters

			this.windowName = "Options";
			this.useScrollView = true;

		}

		void Start () {
			
			this.RegisterButtonInPauseMenu ();

			// adjust rect
			float windowWidth = Screen.width * 0.85f;
			windowWidth = Mathf.Min (windowWidth, 600);
			float windowHeight = Screen.height * 0.8f;
			windowHeight = Mathf.Min (windowHeight, windowWidth * 9 / 16);
			this.windowRect = Utilities.GUIUtils.GetCenteredRect (new Vector2 (windowWidth, windowHeight));

		}


		protected override void OnWindowGUI ()
		{

			GUILayout.Space (10);

			onGUI ();

			GUILayout.Space (20);

			// options to add: show FPS counter, show minimap, 

		}


		/// <summary>
		/// Displays float slider with description.
		/// </summary>
		public	static	void	FloatSlider(ref float value, float min, float max, string description) {

			GUILayout.Label(description + " : " + value);
			value = GUILayout.HorizontalSlider( value, min, max );

		}

		/// <summary>
		/// Displays float slider with description.
		/// </summary>
		public	static	float	FloatSlider(float value, float min, float max, string description) {

			GUILayout.Label(description + " : " + value);
			float newValue = GUILayout.HorizontalSlider( value, min, max );
			return newValue;
		}

		public	static	bool	FloatSlider(FloatInput floatInput)
		{
			floatInput.OldValue = floatInput.value;
			floatInput.value = FloatSlider (floatInput.value, floatInput.minValue, floatInput.maxValue, floatInput.description );
			floatInput.IsChanged = floatInput.value != floatInput.OldValue;
			return floatInput.IsChanged;
		}

		public	static	T	MultipleOptions<T>( T currentValue, string description, params T[] allValues ) {

			GUILayout.Label (description + " : " + currentValue.ToString ());

			GUILayout.BeginHorizontal ();

			T newValue = currentValue;

			foreach (var v in allValues) {
				if (GUILayout.Button (v.ToString ())) {
					newValue = v;
				}
				GUILayout.Space (5);
			}

			GUILayout.EndHorizontal ();

			return newValue;
		}

	}

}
