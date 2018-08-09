using Fclp.Internals.Extensions;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class StarController : MonoBehaviour
{
    private Material sky;
    private new ParticleSystem particleSystem;

    // WIP
    //private static Dictionary<Color, int> weightedColors;

    // Use this for initialization
    private void Start()
    {
        /*weightedColors = new Dictionary<Color, int>()
        {
            { Color.white, 60 },
            { new Color32(190, 210, 255, 255), 10 }, // Light blue
            { new Color32(255, 231, 165, 255), 10 }, // Light orange
            { new Color32(255, 255, 190, 255), 5 }, // Light yellow
            { new Color32(190, 255, 190, 255), 5 }, // Light green
            { new Color32(255, 190, 255, 255), 5 }, // Light purple
            { new Color32(255, 190, 190, 255), 5 }  // Light red
        };*/

        sky = RenderSettings.skybox;
        particleSystem = GetComponent<ParticleSystem>();
        if (WorldController.IsNight && !particleSystem.isPlaying) StartPlaying();
        else StopPlaying();
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
        {
            // maxParticles ... depeding on the zone
            /*int count = particleSystem.main.maxParticles;
            particleSystem.Emit(count);

            ParticleSystem.Particle[] ps = new ParticleSystem.Particle[count];
            particleSystem.GetParticles(ps);
            Dictionary<Color, int> sum = new Dictionary<Color, int>();

            ps.ForEach((x) => {
                x.color = WeightedRandomizer.From(weightedColors).TakeOne();
                if (!sum.ContainsKey(x.startColor))
                    sum.Add(x.startColor, 1);
                else
                    ++sum[x.startColor];
            });

            particleSystem.SetParticles(ps, count);

            sum.ForEach((x) => {
                Debug.LogFormat("Color: {0}; Times: {1}", x.Key, x.Value);
            });*/

            particleSystem.Play();
        }
        //InvokeRepeating("KeepPlaying", 0, particleSystem.main.duration);
    }

    public void KeepPlaying()
    {
        // I keep this, because I can emit as stars as I need
        particleSystem.Play();
    }

    private void StopPlaying()
    {
        if (particleSystem.isPlaying)
        {
            //CancelInvoke();
            particleSystem.Stop();
        }
    }
}