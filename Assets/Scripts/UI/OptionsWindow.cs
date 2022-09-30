using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UGameCore.Utilities;

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
			public string serializationName = "";
			public string description = "";
			public InputPersistType persistType = InputPersistType.None;
			public string category = "";
			public System.Func<bool> isAvailable = () => true;

			public string FinalSerializationName => string.IsNullOrWhiteSpace(this.serializationName)
				? (string.IsNullOrWhiteSpace(this.description)
					? throw new ArgumentException("You must specify serialization name or description")
					: this.description)
				: this.serializationName;

			public abstract void Load ();
			public abstract void Save ();
			public abstract void Display ();
			public abstract void SetValueNonGeneric(object value);
			public abstract object GetValueNonGeneric();
			public abstract void SetDefaultValueNonGeneric(object value);
			public abstract object GetDefaultValueNonGeneric();

		}

		public abstract class Input<T> : Input
		{
			public T defaultValue = default(T);
			public System.Func<T> getValue = () => default(T);
			public System.Action<T> setValue = (val) => {};


			public Input ()
			{
			}

			public Input (string description)
			{
				this.description = description;
			}

			public sealed override void Display ()
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

			public sealed override void Load () {
				if (!this.isAvailable ())
					return;
				if (!PlayerPrefs.HasKey (this.FinalSerializationName))
					return;
				string str = PlayerPrefs.GetString (this.FinalSerializationName, null);
				if (str != null)
				{
					this.setValue (this.Load (str));
				}
			}
			public abstract T Load (string str);

			public sealed override void Save ()
			{
				if (!this.isAvailable ())
					return;

				T currentValue = this.getValue();
				if (currentValue.Equals(this.defaultValue) && !PlayerPrefs.HasKey(this.FinalSerializationName))
					return;

				string str = this.SaveAsString (currentValue);
				if (str != null)
					PlayerPrefs.SetString (this.FinalSerializationName, str);
			}

			public virtual string SaveAsString (T value) {
				return value.ToString ();
			}

			public sealed override void SetValueNonGeneric(object value)
			{
				this.setValue((T)value);
			}

			public sealed override object GetValueNonGeneric()
			{
				return this.getValue();
			}

			public sealed override void SetDefaultValueNonGeneric(object value)
			{
				this.defaultValue = (T) value;
			}

			public sealed override object GetDefaultValueNonGeneric()
			{
				return this.defaultValue;
			}

		}

		public class IntInput : Input<int>
		{
			public int minValue;
			public int maxValue;

			public IntInput () { }

			public IntInput (string description, int minValue, int maxValue) : base (description)
			{
				this.minValue = minValue;
				this.maxValue = maxValue;
			}

			public override int Display (int currentValue)
			{
				return Mathf.RoundToInt( OptionsWindow.FloatSlider (currentValue, this.minValue, this.maxValue, this.description) );
			}

			public override int Load (string str)
			{
				int value = int.Parse (str, System.Globalization.CultureInfo.InvariantCulture);
				value = Mathf.Clamp(value, this.minValue, this.maxValue);
				return value;
			}

			public override string SaveAsString (int value)
			{
				return value.ToString (System.Globalization.CultureInfo.InvariantCulture);
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
				float value = float.Parse (str, System.Globalization.CultureInfo.InvariantCulture);
				value = Mathf.Clamp(value, this.minValue, this.maxValue);
				return value;
			}

			public override string SaveAsString (float value)
			{
				return value.ToString (System.Globalization.CultureInfo.InvariantCulture);
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

		public class StringInput : Input<string>
		{
			public int displayWidth = 200;
			public int maxNumCharacters = 0;

			public override string Display(string currentValue)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(this.description + ":");
				GUILayout.Space(5);
				currentValue = GUILayout.TextField(currentValue, this.maxNumCharacters, GUILayout.Width(this.displayWidth));
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				return currentValue;
			}

			public override string Load(string str)
			{
				return str;
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
			float windowWidth = Mathf.Max (Screen.width * 0.7f, 600);
			float windowHeight = windowWidth * 9f / 16f;
			this.windowRect = GUIUtils.GetCenteredRect (new Vector2 (windowWidth, windowHeight));

			SaveDefaultValues();

			LoadSettings (InputPersistType.OnStart);

		}

		protected override void OnLoaderFinished ()
		{
			LoadSettings (InputPersistType.AfterLoaderFinishes);

			base.OnLoaderFinished();
		}


		protected override void OnWindowStart()
		{
			base.OnWindowStart();
			m_scrollViewStyle = GUI.skin.box;
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


			onGUI ();

		}

		protected override void OnWindowGUIAfterContent ()
		{
			GUILayout.BeginHorizontal ();

			if (GUIUtils.ButtonWithCalculatedSize ("Reset to defaults"))
				ResetSettingsToDefaults ();

			GUILayout.FlexibleSpace ();

			// display Save button
			if (GUIUtils.ButtonWithCalculatedSize ("Save"))
				SaveSettings ();

			GUILayout.Space (5);

			// display Load button
			if (GUIUtils.ButtonWithCalculatedSize ("Load"))
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

		static void SaveDefaultValues()
		{

			foreach (var input in s_registeredInputs)
			{
				F.RunExceptionSafe (() => {
					// assign default value
					if (input.isAvailable())
					{
						input.SetDefaultValueNonGeneric(input.GetValueNonGeneric());
					}
					else
					{
						Debug.LogErrorFormat("Input '{0}' was not available when tried to obtain default value", input.description);
					}
				});
			}

		}

		public static void ResetSettingsToDefaults()
		{

			foreach (var input in s_registeredInputs)
			{
				F.RunExceptionSafe (() => {
					if (input.isAvailable())
					{
						object defaultValue = input.GetDefaultValueNonGeneric();
						object currentValue = input.GetValueNonGeneric();
						if (! defaultValue.Equals(currentValue))
							input.SetValueNonGeneric(defaultValue);
					}
				});
			}

		}

	}

}
