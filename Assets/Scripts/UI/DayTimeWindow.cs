using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours.World;

namespace SanAndreasUnity.UI {

	public class DayTimeWindow : PauseMenuWindow {

		public byte[] availableHours = {7, 12, 15, 18, 21, 0, 3};
		private string m_hoursText = "12";
		private string m_minutesText = "0";
		private string m_timeScaleText = "1";


		DayTimeWindow() {

			// set default parameters

			this.windowName = "DayTime";

		}

		void Start () {
			
			this.RegisterButtonInPauseMenu ();

			// adjust rect
			this.windowRect = new Rect(Screen.width * 0.7f, 10, 250, 300);
		}


		protected override void OnWindowGUI ()
		{
			if (null == DayTimeManager.Singleton)
			{
				GUILayout.Label($"{nameof(DayTimeManager)} not available");
				return;
			}

			GUILayout.Label($"Current time: {DayTimeManager.Singleton.CurrentTimeHours}:{DayTimeManager.Singleton.CurrentTimeMinutes}");
			GUILayout.Label($"Time scale: {DayTimeManager.Singleton.timeScale}");

			GUILayout.Space(15);

			GUILayout.Label("Set Time:");

			GUILayout.BeginHorizontal();

			foreach (byte hour in this.availableHours)
			{
				if (GUILayout.Button(hour.ToString()))
					DayTimeManager.Singleton.SetTime(hour, 0, true);
			}

			GUILayout.EndHorizontal();

			GUILayout.Space(15);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Hours:");
			m_hoursText = GUILayout.TextField(m_hoursText);
			GUILayout.Label("Minutes:");
			m_minutesText = GUILayout.TextField(m_minutesText);
			GUILayout.EndHorizontal();

			if (GUILayout.Button("Set time"))
			{
				if (byte.TryParse(m_hoursText, out byte hours) && byte.TryParse(m_minutesText, out byte minutes))
				{
					DayTimeManager.Singleton.SetTime(hours, minutes, true);
				}
			}

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Previous hour"))
				DayTimeManager.Singleton.SetTime((byte) (DayTimeManager.Singleton.CurrentTimeHours == 0 ? 23 : DayTimeManager.Singleton.CurrentTimeHours - 1), DayTimeManager.Singleton.CurrentTimeMinutes, true);
			if (GUILayout.Button("Next hour"))
				DayTimeManager.Singleton.SetTime((byte) ((DayTimeManager.Singleton.CurrentTimeHours + 1) % 24), DayTimeManager.Singleton.CurrentTimeMinutes, true);
			GUILayout.EndHorizontal();

			GUILayout.Space(15);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Time scale:");
			m_timeScaleText = GUILayout.TextField(m_timeScaleText);
			if (GUILayout.Button("Set scale"))
			{
				if (float.TryParse(m_timeScaleText, out float value))
					DayTimeManager.Singleton.timeScale = value;
			}
			GUILayout.EndHorizontal();

		}

	}

}
