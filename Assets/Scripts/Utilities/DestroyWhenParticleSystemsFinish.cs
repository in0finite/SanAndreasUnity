using System.Collections;
using System.Linq;
using UnityEngine;

namespace SanAndreasUnity.Utilities
{

    public class DestroyWhenParticleSystemsFinish : MonoBehaviour
    {

        IEnumerator Start()
        {
            var systems = this.GetComponentsInChildren<ParticleSystem>();

            while (true)
            {
                yield return null;

                if (systems.All(s => !s.isPlaying))
                {
                    Object.Destroy(this.gameObject);
                    break;
                }
            }

        }

    }

}
