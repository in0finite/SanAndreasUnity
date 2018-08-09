using System;

using UnityEngine;

// ---------------
//  String => Int
// ---------------
[Serializable]
public class StringIntDictionary : SerializableDictionary<string, int> { }

// ---------------
//  GameObject => Float
// ---------------
[Serializable]
public class GameObjectFloatDictionary : SerializableDictionary<GameObject, float> { }

// ---------------
//  Color => Float
// ---------------
[Serializable]
public class ColorFloatDictionary : SerializableDictionary<Color, float> { }