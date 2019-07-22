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



		public float GetAxis(string name){
			if (!this.IsActive)
				return Input.GetAxis(name);
			float value = 0;
			if (axes.TryGetValue (name, out value))
				return value;
			return Input.GetAxis(name);
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

		public void SetButton(string name, bool pressed){
			buttons [name] = pressed;
		}

		public void ResetAllInput()
		{
			axes.Clear();
			buttons.Clear();
		}


		void Awake()
		{
			Instance = this;
		}

	}

}
