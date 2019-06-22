using UnityEngine;

namespace SanAndreasUnity.Stats
{
    public class WorldStats : MonoBehaviour
    {
        
        void Start()
        {
            Utilities.Stats.RegisterStat(new Utilities.Stats.Entry(){category = "WORLD", onGUI = OnStatGUI});
        }

        void OnStatGUI()
        {

            if (Behaviours.World.Cell.Instance != null)
            {
				Behaviours.World.Cell.Instance.showWindow (0);
			}
            else
            {
                GUILayout.Label("World not loaded");
            }

        }

    }
}
