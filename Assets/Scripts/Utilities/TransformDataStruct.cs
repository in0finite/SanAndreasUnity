using UnityEngine;

namespace SanAndreasUnity.Utilities
{

    public struct TransformDataStruct
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public TransformDataStruct(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public TransformDataStruct(Vector3 position, Quaternion rotation) : this(position, rotation, Vector3.one)
        {
            
        }

        public TransformDataStruct(Vector3 position) : this(position, Quaternion.identity)
        {
            
        }

        public TransformDataStruct(Transform tr) : this(tr.position, tr.rotation, tr.lossyScale)
        {
            
        }

    }

}
