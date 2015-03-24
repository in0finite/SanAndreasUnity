using UnityEngine;
using System.Collections;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Conversion;

namespace SanAndreasUnity
{
    public class Test : MonoBehaviour
    {
        void Start()
        {
            ResourceManager.LoadArchive(ResourceManager.GetPath(ResourceManager.ModelsDir, "gta3.img"));

            this.LoadMesh("sw_bit_15.dff");
        }
    }
}
