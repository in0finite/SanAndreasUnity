using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SanAndreasUnity.Behaviours
{
    [Serializable]
    public struct NameParticleSystem
    {
        public string name;
        public ParticleSystem particleSystem;
    }

    public class ParticleSystemManager : MonoBehaviour
    {
        public NameParticleSystem[] effects;
        public static ParticleSystemManager Instance { get; private set; }

        public ParticleSystem GetByNane(string name)
        {
            ParticleSystem particleSystem = effects.First((e) => e.name == name).particleSystem;
            ParticleSystem psInstance = Instantiate(particleSystem);
            psInstance.name = "PS " + name;
            return psInstance;
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
