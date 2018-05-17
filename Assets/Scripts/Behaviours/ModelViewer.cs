using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    [ExecuteInEditMode]
    public class ModelViewer : MonoBehaviour
    {
        public enum ModelLoadType
        {
            Custom = 0,
            Ped = 1,
            Weapon = 2,
            Vehicle = 3
        }

        public int modelId = 355;   // ak47
        public string textureDictionaryName = "";
        public ModelLoadType modelLoadType = ModelLoadType.Weapon;
        public Transform targetParent = null;

        public bool load = false;

        private int m_currentModelId = 355;
        private string m_currentTextureDictionaryName = "";
        private ModelLoadType m_modelLoadType = ModelLoadType.Weapon;

        private FrameContainer m_frameContainer = null;

        // Use this for initialization
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
            this.CheckForChanges();
        }

        private void OnValidate()
        {
            this.CheckForChanges();
        }

        private void CheckForChanges()
        {
            if (!load)
                return;

            if (m_modelLoadType != modelLoadType)
            {
                this.Load();
                return;
            }

            if (m_currentModelId != modelId)
            {
                this.Load();
                return;
            }

            if (modelLoadType == ModelLoadType.Custom)
            {
                if (m_currentTextureDictionaryName != textureDictionaryName)
                {
                    this.Load();
                }
            }
        }

        public void Load()
        {
            string modelName = "";
            string textDict = "";
            if (modelLoadType == ModelLoadType.Weapon)
            {
                WeaponDef def = Item.GetDefinition<WeaponDef>(modelId);
                if (null == def)
                    return;
                modelName = def.ModelName;
                textDict = def.TextureDictionaryName;
            }
            else if (modelLoadType == ModelLoadType.Ped)
            {
                PedestrianDef def = Item.GetDefinition<PedestrianDef>(modelId);
                if (null == def)
                    return;
                modelName = def.ModelName;
                textDict = def.TextureDictionaryName;
            }
            else if (modelLoadType == ModelLoadType.Vehicle)
            {
                VehicleDef def = Item.GetDefinition<VehicleDef>(modelId);
                if (null == def)
                    return;
                modelName = def.ModelName;
                textDict = def.TextureDictionaryName;
            }
            else
            {
                return;
            }

            var geoms = Geometry.Load(modelName, textDict);
            if (null == geoms)
                return;

            if (m_frameContainer != null)
            {
                Destroy(m_frameContainer.Root.gameObject);
                Destroy(m_frameContainer);
            }

            Transform tr = null == targetParent ? transform : targetParent;
            m_frameContainer = geoms.AttachFrames(tr, MaterialFlags.Default);

            m_currentModelId = modelId;
            m_currentTextureDictionaryName = textureDictionaryName;
            m_modelLoadType = modelLoadType;
        }
    }
}