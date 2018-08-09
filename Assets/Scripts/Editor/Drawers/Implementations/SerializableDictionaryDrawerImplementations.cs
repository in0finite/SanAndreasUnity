using UnityEngine;
using UnityEngine.UI;

using UnityEditor;

// ---------------
//  String => Int
// ---------------
[UnityEditor.CustomPropertyDrawer(typeof(StringIntDictionary))]
public class StringIntDictionaryDrawer : SerializableDictionaryDrawer<string, int>
{
    protected override SerializableKeyValueTemplate<string, int> GetTemplate()
    {
        return GetGenericTemplate<SerializableStringIntTemplate>();
    }
}
internal class SerializableStringIntTemplate : SerializableKeyValueTemplate<string, int> { }

// ---------------
//  GameObject => Float
// ---------------
[UnityEditor.CustomPropertyDrawer(typeof(GameObjectFloatDictionary))]
public class GameObjectFloatDictionaryDrawer : SerializableDictionaryDrawer<GameObject, float>
{
    protected override SerializableKeyValueTemplate<GameObject, float> GetTemplate()
    {
        return GetGenericTemplate<SerializableGameObjectFloatTemplate>();
    }
}
internal class SerializableGameObjectFloatTemplate : SerializableKeyValueTemplate<GameObject, float> { }

// ---------------
//  Color => Float
// ---------------
[UnityEditor.CustomPropertyDrawer(typeof(ColorFloatDictionary))]
public class ColorFloatDictionaryDrawer : SerializableDictionaryDrawer<Color, float>
{
    protected override SerializableKeyValueTemplate<Color, float> GetTemplate()
    {
        return GetGenericTemplate<SerializableColorFloatTemplate>();
    }
}
internal class SerializableColorFloatTemplate : SerializableKeyValueTemplate<Color, float> { }