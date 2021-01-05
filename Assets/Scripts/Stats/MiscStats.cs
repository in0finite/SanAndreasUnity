using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Behaviours.Peds;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Stats
{
    public class MiscStats : MonoBehaviour
    {
        
        void Start()
        {
            Utilities.Stats.RegisterStat(new Utilities.Stats.Entry(){category = "MISC", onGUI = OnStatGUI});
        }

        void OnStatGUI()
        {

            var sb = new System.Text.StringBuilder();

            sb.AppendFormat("num peds: {0}\n", Ped.NumPeds);
            sb.AppendFormat("num vehicles: {0}\n", Vehicle.NumVehicles);
            sb.AppendFormat("num dead bodies: {0}\n", DeadBody.NumDeadBodies);
            sb.AppendFormat("num ped state changes received: {0}\n", Ped.NumStateChangesReceived);

            sb.AppendLine();

            // info about local ped

            var ped = Ped.Instance;
            if (ped != null)
            {
                sb.AppendFormat("Local ped:\n");
                sb.AppendFormat("position: {0}\n", ped.transform.position);
                sb.AppendFormat("net id: {0}\n", ped.netId);
                sb.AppendFormat("sync interval: {0}\n", ped.NetTransform.syncInterval);
                sb.AppendFormat("state: {0}\n", ped.CurrentState != null ? ped.CurrentState.GetType().Name : "");
                sb.AppendFormat("velocity: {0}\n", ped.Velocity);
                sb.AppendFormat("is grounded: {0}\n", ped.IsGrounded);
                sb.AppendFormat("vehicle offset: {0}\n", ped.PlayerModel.VehicleParentOffset);
                sb.AppendFormat("model id: {0}\n", ped.PedDef != null ? ped.PedDef.Id.ToString() : "");
                sb.AppendFormat("model name: {0}\n", ped.PedDef != null ? ped.PedDef.ModelName : "");
                sb.AppendFormat("\n");

                // info about current vehicle

                var vehicle = ped.CurrentVehicle;
                if (vehicle != null)
                {
                    List<System.Object> objects = new List<System.Object>(){
                        vehicle.Velocity,
                        vehicle.Accelerator,
                        vehicle.Braking,
                        vehicle.Steering,
                        vehicle.AverageWheelHeight,
                        vehicle.NetTransform.netId,
                        vehicle.NetTransform.syncInterval,
                        vehicle.NetTransform.ComponentIndex,
                    };

                    var texts = new List<string>() {"velocity", "accelerator", "braking", "steering", "average wheel height", 
                        "net id", "sync interval", "component index"};


                    texts.Add("wheels");
                    objects.Add("");
                    foreach (var w in vehicle.Wheels)
                    {
                        texts.Add("\t" + w.Alignment);
                        objects.Add( string.Format("travel {0} rpm {1} radius {2} motor torque {3} mass {4} is grounded {5}", 
                            w.Travel, w.Collider.rpm, w.Collider.radius, w.Collider.motorTorque, w.Collider.mass, w.Collider.isGrounded) );

                    }

                    if (vehicle.Definition != null)
                    {
                        var def = vehicle.Definition;
                        texts.Add("game name");
                        texts.Add("type");
                        objects.Add(def.GameName);
                        objects.Add(def.VehicleType);
                    }

                    texts.Add("rigid body");
                    objects.Add("");
                    if (vehicle.RigidBody != null)
                    {
                        var rb = vehicle.RigidBody;
                        texts.AddRange(new string[]{"\tmass", "\tvelocity", "\tangular velocity"});
                        objects.AddRange(new object[]{rb.mass, rb.velocity, rb.angularVelocity});
                    }

                    texts.Add("seats");
                    objects.Add("");
                    foreach(var seat in vehicle.Seats)
                    {
                        texts.Add("\t" + seat.Alignment);
                        var p = seat.OccupyingPed;
                        objects.Add(p != null ? ("ped: net id " + p.netId) : "empty");
                    }

                    var closestSeat = vehicle.FindClosestSeatTransform(ped.transform.position);
                    if (closestSeat != null)
                    {
                        texts.Add("distance to closest seat");
                        objects.Add(Vector3.Distance(closestSeat.position, ped.transform.position));
                    }

                    // damage

                    texts.Add("damage");
                    objects.Add("");

                    texts.AddRange(new string[] { "\thealth", "\tmax health", "\tis under flame", "\tis under smoke", "\ttime since became under flame" });
                    objects.AddRange(new object[] { vehicle.Health, vehicle.MaxHealth, vehicle.IsUnderFlame, vehicle.IsUnderSmoke, vehicle.TimeSinceBecameUnderFlame });

                    // radio

                    texts.Add("radio");
                    objects.Add("");

                    texts.AddRange(new string[] { "\tis playing", "\tstation index", "\tis waiting for new sound" });
                    objects.AddRange(new object[] { vehicle.IsPlayingRadio, vehicle.CurrentRadioStationIndex, vehicle.IsWaitingForNewRadioSound });
                    
                    if (vehicle.RadioAudioSource != null && vehicle.RadioAudioSource.clip != null)
                    {
                        var clip = vehicle.RadioAudioSource.clip;
                        texts.AddRange(new string[] { "\tclip time", "\tclip length", "\tclip size" });
                        objects.AddRange(new object[] { vehicle.RadioAudioSource.time, clip.length, (Utilities.F.GetAudioClipSizeInBytes(clip) / 1024.0f) + " KB" });
                    }


                    sb.AppendFormat("Current vehicle:\n");
                    for (int i = 0; i < objects.Count; i++)
                    {
                        sb.AppendFormat("{0}: {1}\n", texts[i], objects[i]);
                    }
                    sb.AppendFormat("\n");

                }

                // info about current weapon
                var weapon = ped.CurrentWeapon;
                if (weapon != null)
                {
                    sb.AppendFormat("Current weapon:\n");

                    sb.AppendFormat("net id: {0}\n", weapon.NetWeapon.netId);

                    var def = weapon.Definition;
                    if (def != null)
                    {
                        sb.AppendFormat("model id: {0}\n", def.Id);
                        sb.AppendFormat("name: {0}\n", def.ModelName);
                    }
                    
                    sb.AppendFormat("max range: {0}\n", weapon.MaxRange);
                    sb.AppendFormat("damage: {0}\n", weapon.Damage);
                    sb.AppendFormat("ammo clip size: {0}\n", weapon.AmmoClipSize);
                    sb.AppendFormat("ammo: {0} / {1}\n", weapon.AmmoInClip, weapon.AmmoOutsideOfClip);
                    sb.AppendFormat("slot: {0}\n", weapon.SlotIndex);
                    
                    sb.AppendLine();
                }

                // info about all weapons
                sb.AppendFormat("All weapons:\n");
                foreach (var w in ped.WeaponHolder.AllWeapons)
                {
                    sb.AppendFormat("\tslot: {0} name: {1}\n", w.SlotIndex, w.Definition.ModelName);
                }
                sb.AppendLine();

            }

            // on-screen messages
            sb.AppendFormat("num on-screen messages: {0}\n", OnScreenMessageManager.Instance.Messages.Count);
            sb.AppendFormat("num pooled on-screen messages: {0}\n", OnScreenMessageManager.Instance.NumPooledMessages);


            GUILayout.Label(sb.ToString());

        }

    }
}
