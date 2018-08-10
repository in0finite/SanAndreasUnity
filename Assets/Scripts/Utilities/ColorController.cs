using System.Collections.Generic;
using UnityEngine;

// Inherit this monoBehaviour if you want to have a button to set your mapColor 
public abstract class ColorController : MonoBehaviour
{
    public ColorFloatDictionary _mapColor = new ColorFloatDictionary();

    [SerializeField]
    public ColorFloatDictionary serializedMapColor
    {
        get
        {
            return _mapColor;
        }
        set
        {
            _mapColor = value;
        }
    }

    public ColorFloatDictionary GetMap()
    {
        return _mapColor;
    }
}