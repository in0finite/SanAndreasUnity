using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Behaviours.Peds;
using SanAndreasUnity.Behaviours.Vehicles;
using UGameCore.Utilities;
using System;

namespace SanAndreasUnity.Stats
{
    public class MiscStats : MonoBehaviour
    {
        private int m_nestingLevel = 0;

        
        void Start()
        {
            UGameCore.Utilities.Stats.RegisterStat(new UGameCore.Utilities.Stats.Entry(){category = "MISC", getStatsAction = GetStats});
        }

        void GetStats(UGameCore.Utilities.Stats.GetStatsContext context)
        {

            m_nestingLevel = 0;

            var sb = context.stringBuilder;

            sb.AppendFormat("num peds: {0}\n", Ped.NumPeds);
            sb.AppendFormat("num vehicles: {0}\n", Vehicle.NumVehicles);
            sb.AppendFormat("num dead bodies: {0}\n", DeadBody.NumDeadBodies);
            sb.AppendFormat("num bones in dead bodies: {0}\n", DeadBody.DeadBodies.Sum(db => db.NumBones));
            sb.AppendFormat("num rigid bodies in dead bodies: {0}\n", DeadBody.DeadBodies.Sum(db => db.NumRigidBodies));
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
                sb.AppendFormat("transform syncer: \n");
                AddTransformSyncer(sb, ped.NetTransform.TransformSyncer, "\t");
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
                        vehicle.Input.accelerator,
                        vehicle.Input.isHandBrakeOn,
                        vehicle.Input.steering,
                        vehicle.AverageWheelHeight,
                        vehicle.NetTransform.netId,
                        vehicle.NetTransform.syncInterval,
                        vehicle.NetTransform.ComponentIndex,
                    };

                    var texts = new List<string>() {"velocity", "accelerator", "is handbrake on", "steering angle", "average wheel height", 
                        "net id", "sync interval", "component index"};


                    texts.Add("wheels");
                    objects.Add("");
                    foreach (var w in vehicle.Wheels)
                    {
                        texts.Add("\t" + w.Alignment);
                        objects.Add(
                            w.Collider != null
                                ? string.Format("travel {0} rpm {1} radius {2} motor torque {3} brake torque {4} mass {5} is grounded {6}",
                                    w.Travel, w.Collider.rpm, w.Collider.radius, w.Collider.motorTorque, w.Collider.brakeTorque, w.Collider.mass, w.Collider.isGrounded)
                                : "");
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
                        objects.AddRange(new object[] { vehicle.RadioAudioSource.time, clip.length, (F.GetAudioClipSizeInBytes(clip) / 1024.0f) + " KB" });
                    }


                    sb.AppendFormat("Current vehicle:\n");
                    for (int i = 0; i < objects.Count; i++)
                    {
                        sb.AppendFormat("{0}: {1}\n", texts[i], objects[i]);
                    }

                    sb.AppendFormat("transform syncer: \n");
                    AddTransformSyncer(sb, vehicle.NetTransform.TransformSyncer, "\t");

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

            // time
            sb.AppendLine("time:");
            m_nestingLevel++;
            AddTimeSpan(sb, "time", Time.timeAsDouble);
            AddTimeSpan(sb, "unscaled time", Time.unscaledTimeAsDouble);
            AddTimeSpan(sb, "fixed time", Time.fixedTimeAsDouble);
            AddTimeSpan(sb, "fixed unscaled time", Time.fixedUnscaledTimeAsDouble);
            AddTimeSpan(sb, "realtime since startup", Time.realtimeSinceStartupAsDouble);
            AddTimeSpan(sb, "time since level load", Time.timeSinceLevelLoadAsDouble);
            AddAsMs(sb, "delta time", Time.deltaTime);
            AddAsMs(sb, "fixed delta time", Time.fixedDeltaTime);
            AddAsMs(sb, "smooth delta time", Time.smoothDeltaTime);
            AddAsMs(sb, "maximum delta time", Time.maximumDeltaTime);
            Add(sb, "FPS", Mathf.RoundToInt(1.0f / Time.deltaTime).ToString());
            Add(sb, "smooth FPS", Mathf.RoundToInt(1.0f / Time.smoothDeltaTime).ToString());
            Add(sb, "frame count", $"{(Time.frameCount / 1000.0):0.00} K");
            Add(sb, "rendered frame count", $"{(Time.renderedFrameCount / 1000.0):0.00} K");
            m_nestingLevel--;
            sb.AppendLine();

