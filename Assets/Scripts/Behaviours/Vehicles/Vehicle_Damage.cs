using SanAndreasUnity.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{

    public partial class Vehicle
    {

        public Damageable Damageable { get; private set; }

        public float Health { get => this.Damageable.Health; set => this.Damageable.Health = value; }

        public float MaxHealth { get; set; } = 1000;

        public bool IsUnderFlame { get; private set; } = false;
        public bool IsUnderSmoke { get; private set; } = false;

        public float TimeWhenBecameUnderFlame { get; private set; } = float.NegativeInfinity;



        void Awake_Damage()
        {
            this.Damageable = this.GetComponentOrThrow<Damageable>();
            this.Damageable.OnDamageEvent.AddListener(() => this.OnDamaged());
        }

        void OnDamaged()
        {
            var damageInfo = this.Damageable.LastDamageInfo;

            if (this.Health <= 0)
                return;

            this.Health -= damageInfo.amount;

            if (this.Health <= 0)
            {
                this.Explode();
            }

        }

        void Update_Damage()
        {

            bool shouldBeUnderSmoke = this.MaxHealth * 0.33f >= this.Health;
            if (shouldBeUnderSmoke != this.IsUnderSmoke)
            {
                // smoke status changed
                this.IsUnderSmoke = shouldBeUnderSmoke;
                // update vfx

            }

            bool shouldBeUnderFlame = this.MaxHealth * 0.1f >= this.Health;
            if (shouldBeUnderFlame != this.IsUnderFlame)
            {
                // flame status changed
                this.IsUnderFlame = shouldBeUnderFlame;
                if (this.IsUnderFlame)
                    this.TimeWhenBecameUnderFlame = Time.time;
                // update vfx

            }

            if (this.IsUnderFlame && Time.time - this.TimeWhenBecameUnderFlame >= 5)
            {
                // enough time passed since vehicle flamed - explode it
                this.Explode();
            }

        }

        public void Explode()
        {

        }

    }

}
