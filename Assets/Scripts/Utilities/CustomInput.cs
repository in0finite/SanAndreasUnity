using UnityEngine;
using System.Collections.Generic;

namespace SanAndreasUnity.Utilities
{

	public class CustomInput : MonoBehaviour
	{
		public static CustomInput Instance { get; private set; }

		public bool IsActive { get; set; } = false;

		Dictionary<string,float> axes = new Dictionary<string, float>();
		Dictionary<string,bool> buttons = new Dictionary<string, bool>();
		Dictionary<string,bool> buttonsDown = new Dictionary<string, bool>();
		Dictionary<KeyCode,bool> keysDown = new Dictionary<KeyCode, bool>();



		public float GetAxis(string name){
			if (!this.IsActive)
				return Input.GetAxis(name);
			float value = 0;
			if (axes.TryGetValue (name, out value))
				return value;
			return Input.GetAxis(name);
		}

		public float GetAxisRaw(string name){
			if (!this.IsActive)
				return Input.GetAxisRaw(name);
			float value = 0;
			if (axes.TryGetValue (name, out value))
				return value;
			return Input.GetAxisRaw(name);
		}

		public void SetAxis(string name, float value){
			axes [name] = value;
		}

		public bool GetButton(string name){
			if (!this.IsActive)
				return Input.GetButton(name);
			bool value = false;
			if (buttons.TryGetValue (name, out value))
				return value;
			return Input.GetButton(name);
		}

		public bool GetButtonNoDefaultInput(string name){
			if (!this.IsActive)
				return false;
			bool value = false;
			buttons.TryGetValue (name, out value);
			return value;
		}

		public bool HasButton(string name){
			if (!this.IsActive)
				return false;
			return buttons.ContainsKey (name);
		}

		public bool GetButtonDown(string name){
			if (!this.IsActive)
				return Input.GetButtonDown(name);
			bool value = false;
			if (buttonsDown.TryGetValue (name, out value))
				return value;
			return Input.GetButtonDown(name);
		}

		public void SetButton(string name, bool pressed){
			buttons [name] = pressed;
		}

		public void SetButtonDown(string name, bool pressed){
			buttonsDown [name] = pressed;
		}

		public bool GetKeyDown(KeyCode keyCode){
			if (!this.IsActive)
				return Input.GetKeyDown(keyCode);
			bool value = false;
			if (keysDown.TryGetValue (keyCode, out value))
				return value;
			return Input.GetKeyDown(keyCode);
		}

		public void SetKeyDown(KeyCode keyCode, bool pressed)
		{
			keysDown [keyCode] = pressed;
		}

		public void ResetAllInput()
		{
			axes.Clear();
			buttons.Clear();
			buttonsDown.Clear();
			keysDown.Clear();
		}


		void Awake()
		{
			Instance = this;
		}

	}

}
