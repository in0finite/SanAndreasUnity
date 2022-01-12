using SanAndreasUnity.Behaviours.World;
using UnityEngine;
using SanAndreasUnity.UI;

namespace SanAndreasUnity.Settings
{
	public class WorldSettings : MonoBehaviour
	{
		private static WorldSettings Singleton { get; set; }

		private float _drawDistanceToApply = 0f;

		public const string DrawDistanceSerializationName = "max_draw_distance";

		private OptionsWindow.FloatInput m_maxDrawDistanceInput = new OptionsWindow.FloatInput
		{
			serializationName = DrawDistanceSerializationName,
			description = "Max draw distance",
			minValue = WorldManager.MinMaxDrawDistance,
			maxValue = WorldManager.MaxMaxDrawDistance,
			getValue = () => WorldManager.Singleton.MaxDrawDistance,
			setValue = value => { Singleton.OnDrawDistanceChanged(value); },
			persistType = OptionsWindow.InputPersistType.OnStart,
		};

		public float overridenMaxDrawDistance = 0f;


		void Awake ()
		{
			Singleton = this;

			if (this.overridenMaxDrawDistance > 0f)
				m_maxDrawDistanceInput.maxValue = this.overridenMaxDrawDistance;

			OptionsWindow.RegisterInputs ("WORLD", m_maxDrawDistanceInput);
		}

		void OnDrawDistanceChanged(float newValue)
		{
			this.CancelInvoke(nameof(ChangeDrawDistanceDelayed));
			_drawDistanceToApply = newValue;
			this.Invoke(nameof(ChangeDrawDistanceDelayed), 0.2f);
		}

		void ChangeDrawDistanceDelayed()
		{
			WorldManager.Singleton.MaxDrawDistance = _drawDistanceToApply;
		}
	}

}
