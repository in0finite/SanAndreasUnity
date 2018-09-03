using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SanAndreasUnity.UI {

	public class OptionsWindow : PauseMenuWindow {

		/// <summary>
		/// Subscribe to this event to draw gui inside options window.
		/// </summary>
		public	static	event System.Action	onGUI = delegate {};


		public abstract class Input<T>
		{
			public string description = "";
		//	public T value;
		//	public T OldValue { get; internal set; }
		//	public bool IsChanged { get; internal set; }

			public System.Func<T> getValue = () => default(T);
			public System.Action<T> setValue = (val) => {};
			public System.Func<bool> isAvailable = () => true;


			public Input ()
			{
			}

			public Input (string description)
			{
				this.description = description;
			}

			public abstract T Display (T currentValue);

		}

		public class FloatInput : Input<float>
		{
			public float minValue;
			public float maxValue;

			public FloatInput (string description, float minValue, float maxValue) : base (description)
			{
				this.minValue = minValue;
				this.maxValue = maxValue;
			}

			public override float Display (float currentValue)
			{
				return OptionsWindow.FloatSlider (currentValue, this.minValue, this.maxValue, this.description);
			}
		}

		public class BoolInput : Input<bool>
		{
			public BoolInput (string description) : base (description)
			{
			}

			public override bool Display (bool currentValue)
			{
				return GUILayout.Toggle (currentValue, this.description);
			}
		}

		public class EnumInput<T> : Input<T> where T : struct
		{
			
			public override T Display (T currentValue)
			{
				return OptionsWindow.Enum (currentValue, this.description);
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

		public	static	T	Enum<T>( T currentValue, string description ) where T : struct {

			var values = System.Enum.GetValues (typeof(T));

			var newValue = MultipleOptions (currentValue, description, values.Cast<T> ().ToArray ());
			return newValue;
		}

		public	static	void	Input<T>( Input<T> input )
		{
			if (!input.isAvailable ())
				return;

			var oldValue = input.getValue ();

			var newValue = input.Display (oldValue);

			if (!newValue.Equals( oldValue ))
			{
				input.setValue (newValue);
			}

		}

	}

}
