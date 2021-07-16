using SanAndreasUnity.Behaviours.World;
using UnityEngine;
using SanAndreasUnity.UI;

namespace SanAndreasUnity.Settings {

	public class WorldSettings : MonoBehaviour {

		private OptionsWindow.FloatInput m_maxDrawDistanceInput = new OptionsWindow.FloatInput() {
			description = "Max draw distance",
			minValue = WorldManager.MinMaxDrawDistance,
			maxValue = WorldManager.MaxMaxDrawDistance,
			getValue = () => WorldManager.Singleton.MaxDrawDistance,
			setValue = (value) => { WorldManager.Singleton.MaxDrawDistance = value; },
			persistType = OptionsWindow.InputPersistType.OnStart
		};


		void Awake ()
		{
			OptionsWindow.RegisterInputs ("WORLD", m_maxDrawDistanceInput);
		}
	}

}
