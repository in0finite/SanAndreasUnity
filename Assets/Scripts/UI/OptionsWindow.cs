using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.UI {

	public class OptionsWindow : PauseMenuWindow {

		/// <summary>
		/// Subscribe to this event to draw gui inside options window.
		/// </summary>
		public	static	event System.Action	onGUI = delegate {};


		public enum InputPersistType
		{
			None,
			OnStart,
			AfterLoaderFinishes
		}

		public abstract class Input
		{
			public string description = "";
			public InputPersistType persistType = InputPersistType.None;
			public string category = "";

			public abstract void Load ();
			public abstract void Save ();
			public abstract void Display ();
		}

		public abstract class Input<T> : Input
		{
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

			public override void Display ()
			{
				if (!this.isAvailable ())
					return;

				var oldValue = this.getValue ();

				var newValue = this.Display (oldValue);

				if (!newValue.Equals( oldValue ))
				{
					this.setValue (newValue);
				}
			}

			public abstract T Display (T currentValue);

			public override void Load () {
				if (!this.isAvailable ())
					return;
				if (!PlayerPrefs.HasKey (this.description))
					return;
				string str = PlayerPrefs.GetString (this.description, null);
				if (str != null)
				{
					this.setValue (this.Load (str));
				}
			}
			public abstract T Load (string str);

			public override void Save () {
				if (!this.isAvailable ())
					return;
				string str = this.SaveAsString (this.getValue ());
				if (str != null)
					PlayerPrefs.SetString (this.description, str);
			}

			public virtual string SaveAsString (T value) {
				return value.ToString ();
			}

		}

		public class FloatInput : Input<float>
		{
			public float minValue;
			public float maxValue;

			public FloatInput () { }

			public FloatInput (string description, float minValue, float maxValue) : base (description)
			{
				this.minValue = minValue;
				this.maxValue = maxValue;
			}

			public override float Display (float currentValue)
			{
				return OptionsWindow.FloatSlider (currentValue, this.minValue, this.maxValue, this.description);
			}

			public override float Load (string str)
			{
				return float.Parse (str, System.Globalization.CultureInfo.InvariantCulture);
			}
		}

		public class BoolInput : Input<bool>
		{
			public BoolInput () { }

			public BoolInput (string description) : base (description)
			{
			}

			public override bool Display (bool currentValue)
			{
				return GUILayout.Toggle (currentValue, this.description);
			}

			public override bool Load (string str)
			{
				return bool.Parse (str);
			}
		}

		public class EnumInput<T> : Input<T> where T : struct
		{
			
			public override T Display (T currentValue)
			{
				return OptionsWindow.Enum (currentValue, this.description);
			}

			public override T Load (string str)
			{
				return (T) System.Enum.Parse (typeof(T), str);
			}
		}

		public class MultipleOptionsInput<T> : Input<T>
		{
			public T[] Options { get; set; }

			public override T Display (T currentValue)
			{
				return OptionsWindow.MultipleOptions (currentValue, this.description, this.Options);
			}

			public override T Load (string str)
			{
				int index = this.Options.FindIndex (t => this.SaveAsString (t) == str);
				if (index < 0)
					throw new System.ArgumentException (
						string.Format ("Error loading multiple options of type {0} - specified option '{1}' was not found", 
							typeof(T), str));

				return this.Options [index];
			}
		}


		private static List<OptionsWindow.Input> s_registeredInputs = new List<OptionsWindow.Input> ();
		private static int s_currentTabIndex = 0;
		private static string[] s_categories;



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

			LoadSettings (InputPersistType.OnStart);

		}

		void OnLoaderFinished ()
		{
			LoadSettings (InputPersistType.AfterLoaderFinishes);
		}


		protected override void OnWindowGUIBeforeContent ()
		{
			s_categories = s_registeredInputs.Select (i => i.category).Distinct ().ToArray ();

			if (s_categories.Length > 0)
			{
				s_currentTabIndex = GUIUtils.TabsControl (s_currentTabIndex, s_categories);

				GUILayout.Space (20);
			}
		}

		protected override void OnWindowGUI ()
		{
			
			// draw inputs

			if (s_categories.Length > 0)
			{
				foreach (var input in s_registeredInputs.Where( i => i.category == s_categories[s_currentTabIndex] ))
				{
					input.Display ();
				}
			}

			/*
			var groupings = s_registeredInputs.GroupBy (i => i.category);
			foreach (var grouping in groupings)
			{
				GUILayout.Label ("\n" + grouping.Key + "\n");

				foreach (var input in grouping)
				{
					input.Display ();
				}
			}
			*/


			onGUI ();

		}

		protected override void OnWindowGUIAfterContent ()
		{
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();

			// display Save button
			if (GUILayout.Button ("Save", GUILayout.ExpandWidth (false)))
				SaveSettings ();

			GUILayout.Space (5);

			// display Load button
			if (GUILayout.Button ("Load", GUILayout.ExpandWidth (false)))
				LoadSettings ();

			GUILayout.EndHorizontal ();

			GUILayout.Space (5);
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

		public	static	void	DisplayInput<T>( Input<T> input )
		{
			input.Display ();
		}


		public static void RegisterInput (Input input)
		{
			s_registeredInputs.AddIfNotPresent ( input );
		}

		public static void RegisterInputs (string category, params Input[] inputs)
		{
			foreach (var input in inputs)
			{
				input.category = category;
				RegisterInput (input);
			}
		}

		public static void LoadSettings (InputPersistType persistType)
		{
			var inputs = s_registeredInputs.Where (input => input.persistType == persistType).ToArray ();

			Debug.Log ("The following inputs will be loaded: " + string.Join(", ", inputs.Select( i => i.description )));

			foreach (var input in inputs)
			{
				F.RunExceptionSafe (() => input.Load ());
			}

		}

		public static void LoadSettings ()
		{
			LoadSettings (InputPersistType.OnStart);
			if (Behaviours.Loader.HasLoaded)
				LoadSettings (InputPersistType.AfterLoaderFinishes);
		}

		public static void SaveSettings ()
		{
			var inputs = s_registeredInputs.Where (input => input.persistType != InputPersistType.None).ToArray ();

			Debug.Log ("The following inputs will be saved: " + string.Join(", ", inputs.Select( i => i.description )));

			foreach (var input in inputs)
			{
				F.RunExceptionSafe (() => input.Save ());
			}

			PlayerPrefs.Save ();

		}

	}

}
