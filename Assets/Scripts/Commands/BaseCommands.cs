using System.Collections.Generic;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Net;
using SanAndreasUnity.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Commands
{
    public class VehicleCommands : MonoBehaviour
    {
        public static VehicleCommands Singleton { get; private set; }

        private class PlayerData
        {
            public List<Vehicle> vehicles = new List<Vehicle>();
        }

        readonly Dictionary<Player, PlayerData> m_perPlayerData = new Dictionary<Player, PlayerData>();

        public float vehicleLimitInterval = 3f;
        public float pedLimitInterval = 2f;
        public float weaponLimitInterval = 1f;

        public int maxVehiclesPerPlayer = 3;


        void Awake()
        {
            Singleton = this;
        }

        void Start()
        {
            Player.onDisable += PlayerOnDisable;

            var commands = new CommandManager.CommandInfo[]
            {
                new CommandManager.CommandInfo("skin", "change skin", true, true, this.pedLimitInterval),
                new CommandManager.CommandInfo("stalker", "spawn stalker ped", true, true, this.pedLimitInterval),
                new CommandManager.CommandInfo("suicide", "commit suicide", true, true, this.pedLimitInterval),
                new CommandManager.CommandInfo("veh", "spawn a vehicle", true, true, this.vehicleLimitInterval),
                new CommandManager.CommandInfo("dveh", "destroy my vehicles", true, true, 0f),
                new CommandManager.CommandInfo("w", "give a weapon", true, true, this.weaponLimitInterval),
                new CommandManager.CommandInfo("rand_w", "give random weapons", true, true, this.weaponLimitInterval),
                new CommandManager.CommandInfo("rem_w", "remove all weapons", true, true, this.weaponLimitInterval),
                new CommandManager.CommandInfo("rem_current_w", "remove current weapon", true, true, this.weaponLimitInterval),
                new CommandManager.CommandInfo("ammo", "give ammo", true, true, this.weaponLimitInterval),
            };

            foreach (var immutableCmd in commands)
            {
                var cmd = immutableCmd;
                cmd.commandHandler = ProcessCommand;
                CommandManager.Singleton.RegisterCommand(cmd);
            }
        }

        private void PlayerOnDisable(Player player)
        {
            m_perPlayerData.Remove(player);
        }

        CommandManager.ProcessCommandResult ProcessCommand(CommandManager.ProcessCommandContext context)
        {
            string[] arguments = CommandManager.SplitCommandIntoArguments(context.command);
            int numArguments = arguments.Length;
            Player player = context.player;
            var pedNotAliveResult = CommandManager.ProcessCommandResult.Error("Your ped must be alive to run this command");

            if (arguments[0] == "skin")
            {
                if (null == player.OwnedPed)
                    return CommandManager.ProcessCommandResult.Error("Your ped must be alive to change skin");

                player.OwnedPed.PlayerModel.Load(Ped.RandomPedId);

                return CommandManager.ProcessCommandResult.Success;
            }
            else if (arguments[0] == "stalker")
            {
                if (null == player.OwnedPed)
                    return CommandManager.ProcessCommandResult.Error("Your ped must be alive to spawn stalker");

                Ped.SpawnPedStalker(Ped.RandomPedId, player.OwnedPed.transform, player.OwnedPed);

                return CommandManager.ProcessCommandResult.Success;
            }
            else if (arguments[0] == "suicide")
            {
                if (null == player.OwnedPed)
                    return CommandManager.ProcessCommandResult.Error("Your ped must be alive to commit suicide");

                player.OwnedPed.Kill();

                return CommandManager.ProcessCommandResult.Success;
            }
            else if (arguments[0] == "veh")
            {
                int id = -1;
                if (numArguments > 1)
                    id = int.Parse(arguments[1]);

                if (null == player.OwnedPed)
                    return CommandManager.ProcessCommandResult.Error("Your ped must be alive to spawn a vehicle");

                if (!m_perPlayerData.TryGetValue(player, out PlayerData playerData))
                    playerData = new PlayerData();

                playerData.vehicles.RemoveDeadObjects();
                if (playerData.vehicles.Count >= this.maxVehiclesPerPlayer)
                    return CommandManager.ProcessCommandResult.Error($"You can have a maximum of {this.maxVehiclesPerPlayer} vehicles");

                var vehicle = Vehicle.CreateInFrontOf(id, player.OwnedPed.transform);

                playerData.vehicles.Add(vehicle);
                m_perPlayerData[player] = playerData;

                return CommandManager.ProcessCommandResult.Success;
            }
            else if (arguments[0] == "dveh")
            {
                if (m_perPlayerData.TryGetValue(player, out var playerData))
                {
                    playerData.vehicles.RemoveDeadObjects();
                    foreach (var vehicle in playerData.vehicles)
                        vehicle.Explode();
                }

                return CommandManager.ProcessCommandResult.Success;
            }
            else if (arguments[0] == "w")
            {
                if (arguments.Length < 2)
                    return CommandManager.ProcessCommandResult.Error("Invalid syntax. Example: w 355");

                int modelId = int.Parse(arguments[1]);

                if (null == player.OwnedPed)
                    return pedNotAliveResult;

                var weapon = player.OwnedPed.WeaponHolder.SetWeaponAtSlot(modelId, 0);
                player.OwnedPed.WeaponHolder.SwitchWeapon(weapon.SlotIndex);
                WeaponHolder.AddRandomAmmoAmountToWeapon(weapon);

                return CommandManager.ProcessCommandResult.Success;
            }
            else if (arguments[0] == "rand_w")
            {
                if (null == player.OwnedPed)
                    return pedNotAliveResult;

                player.OwnedPed.WeaponHolder.AddRandomWeapons();

                return CommandManager.ProcessCommandResult.Success;
            }
            else if (arguments[0] == "rem_w")
            {
                if (null == player.OwnedPed)
                    return pedNotAliveResult;

                player.OwnedPed.WeaponHolder.RemoveAllWeapons();

                return CommandManager.ProcessCommandResult.Success;
            }
            else if (arguments[0] == "rem_current_w")
            {
                if (null == player.OwnedPed)
                    return pedNotAliveResult;

                var weapon = player.OwnedPed.CurrentWeapon;
                if (weapon != null)
                    Destroy(weapon.gameObject);

                return CommandManager.ProcessCommandResult.Success;
            }
            else if (arguments[0] == "ammo")
            {
                if (null == player.OwnedPed)
                    return pedNotAliveResult;

                foreach (var weapon in player.OwnedPed.WeaponHolder.AllWeapons)
                    WeaponHolder.AddRandomAmmoAmountToWeapon(weapon);

                return CommandManager.ProcessCommandResult.Success;
            }

            return CommandManager.ProcessCommandResult.UnknownCommand;
        }
    }
}
