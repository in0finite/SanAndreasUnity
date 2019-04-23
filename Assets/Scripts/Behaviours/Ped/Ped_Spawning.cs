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


		public static Ped SpawnPed (PedestrianDef def, Vector3 pos, Quaternion rot)
		{
			CheckPedPrefab ();

			var go = Instantiate (PedManager.Instance.pedPrefab, pos, rot);
			go.name = "Ped " + def.ModelName + " " + def.Id;

			var ped = go.GetComponentOrThrow<Ped> ();
			ped.PlayerModel.StartingPedId = def.Id;
			ped.EnterVehicleRadius = PedManager.Instance.AIVehicleEnterDistance;

			var destroyer = ped.gameObject.GetOrAddComponent<OutOfRangeDestroyer> ();
			destroyer.timeUntilDestroyed = PedManager.Instance.AIOutOfRangeTimeout;
			destroyer.range = PedManager.Instance.AIOutOfRangeDistance;

			return ped;
		}

		public static Ped SpawnPed (int pedId, Vector3 pos, Quaternion rot)
		{
			var def = Item.GetDefinition<PedestrianDef> (pedId);
			if (null == def)
				throw new System.ArgumentException ("Failed to spawn ped: definition not found by id: " + pedId);
			return SpawnPed (def, pos, rot);
		}

		public static Ped SpawnPed (int pedId)
		{
			Vector3 pos;
			Quaternion rot;
			if (GetPositionForPedSpawn (out pos, out rot))
				return SpawnPed (pedId, pos, rot);
			return null;
		}

		public static PedStalker SpawnPedStalker (int pedId, Vector3 pos, Quaternion rot)
		{
			var ped = SpawnPed (pedId, pos, rot);

			var stalker = ped.gameObject.GetOrAddComponent<PedStalker> ();
			stalker.stoppingDistance = PedManager.Instance.AIStoppingDistance;

			return stalker;
		}

		public static PedStalker SpawnPedStalker (int pedId)
		{
			Vector3 pos;
			Quaternion rot;
			if (GetPositionForPedSpawn (out pos, out rot))
				return SpawnPedStalker (pedId, pos, rot);
			return null;
		}

		public static bool GetPositionForPedSpawn (out Vector3 pos, out Quaternion rot)
		{
			pos = Vector3.zero;
			rot = Quaternion.identity;

			if (Ped.Instance != null) {

				Vector3 offset = Random.onUnitSphere;
				offset.y = 0f;
				offset.Normalize ();
				offset *= Random.Range (5f, 15f);

				pos = Ped.Instance.transform.TransformPoint (offset);
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
