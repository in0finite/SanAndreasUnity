using UnityEngine;

// This is used only for attached MonoBehaviour calls
public class CallerController : MonoBehaviour
{
    private void Awake()
    {
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
}