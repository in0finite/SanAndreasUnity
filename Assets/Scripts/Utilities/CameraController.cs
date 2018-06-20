using UnityEngine;

//using UnityEngine.PostProcessing;

[RequireComponent(typeof(Camera))]
//[RequireComponent(typeof(PostProcessingBehaviour))]
//[RequireComponent(typeof(Tonemapping))]
//[RequireComponent(typeof(Bloom))]
public class CameraController : MonoBehaviour
{
    /*public bool highQuality;

    private Camera camera;
    private PostProcessingBehaviour behaviour;
    //private Tonemapping tonemapping;
    //private Bloom bloom;

    // Use this for initialization
    private void Start()
    {
        camera = GetComponent<Camera>();
        behaviour = GetComponent<PostProcessingBehaviour>();
        //tonemapping = GetComponent<Tonemapping>();
        //bloom = GetComponent<Bloom>();

        SetQuality();

        // WIP: When some of this parameters are changed we have to recall SpriteLights.Init again
        SpriteLights.Init(0, 0, Camera.main.fieldOfView, Screen.height);
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
            ToggleQuality();
    }

    private void SetQuality()
    {
        camera.allowHDR = highQuality;
        behaviour.enabled = highQuality;
        //tonemapping.enabled = highQuality;
        //bloom.enabled = highQuality;
    }

    private void ToggleQuality()
    {
        SetQuality();

        highQuality = !highQuality;
    }*/
}