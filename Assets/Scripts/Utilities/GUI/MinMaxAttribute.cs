// NOTE DONT put in an editor folder

using UnityEngine;

public class MinMaxAttribute : PropertyAttribute
{
    public float MinLimit = 0;
    public float MaxLimit = 1;
    public bool ShowEditRange;
    public bool ShowDebugValues;

    public MinMaxAttribute(int min, int max)
    {
        MinLimit = min;
        MaxLimit = max;
    }
}