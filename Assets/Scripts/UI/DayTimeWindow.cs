using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours.World;

namespace SanAndreasUnity.UI {

	public class DayTimeWindow : PauseMenuWindow {

		public byte[] availableHours = {7, 12, 15, 18, 21, 0, 3};
		private string m_hoursText = "";
		private string m_minutesText = "";


		DayTimeWindow() {

			// set default parameters

			this.windowName = "DayTime";

		}

		void Start () {
			
			this.RegisterButtonInPauseMenu ();

			// adjust rect
			this.windowRect = new Rect(10, Screen.height - 220, 250, 200);
		}


		protected override void OnWindowGUI ()
		{

			GUILayout.Label($"Current time: {WorldController.Singleton.CurrentTimeHours}:{WorldController.Singleton.CurrentTimeMinutes}");

			GUILayout.Space(15);

			GUILayout.Label("Set Time:");

			GUILayout.BeginHorizontal();

			foreach (byte hour in this.availableHours)
			{
				if (GUILayout.Button(hour.ToString()))
					WorldController.Singleton.SetTime(hour, 0, true);
			}

			GUILayout.EndHorizontal();

			GUILayout.Space(15);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Hours:");
			m_hoursText = GUILayout.TextField(m_hoursText);
			GUILayout.Label("Minutes:");
			m_minutesText = GUILayout.TextField(m_minutesText);
			GUILayout.EndHorizontal();

			if (GUILayout.Button("Set"))
			{
				if (byte.TryParse(m_hoursText, out byte hours) && byte.TryParse(m_minutesText, out byte minutes))
				{
					WorldController.Singleton.SetTime(hours, minutes, true);
				}
			}

		}

	}

}
