using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Net;
using SanAndreasUnity.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Commands
{
    public class BaseCommands : MonoBehaviour
    {
        public static BaseCommands Singleton { get; private set; }

        private class PlayerData
        {
            public List<Vehicle> vehicles = new List<Vehicle>();
        }

        readonly Dictionary<Player, PlayerData> m_perPlayerData = new Dictionary<Player, PlayerData>();

        List<string> m_registeredCommands = new List<string>();
        public IReadOnlyList<string> RegisteredCommands => m_registeredCommands;

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
                new CommandManager.CommandInfo("teleport", "teleport", true, true, this.pedLimitInterval),
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

            m_registeredCommands = commands.Select(c => c.command).ToList();
        }

        private void PlayerOnDisable(Player player)
        {
            m_perPlayerData.Remove(player);
        }

        bool GetPedModelId(string[] arguments, int argumentIndex, out int modelId)
        {
            if (argumentIndex >= arguments.Length)
            {
                modelId = Ped.RandomPedId;
                return true;
            }

            int id = int.Parse(arguments[argumentIndex]);
            bool idExists = Ped.SpawnablePedDefs.Any(def => def.Id == id);
            if (!idExists)
            {
                modelId = 0;
                return false;
            }

            modelId = id;
            return true;
        }

        CommandManager.ProcessCommandResult ProcessCommand(CommandManager.ProcessCommandContext context)
        {
            string[] arguments = CommandManager.SplitCommandIntoArguments(context.command);
            int numArguments = arguments.Length;
            Player player = context.player;
            var pedNotAliveResult = CommandManager.ProcessCommandResult.Error("You must control a ped to run this command");
            var pedModelIdDoesNotExist = CommandManager.ProcessCommandResult.Error("Specified model id does not exist");

            if (arguments[0] == "skin")
            {
                if (null == player.OwnedPed)
                    return pedNotAliveResult;

                if (!GetPedModelId(arguments, 1, out int modelId))
                    return pedModelIdDoesNotExist;

                player.OwnedPed.PlayerModel.Load(modelId);

                return CommandManager.ProcessCommandResult.Success;
            }
            else if (arguments[0] == "stalker")
            {
                if (null == player.OwnedPed)
                    return pedNotAliveResult;

                if (!GetPedModelId(arguments, 1, out int modelId))
                    return pedModelIdDoesNotExist;

                Ped.SpawnPedStalker(modelId, player.OwnedPed.transform, player.OwnedPed);

                return CommandManager.ProcessCommandResult.Success;
            }
            else if (arguments[0] == "suicide")
            {
                if (null == player.OwnedPed)
                    return pedNotAliveResult;

                player.OwnedPed.Kill();

                return CommandManager.ProcessCommandResult.Success;
            }
            else if (arguments[0] == "teleport")
            {
                if (null == player.OwnedPed)
                    return pedNotAliveResult;

                Vector3 position;
                Quaternion rotation;

                try
                {
                    position = CommandManager.ParseVector3(arguments, 1);
                    rotation = player.OwnedPed.transform.rotation;
                    if (numArguments > 4)
                        rotation = Quaternion.Euler(0f, float.Parse(arguments[4], CultureInfo.InvariantCulture), 0f);
                }
                catch
                {
                    return CommandManager.ProcessCommandResult.Error("Invalid syntax. Example: teleport 2000 10.2 -1234.5 or teleport 2000 10.2 -1234.5 28.3");
                }

                player.OwnedPed.Teleport(position, rotation);

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

                var weapon = player.OwnedPed.WeaponHolder.AddWeapon(modelId);
                player.OwnedPed.WeaponHolder.SwitchWeapon(weapon.SlotIndex);
                weapon.AddRandomAmmoAmount();

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
                    weapon.AddRandomAmmoAmount();

                return CommandManager.ProcessCommandResult.Success;
            }

            return CommandManager.ProcessCommandResult.UnknownCommand;
        }
    }
}
