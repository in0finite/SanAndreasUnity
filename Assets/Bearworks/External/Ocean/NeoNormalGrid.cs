using UnityEngine;
using UnityEngine.Rendering.Universal;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NOcean
{


    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [ExecuteInEditMode]
    public class NeoNormalGrid : MonoBehaviour
    {
        public Material oceanMaterial = null;

        void Start()
        {
            Init();
        }

        protected virtual void OnEnable()
        {
            GetComponent<Renderer>().enabled = true;
        }

        protected virtual void OnDisable()
        {
            GetComponent<Renderer>().enabled = false;
        }


        protected virtual void Init()
        {
            if (NeoOcean.instance == null)
                return;

            GetComponent<Renderer>().sharedMaterial = oceanMaterial;
            GetComponent<Renderer>().enabled = true;

            NeoOcean.instance.AddPG(this);
        }

        protected virtual void OnDestroy()
        {
            GetComponent<Renderer>().enabled = false;

            if (NeoOcean.instance != null)
                NeoOcean.instance.RemovePG(this);

        }

        //public bool willRender = false;

        public virtual void LateUpdate()
        {
            NeoOcean.oceanheight = this.transform.position.y;

#if UNITY_EDITOR
            NeoOcean.instance.AddPG(this);
#endif

            oceanMaterial.DisableKeyword("_PROJECTED_ON");
        }

        // Update is called once per frame
        public void SetupMaterial(RenderTexture rt, RenderTexture rt2, float scale)
        {
            Renderer rd = GetComponent<Renderer>();

            if (rd == null)
            {
                return;
            }

            if (oceanMaterial == null)
            {
                if(rd.sharedMaterial != null)
                    oceanMaterial = rd.sharedMaterial;
                else
                    return;
            }

            oceanMaterial.SetFloat("_InvNeoScale", scale);

            oceanMaterial.SetTexture("_Map0", rt);
            oceanMaterial.SetTexture("_Map1", rt2);

#if UNITY_EDITOR
            rd.hideFlags = HideFlags.HideInInspector;
#endif
            rd.sharedMaterial = oceanMaterial;
        }


    }
}