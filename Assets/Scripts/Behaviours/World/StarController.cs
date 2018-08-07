using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Behaviours.World;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class StarController : MonoBehaviour
{
    private Material sky;
    private new ParticleSystem particleSystem;

    // Use this for initialization
    private void Start()
    {
        sky = RenderSettings.skybox;
        particleSystem = GetComponent<ParticleSystem>();
        if (WorldController.IsNight && (particleSystem.isStopped || particleSystem.isPaused)) particleSystem.Play();
        else if (!WorldController.IsNight && particleSystem.isPlaying) particleSystem.Stop();
    }

    // Update is called once per frame
    private void Update()
    {
        transform.rotation = transform.rotation;
        transform.position = Player.InstancePos;
    }

    public void OnDawnTime()
    {
        StopPlaying();
    }

    public void OnNoonTime()
    {
        StopPlaying();
    }

    public void OnDuskTime()
    {
        StartPlaying();
    }

    public void OnMidnightTime()
    {
        StartPlaying();
    }

    private void StartPlaying()
    {
        if (!particleSystem.isPlaying)
            particleSystem.Play();
    }

    private void StopPlaying()
    {
        if (particleSystem.isPlaying)
            particleSystem.Stop();
    }
}