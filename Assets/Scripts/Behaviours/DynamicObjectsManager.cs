using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    public struct DynamicObjectProperties
    {
        int test;
    }

    public class DynamicObjectsManager : MonoBehaviour
    {
        public static DynamicObjectsManager Instance { get; private set; }

        private Dictionary<int, DynamicObjectProperties> m_dynamicObjects = new Dictionary<int, DynamicObjectProperties>
        {
            [1265] = new DynamicObjectProperties(),
            [1230] = new DynamicObjectProperties(),
            [1220] = new DynamicObjectProperties(),
            [1221] = new DynamicObjectProperties(),
        };

        public bool IsModelDynamic(int model)
        {
            return m_dynamicObjects.ContainsKey(model);
        }
        
        public DynamicObjectProperties? GetModelProperties(int model)
        {
            if (m_dynamicObjects.TryGetValue(model, out DynamicObjectProperties value))
                return value;
            return null;
        }

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {

        }
    }

}
