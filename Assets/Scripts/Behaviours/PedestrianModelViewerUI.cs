using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Importing.Animation;
using UnityEngine;

public class PedestrianModelViewerUI : MonoBehaviour
{
    public PedModel pedestrian = null;

    
    private void OnGUI()
    {
        if (null == pedestrian)
            return;

        
        GUILayout.Label("Current model ID: " + pedestrian.PedestrianId);
        GUILayout.Label("Current model name: " + ((pedestrian.Definition != null) ? pedestrian.Definition.ModelName : "(null!)"));

    }
}