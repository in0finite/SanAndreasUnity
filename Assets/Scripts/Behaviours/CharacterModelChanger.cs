using SanAndreasUnity.Behaviours;
using UnityEngine;

public class CharacterModelChanger : MonoBehaviour
{
    public KeyCode actionKey = KeyCode.P;

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(actionKey))
        {
            ChangePedestrianModel();
        }
    }

    public static void ChangePedestrianModel()
    {
        if (Player.Instance != null)
        {
			ChangePedestrianModel(Player.Instance.PlayerModel, -1);
        }
    }

    public static void ChangePedestrianModel(PedModel ped, int newModelId)
    {
        
		if (-1 == newModelId)
			newModelId = Player.RandomPedId;

		if (newModelId < Player.kMinPedId || newModelId > Player.kMaxPedId)
        {
            return;
        }

        // Retry with another random model if this one doesn't work
        try
        {
            ped.Load(newModelId);
        }
        catch (System.NullReferenceException ex)
        {
			Debug.LogException (ex);
            ChangePedestrianModel(ped, -1);
        }
    }
}