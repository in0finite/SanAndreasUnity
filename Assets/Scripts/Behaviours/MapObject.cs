using SanAndreasUnity.Importing.Items;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    public class MapObject : MonoBehaviour
    {
        protected Cell Cell { get; private set; }
        protected Instance Instance { get; private set; }
        protected Importing.Items.Object Object { get; private set; }

        public void Initialize(Cell cell, Instance inst)
        {
            Cell = cell;
            Instance = inst;
            Object = cell.GameData.GetObject(Instance.ObjectId);

            transform.localPosition = inst.Position;
            transform.localRotation = inst.Rotation;

            name = Object != null ? Object.Geometry : string.Format("Unknown ({0})", Instance.ObjectId);
        }
    }
}
