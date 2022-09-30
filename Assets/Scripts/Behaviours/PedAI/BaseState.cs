using System;
using System.Collections.Generic;
using SanAndreasUnity.Behaviours.Vehicles;
using UGameCore.Utilities;

namespace SanAndreasUnity.Behaviours.Peds.AI
{
    /// <summary>
    /// Base class for all ped AI states.
    /// </summary>
    public abstract class BaseState : IState
    {
        protected PedAI _pedAI { get; private set; }

        protected Ped _ped => _pedAI.MyPed;
        protected Ped MyPed => _pedAI.MyPed;
        protected List<Ped> _enemyPeds => _pedAI.EnemyPeds;

        public object ParameterForEnteringState { get; set; }

        public double LastTimeWhenActivated { get; set; } = 0;
        public double LastTimeWhenDeactivated { get; set; } = 0;



        protected internal virtual void OnAwake(PedAI pedAI)
        {
            _pedAI = pedAI;
        }

        public virtual void OnBecameActive()
        {
        }

        public virtual void OnBecameInactive()
        {
        }

        public bool RepresentsState(Type type)
        {
            throw new NotSupportedException();
        }

        public bool RepresentsState<T>() where T : IState
        {
            throw new NotSupportedException();
        }

        public virtual void UpdateState()
        {
        }

        public virtual void LateUpdateState()
        {
        }

        public virtual void FixedUpdateState()
        {
        }

        public virtual void UpdateState2Seconds()
        {
        }

        protected internal virtual void OnMyPedDamaged(DamageInfo dmgInfo, Ped.DamageResult dmgResult)
        {
            Ped attackerPed = dmgInfo.GetAttackerPed();
            if (attackerPed != null)
                _enemyPeds.AddIfNotPresent(attackerPed);
        }

        protected internal virtual void OnOtherPedDamaged(Ped damagedPed, DamageInfo dmgInfo, Ped.DamageResult dmgResult)
        {
        }

        protected internal virtual void OnVehicleDamaged(Vehicle vehicle, DamageInfo damageInfo)
        {
        }

        protected internal virtual void OnWeaponConductedAttack(Weapon.AttackConductedEventData data)
        {
        }

        protected internal virtual void OnUnderAim(IReadOnlyList<Ped.UnderAimInfo> underAimInfos)
        {
        }

        protected internal virtual void OnRecruit(Ped recruiterPed)
        {
        }

        protected internal virtual void OnDrawGizmosSelected()
        {
        }
    }
}
