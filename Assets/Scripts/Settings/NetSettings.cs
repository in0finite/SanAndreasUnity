using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.UI;
using SanAndreasUnity.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Settings
{
    public class NetSettings : MonoBehaviour
    {
        OptionsWindow.FloatInput m_pedSyncRate = new OptionsWindow.FloatInput ("Ped sync rate", 1, 60) {
			isAvailable = () => PedManager.Instance != null,
			getValue = () => PedManager.Instance.pedSyncRate,
			setValue = (value) => { ApplyPedSyncRate(value); },
			persistType = OptionsWindow.InputPersistType.OnStart
		};

		OptionsWindow.FloatInput m_vehicleSyncRate = new OptionsWindow.FloatInput ("Vehicle sync rate", 1, 60) {
			isAvailable = () => VehicleManager.Instance != null,
			getValue = () => VehicleManager.Instance.vehicleSyncRate,
			setValue = (value) => { ApplyVehicleSyncRate(value); },
			persistType = OptionsWindow.InputPersistType.OnStart
		};
		OptionsWindow.BoolInput m_syncVehicleTransformUsingSyncVars = new OptionsWindow.BoolInput ("Sync vehicle transform using syncvars") {
			isAvailable = () => VehicleManager.Instance != null,
			getValue = () => VehicleManager.Instance.syncVehicleTransformUsingSyncVars,
			setValue = (value) => { VehicleManager.Instance.syncVehicleTransformUsingSyncVars = value; },
			persistType = OptionsWindow.InputPersistType.OnStart
		};
		OptionsWindow.BoolInput m_syncVehiclesLinearVelocity = new OptionsWindow.BoolInput ("Sync vehicle's linear velocity") {
			isAvailable = () => VehicleManager.Instance != null,
			getValue = () => VehicleManager.Instance.syncLinearVelocity,
			setValue = (value) => { VehicleManager.Instance.syncLinearVelocity = value; },
			persistType = OptionsWindow.InputPersistType.OnStart
		};
		OptionsWindow.BoolInput m_syncVehiclesAngularVelocity = new OptionsWindow.BoolInput ("Sync vehicle's angular velocity") {
			isAvailable = () => VehicleManager.Instance != null,
			getValue = () => VehicleManager.Instance.syncAngularVelocity,
			setValue = (value) => { VehicleManager.Instance.syncAngularVelocity = value; },
			persistType = OptionsWindow.InputPersistType.OnStart
		};
		OptionsWindow.BoolInput m_controlWheelsOnLocalPlayer = new OptionsWindow.BoolInput ("Control wheels on local player") {
			isAvailable = () => VehicleManager.Instance != null,
			getValue = () => VehicleManager.Instance.controlWheelsOnLocalPlayer,
			setValue = (value) => { VehicleManager.Instance.controlWheelsOnLocalPlayer = value; },
			persistType = OptionsWindow.InputPersistType.OnStart
		};
		OptionsWindow.BoolInput m_controlVehicleInputOnLocalPlayer = new OptionsWindow.BoolInput ("Control vehicle input on local player") {
			isAvailable = () => VehicleManager.Instance != null,
			getValue = () => VehicleManager.Instance.controlInputOnLocalPlayer,
			setValue = (value) => { VehicleManager.Instance.controlInputOnLocalPlayer = value; },
			persistType = OptionsWindow.InputPersistType.OnStart
		};
		OptionsWindow.EnumInput<WhenOnClient> m_whenToDisableVehiclesRigidBody = new OptionsWindow.EnumInput<WhenOnClient> () {
			description = "When to disable vehicle rigid body on clients",
			isAvailable = () => VehicleManager.Instance != null,
			getValue = () => VehicleManager.Instance.whenToDisableRigidBody,
			setValue = (value) => { ApplyRigidBodyState(value); },
			persistType = OptionsWindow.InputPersistType.OnStart
		};
		OptionsWindow.BoolInput m_syncPedTransformWhileInVehicle = new OptionsWindow.BoolInput ("Sync ped transform while in vehicle") {
			isAvailable = () => VehicleManager.Instance != null,
			getValue = () => VehicleManager.Instance.syncPedTransformWhileInVehicle,
			setValue = (value) => { VehicleManager.Instance.syncPedTransformWhileInVehicle = value; },
			persistType = OptionsWindow.InputPersistType.OnStart
		};

        private void Awake()
        {
            OptionsWindow.RegisterInputs ("NET",
	            m_pedSyncRate,
	            m_vehicleSyncRate,
	            m_syncVehicleTransformUsingSyncVars,
	            m_syncVehiclesLinearVelocity,
	            m_syncVehiclesAngularVelocity,
	            m_controlWheelsOnLocalPlayer,
	            m_controlVehicleInputOnLocalPlayer,
	            m_whenToDisableVehiclesRigidBody,
	            m_syncPedTransformWhileInVehicle);
        }

        static void ApplyPedSyncRate(float syncRate)
        {
	        PedManager.Instance.pedSyncRate = syncRate;
	        foreach (var ped in Ped.AllPedsEnumerable)
		        ped.ApplySyncRate(syncRate);
        }

        static void ApplyVehicleSyncRate(float syncRate)
        {
	        VehicleManager.Instance.vehicleSyncRate = syncRate;
	        foreach (var v in Vehicle.AllVehicles)
	        {
		        v.ApplySyncRate(syncRate);
	        }
        }

        static void ApplyRigidBodyState(WhenOnClient when)
        {
	        VehicleManager.Instance.whenToDisableRigidBody = when;
	        foreach (var v in Vehicle.AllVehicles)
		        v.GetComponent<VehicleController>().EnableOrDisableRigidBody();
        }
    }
}
