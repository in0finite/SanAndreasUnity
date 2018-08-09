using System.Collections.Generic;
using UnityEngine;

// Inherit this monoBehaviour if you want to have a button to set your mapColor 
public abstract class ColorController : MonoBehaviour
{
    public ColorFloatDictionary _mapColor = new ColorFloatDictionary();

    [SerializeField]
    public abstract ColorFloatDictionary serializedMapColor { get; set; }

    private void Awake()
    {
        serializedMapColor = new ColorFloatDictionary();
    }
}