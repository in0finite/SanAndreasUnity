using SanAndreasUnity.Behaviours;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class FogController : MonoBehaviour
{
    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        transform.position = Player.InstancePos;
    }
}