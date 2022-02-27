using SanAndreasUnity.Importing.Items.Placements;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.World
{
    public class WaterFaceInfo : MonoBehaviour
    {
        public WaterFace WaterFace { get; set; }

        private void OnDrawGizmosSelected()
        {
            if (null == this.WaterFace)
                return;

            Gizmos.color = Color.green;

            foreach (var vertex in this.WaterFace.Vertices)
            {
                Gizmos.DrawWireSphere(vertex.Position, 2f);
            }
        }
    }
}