            // on-screen messages
            sb.AppendFormat("num on-screen messages: {0}\n", OnScreenMessageManager.Instance.Messages.Count);
            sb.AppendFormat("num pooled on-screen messages: {0}\n", OnScreenMessageManager.Instance.NumPooledMessages);
            sb.AppendLine();

            // loading thread
            sb.Append("loading thread:\n");
            sb.Append($"\tmax time per frame ms: {Importing.LoadingThread.Singleton.maxTimePerFrameMs}\n");
            AppendStatsForBackgroundJobRunner(sb, Importing.LoadingThread.Singleton.BackgroundJobRunner, "\t");
            sb.AppendLine();

            // pathfinding manager
            sb.Append("pathfinding manager:\n");
            sb.Append($"\tmax time per frame ms: {PathfindingManager.Singleton.MaxTimePerFrameMs}\n");
            AppendStatsForBackgroundJobRunner(sb, PathfindingManager.Singleton.BackgroundJobRunner, "\t");
            sb.AppendLine();

        }

        private void AddNesting(System.Text.StringBuilder sb)
        {
            for (int i = 0; i < m_nestingLevel; i++)
                sb.Append('\t');
        }

        private void Add(System.Text.StringBuilder sb, string text, string value)
        {
            this.AddNesting(sb);

            sb.Append(text);
            sb.Append(": ");
            sb.Append(value);
            sb.AppendLine();
        }

        private void AddTimeSpan(System.Text.StringBuilder sb, string text, double seconds)
        {
            this.Add(sb, text, F.FormatElapsedTime(seconds));
        }

        private void AddAsMs(System.Text.StringBuilder sb, string text, double seconds)
        {
            this.Add(sb, text, $"{seconds * 1000:0.00} ms");
        }

        private static void AppendStatsForBackgroundJobRunner(
            System.Text.StringBuilder sb,
            BackgroundJobRunner backgroundJobRunner,
            string prefix)
        {
            sb.Append($"{prefix}is background thread running: {backgroundJobRunner.IsBackgroundThreadRunning()}\n");
            sb.Append($"{prefix}background thread id: {backgroundJobRunner.GetBackgroundThreadId()}\n");
            sb.Append($"{prefix}num pending jobs: {backgroundJobRunner.GetNumPendingJobs()}\n");
            sb.Append($"{prefix}last processed job id: {backgroundJobRunner.GetLastProcessedJobId()}\n");
            sb.Append($"{prefix}processed jobs buffer count: {backgroundJobRunner.GetProcessedJobsBufferCount()}\n");
        }

        private static void AddTransformSyncer(
            System.Text.StringBuilder sb,
            Net.TransformSyncer transformSyncer,
            string prefix)
        {
            var parameters = transformSyncer.Params;
            sb.AppendLine($"{prefix}client update type: {parameters.clientUpdateType}");
            sb.AppendLine($"{prefix}snapshot latency: {parameters.snapshotLatency}");
            sb.AppendLine($"{prefix}snapshot buffer count: {transformSyncer.SnapshotBufferCount}");
            sb.AppendLine($"{prefix}use rigid body: {parameters.useRigidBody}");
            sb.AppendLine($"{prefix}calculated velocity: {transformSyncer.CurrentSyncData.CalculatedVelocityMagnitude}");
            sb.AppendLine($"{prefix}calculated angular velocity: {transformSyncer.CurrentSyncData.CalculatedAngularVelocityMagnitude}");
            if (transformSyncer.Transform != null)
            {
                sb.AppendLine($"{prefix}distance: {Vector3.Distance(transformSyncer.CurrentSyncData.Position, transformSyncer.Transform.localPosition)}");
                sb.AppendLine($"{prefix}angle: {Quaternion.Angle(transformSyncer.CurrentSyncData.Rotation, transformSyncer.Transform.localRotation)}");
            }
        }

    }
}
