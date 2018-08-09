using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

namespace RotaryHeart.Lib
{
    [CreateAssetMenu(fileName = "DataBase.asset", menuName = "Data Base")]
    public class DataBaseExample : ScriptableObject
    {
        public enum EnumExample
        {
            None,
            Value1,
            Value2,
            Value3,
            Value4,
            Value5,
            Value6
        }

        [System.Serializable]
        public class ChildTest
        {
            public Color myChildColor;
            public bool myChildBool;
            public Gradient test;
        }

        [System.Serializable]
        public class ClassTest
        {
            public string id;
            public float test;
            public string test2;
            public Quaternion quat;
            public ChildTest[] childTest;
        }

        [System.Serializable]
        public class ArrayTest
        {
            public int[] myArray;
        }

        [System.Serializable]
        public class AdvancedGenericClass
        {
            [Range(0, 100)]
            public float value;

            public bool Equals(AdvancedGenericClass other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return other.value == value;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof(AdvancedGenericClass)) return false;
                return Equals((AdvancedGenericClass)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return value.GetHashCode();
                }
            }
        }

        [SerializeField]
        public Generic_String _genericString;

        [SerializeField]
        public Generic_Generic _genericGeneric;

        [SerializeField, ID("id")]
        private S_GenericDictionary _stringGeneric;

        [SerializeField]
        private I_GenericDictionary _intGeneric;

        [SerializeField]
        private I_GO _intGameobject;
        [SerializeField]
        private GO_I _gameobjectInt;

        [SerializeField]
        private S_GO _stringGameobject;
        [SerializeField]
        private GO_S _gameobjectString;

        [SerializeField]
        private S_Mat _stringMaterial;
        [SerializeField]
        private Mat_S _materialString;

        [SerializeField]
        private V3_Q _vector3Quaternion;
        [SerializeField]
        private Q_V3 _quaternionVector3;

        [SerializeField]
        private S_AC _stringAudioClip;
        [SerializeField]
        private AC_S _audioClipString;

        [SerializeField]
        private C_Int _charInt;
        [SerializeField]
        private G_Int _gradientInt;

        [SerializeField]
        private Int_IntArray _intArray;

        [SerializeField]
        private Enum_String _enumString;

        [SerializeField, DrawKeyAsProperty]
        private AdvanGeneric_String _advancedGenericKey;

        [System.Serializable]
        public class Generic_String : SerializableDictionaryBase<ClassTest, string> { }

        [System.Serializable]
        public class Generic_Generic : SerializableDictionaryBase<ClassTest, ClassTest> { }

        [System.Serializable]
        public class C_Int : SerializableDictionaryBase<char, int> { }
        [System.Serializable]
        public class G_Int : SerializableDictionaryBase<Gradient, int> { }

        [System.Serializable]
        public class I_GO : SerializableDictionaryBase<int, GameObject> { }
        [System.Serializable]
        public class GO_I : SerializableDictionaryBase<GameObject, int> { }

        [System.Serializable]
        public class S_GO : SerializableDictionaryBase<string, GameObject> { }
        [System.Serializable]
        public class GO_S : SerializableDictionaryBase<GameObject, string> { }

        [System.Serializable]
        public class S_Mat : SerializableDictionaryBase<string, Material> { }
        [System.Serializable]
        public class Mat_S : SerializableDictionaryBase<Material, string> { }

        [System.Serializable]
        public class S_AC : SerializableDictionaryBase<string, AudioClip> { }
        [System.Serializable]
        public class AC_S : SerializableDictionaryBase<AudioClip, string> { }

        [System.Serializable]
        public class S_Sprite : SerializableDictionaryBase<string, Sprite> { }

        [System.Serializable]
        public class V3_Q : SerializableDictionaryBase<Vector3, Quaternion> { }
        [System.Serializable]
        public class Q_V3 : SerializableDictionaryBase<Quaternion, Vector3> { }

        [System.Serializable]
        public class S_GenericDictionary : SerializableDictionaryBase<string, ClassTest> { }

        [System.Serializable]
        public class I_GenericDictionary : SerializableDictionaryBase<int, ClassTest> { }

        [System.Serializable]
        public class Int_IntArray : SerializableDictionaryBase<int, ArrayTest> { }

        [System.Serializable]
        public class Enum_String : SerializableDictionaryBase<EnumExample, string> { };

        [System.Serializable]
        public class AdvanGeneric_String : SerializableDictionaryBase<AdvancedGenericClass, string> { };

    }
}