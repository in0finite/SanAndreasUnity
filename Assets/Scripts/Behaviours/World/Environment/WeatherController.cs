using System.Collections.Generic;
using UnityEngine;

public class WeatherController : ColorController
{
    public override ColorFloatDictionary serializedMapColor { get; set; }

    // Use this for initialization
    private void Start()
    {
        if (!Application.isPlaying) return;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!Application.isPlaying) return;
    }
}