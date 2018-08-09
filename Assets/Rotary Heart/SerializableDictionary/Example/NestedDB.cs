using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using UnityEngine;

namespace RotaryHeart.Lib
{
    [CreateAssetMenu(fileName = "NestedDB.asset", menuName = "Nested DB")]
    public class NestedDB : ScriptableObject
    {
        [SerializeField, ID("id")]
        public MainDict nested;
    }

    [System.Serializable]
    public class Example
    {
        public string id;
        public QueryTriggerInteraction enumVal;
        public NestedDict nestedData;
    }

    [System.Serializable]
    public class NestedExample
    {
        public GameObject prefab;
        public float speed;
        public Color color;
        public Nested2Dict deepNested;
    }

    [System.Serializable]
    public class MainDict : SerializableDictionaryBase<string, Example> { }

    [System.Serializable]
    public class NestedDict : SerializableDictionaryBase<int, NestedExample> { }

    [System.Serializable]
    public class Nested2Dict : SerializableDictionaryBase<QueryTriggerInteraction, string> { }

}
