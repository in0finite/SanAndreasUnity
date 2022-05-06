using SanAndreasUnity.Importing.Animation;
using SanAndreasUnity.Importing.Items.Definitions;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class PanicState : BaseMovementState
	{
        public override AnimId movementAnim => new AnimId(AnimGroup.WalkCycle, AnimIndex.Panicked);
        public override AnimId movementWeaponAnim => this.movementAnim;


		private const string kSfxFileName = "PAIN_A";
		private const int kMaleBankIndex = 2;
		private const int kMaleSoundIndexMin = 69;
		private const int kMaleSoundIndexMax = 100;
		private const int kFemaleBankIndex = 1;
		private const int kFemaleSoundIndexMin = 88;
		private const int kFemaleSoundIndexMax = 130;

		private double _timeWhenEmittedSound = 0;
		public float randomAverageTimeIntervalToEmitSound = 10f;
		private float _timeToEmitSound;


        public override void UpdateState()
        {
			if (m_isServer)
				this.TryEmitSound();

            base.UpdateState();
        }

        protected override void SwitchToAimState()
		{
			// don't switch to aim state
			// we will switch to aim state from other movement states
		}

		private Audio.SoundId GetSoundToPlay()
        {
			if (null == m_ped.PedDef)
				return default;

			var soundId = new Audio.SoundId
            {
				fileName = kSfxFileName,
				isStream = false,
            };

			if (m_ped.PedDef.DefaultType.IsMale())
            {
				soundId.bankIndex = kMaleBankIndex;
				soundId.audioIndex = Random.Range(kMaleSoundIndexMin, kMaleSoundIndexMax + 1);
            }
            else
            {
				soundId.bankIndex = kFemaleBankIndex;
				soundId.audioIndex = Random.Range(kFemaleSoundIndexMin, kFemaleSoundIndexMax + 1);
			}

			return soundId;
        }

		private void TryEmitSound()
        {
			if (Time.timeAsDouble - _timeWhenEmittedSound < _timeToEmitSound)
				return;

			_timeToEmitSound = Random.Range(
					this.randomAverageTimeIntervalToEmitSound * 0.5f,
					this.randomAverageTimeIntervalToEmitSound * 1.5f);

			_timeWhenEmittedSound = Time.timeAsDouble;
			m_ped.PlaySoundFromPedMouth(this.GetSoundToPlay());
		}

	}

}
