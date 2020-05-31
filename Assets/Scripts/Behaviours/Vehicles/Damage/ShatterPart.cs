using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    [RequireComponent(typeof(Renderer))]
    [DisallowMultipleComponent]
    [AddComponentMenu("RVP/Damage/Shatter Part", 2)]

    //Class for parts that shatter
    public class ShatterPart : MonoBehaviour
    {
        [System.NonSerialized]
        public Renderer rend;

        [System.NonSerialized]
        public bool shattered;

        public float breakForce = 5;

        [Tooltip("Transform used for maintaining seams when deformed after shattering")]
        public Transform seamKeeper;

        [System.NonSerialized]
        public Material initialMat;

        public Material brokenMaterial;
        public ParticleSystem shatterParticles;
        public AudioSource shatterSnd;

        private void Start()
        {
            rend = GetComponent<Renderer>();
            if (rend)
            {
                initialMat = rend.sharedMaterial;
            }
        }

        public void Shatter()
        {
            if (!shattered)
            {
                shattered = true;

                if (shatterParticles)
                {
                    shatterParticles.Play();
                }

                if (brokenMaterial)
                {
                    rend.sharedMaterial = brokenMaterial;
                }
                else
                {
                    rend.enabled = false;
                }

                if (shatterSnd)
                {
                    shatterSnd.Play();
                }
            }
        }
    }
}