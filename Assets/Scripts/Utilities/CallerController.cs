using UnityEngine;

// This is used only for attached MonoBehaviour calls
public class CallerController : MonoBehaviour
{
    public static CallerController Instance;

    private void Awake()
    {
        Instance = this;

        ZHelpers.Awake();
    }

    // Use this for initialization
    private void Start()
    {
        ZHelpers.Start();
    }

    // Update is called once per frame
    private void Update()
    {
        ZHelpers.Update();
    }

    private void OnGUI()
    {
        ZHelpers.OnGUI();
    }

    private void OnApplicationQuit()
    {
        ZHelpers.OnApplicationQuit();
    }
}