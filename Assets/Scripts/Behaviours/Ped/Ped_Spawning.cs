using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using System.Linq;

namespace SanAndreasUnity.Behaviours
{
	
	public partial class Ped
	{

		public static IEnumerable<PedestrianDef> SpawnablePedDefs {
			get {
				return Item.GetDefinitions<PedestrianDef> ().Where (def => def.Id != 0 && def.ModelName != "WMYST"
				           && def.TextureDictionaryName != "generic");
			}
		}


		public static Ped SpawnPed (PedestrianDef def, Vector3 pos, Quaternion rot, bool spawnOnNetwork)
		{
			Net.NetStatus.ThrowIfNotOnServer();

			CheckPedPrefab ();

			var go = Instantiate (PedManager.Instance.pedPrefab, pos, rot);
			go.name = "Ped " + def.ModelName + " " + def.Id;

			var ped = go.GetComponentOrThrow<Ped> ();
			ped.PlayerModel.StartingPedId = def.Id;
			ped.EnterVehicleRadius = PedManager.Instance.AIVehicleEnterDistance;

			if (spawnOnNetwork)
				Net.NetManager.Spawn(go);

			return ped;
		}

		public static Ped SpawnPed (int pedId, Vector3 pos, Quaternion rot, bool spawnOnNetwork)
		{
			var def = Item.GetDefinition<PedestrianDef> (pedId);
			if (null == def)
				throw new System.ArgumentException ("Failed to spawn ped: definition not found by id: " + pedId);
			return SpawnPed (def, pos, rot, spawnOnNetwork);
		}

		public static PedAI SpawnPedAI(int pedId, Vector3 pos, Quaternion rot)
		{
			var ped = SpawnPed(pedId, pos, rot, true);
			return ped.gameObject.GetOrAddComponent<PedAI>();
		}

		public static PedAI SpawnPedAI(int pedId, Transform nearbyTransform)
		{
			if (GetPositionForPedSpawn (out var pos, out var rot, nearbyTransform))
				return SpawnPedAI(pedId, pos, rot);
			return null;
		}

		public static PedAI SpawnPedStalker (int pedId, Vector3 pos, Quaternion rot, Ped targetPed)
		{
			var ped = SpawnPed (pedId, pos, rot, true);

			var pedAI = ped.gameObject.GetOrAddComponent<PedAI> ();
			pedAI.StartFollowing(targetPed);

			return pedAI;
		}

		public static PedAI SpawnPedStalker (int pedId, Transform nearbyTransform, Ped targetPed)
		{
			Vector3 pos;
			Quaternion rot;
			if (GetPositionForPedSpawn (out pos, out rot, nearbyTransform))
				return SpawnPedStalker (pedId, pos, rot, targetPed);
			return null;
		}

		public static bool GetPositionForPedSpawn (out Vector3 pos, out Quaternion rot, Transform nearbyTransform)
		{
			pos = Vector3.zero;
			rot = Quaternion.identity;

			if (nearbyTransform != null) {

				Vector3 offset = Random.onUnitSphere;
				offset.y = 0f;
				offset.Normalize ();
				offset *= Random.Range (5f, 15f);

				pos = nearbyTransform.TransformPoint (offset);
				rot = Random.rotation;

				return true;
			}

			return false;
		}

		private static void CheckPedPrefab ()
		{
			
			if(null == PedManager.Instance.pedPrefab)
				throw new System.Exception ("Ped prefab is null");

		}

	}

}
