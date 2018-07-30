using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;

namespace SanAndreasUnity.Behaviours
{
	
	public partial class Player : MonoBehaviour
	{



		public static Player SpawnPed (PedestrianDef def, Vector3 pos, Quaternion rot)
		{
			CheckPedPrefab ();

			var go = Instantiate (GameManager.Instance.pedPrefab, pos, rot);

			var player = go.GetComponentOrLogError<Player> ();
			player.PlayerModel.StartingPedId = def.Id;

			go.name = "Ped " + def.ModelName;

			return player;
		}

		public static Player SpawnPed (int pedId, Vector3 pos, Quaternion rot)
		{
			var def = Item.GetDefinition<PedestrianDef> (pedId);
			if (null == def)
				throw new System.ArgumentException ("Failed to spawn ped: definition not found by id: " + pedId);
			return SpawnPed (def, pos, rot);
		}

		public static Player SpawnPed (int pedId)
		{
			Vector3 pos;
			Quaternion rot;
			if (GetPositionForPedSpawn (out pos, out rot))
				return SpawnPed (pedId, pos, rot);
			return null;
		}

		public static PedStalker SpawnPedStalker (int pedId, Vector3 pos, Quaternion rot)
		{
			var player = SpawnPed (pedId, pos, rot);
			return player.gameObject.GetOrAddComponent<PedStalker> ();
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

			if (Player.Instance != null) {

				Vector3 offset = Random.onUnitSphere;
				offset.y = 0f;
				offset.Normalize ();
				offset *= Random.Range (5f, 15f);

				pos = Player.Instance.transform.TransformPoint (offset);
				rot = Random.rotation;

				return true;
			}

			return false;
		}

		private static void CheckPedPrefab ()
		{
			if (null == GameManager.Instance)
				throw new System.Exception ("Can't find ped prefab: game manager instance not found");

			if(null == GameManager.Instance.pedPrefab)
				throw new System.Exception ("Ped prefab is null");

		}

	}

}
