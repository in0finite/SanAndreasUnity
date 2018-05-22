using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LlamaSoftware
{
    [RequireComponent(typeof(Light))]
    public class LightLOD : MonoBehaviour
    {
        private new Light light;

        [SerializeField]
        [Tooltip("For static (uncontrolled by script) lights, this should be true. If you have interactable lights, you should also adjust this with light.enabed")]
        public bool LightShouldBeOn = true;

        [SerializeField]
        [Tooltip("The lower you set this, the faster the light will respond to player locations, and the higher the CPU usage")]
        [Range(0, 1f)]
        private float UpdateDelay = 0.1f;

        //Replace this with your "Player" object
        private FreeCamera LocalPlayer;

        [SerializeField]
        public List<LODAdjustment> ShadowQualityLods;

        [SerializeField]
        [Tooltip("For Debugging - If you check this, the light color will be changed to the debug color defined on each LOD quality")]
        private bool ShowLightColorAsDebugColor;

        [SerializeField]
        [Tooltip("For Debugging - displays how far player is from the light source")]
        private float DistanceFromPlayer;

        [SerializeField]
        [Tooltip("For Debugging - displays if the Light's Shadow Resolution is clamped to Quality Settings")]
        private bool IsClamped;

        [SerializeField]
        private int LOD;

        private Color CurrentDebugColor;
        private LightShadows DesiredLightShadowQuality;

        private void Start()
        {
            light = GetComponent<Light>();
            DesiredLightShadowQuality = light.shadows;

            //Replace this with your "Player" object
            LocalPlayer = GameObject.FindObjectOfType<FreeCamera>();

            StartCoroutine("AdjustLODQuality");
        }

#if UNITY_EDITOR

        private void Update()
        {
            if (LocalPlayer != null)
            {
                CurrentDebugColor = ShadowQualityLods[LOD].DebugColor;

                Debug.DrawLine(this.transform.position, LocalPlayer.transform.position, CurrentDebugColor);
            }
        }

#endif

        private IEnumerator AdjustLODQuality()
        {
            float delay = UpdateDelay + UnityEngine.Random.value / 20f; //this randomization is to prevent all lights updating at the same time causing frame spikes
            int i = 0;
            int DesiredQuality;
            LODAdjustment ClampedLOD;

            while (true)
            {
                if (LightShouldBeOn)
                {
                    DistanceFromPlayer = Vector3.Distance(this.transform.position, LocalPlayer.transform.position);
                    for (i = 0; i < ShadowQualityLods.Count; i++)
                    {
                        if ((DistanceFromPlayer > ShadowQualityLods[i].DistanceRange.x && DistanceFromPlayer <= ShadowQualityLods[i].DistanceRange.y) || i == ShadowQualityLods.Count - 1)
                        {
                            LOD = i;
                            if (ShadowQualityLods[i].CastNoShadows)
                            {
                                light.shadows = LightShadows.None;
                            }
                            else
                            {
                                light.shadows = DesiredLightShadowQuality;
                                light.enabled = true;
                                //respect quality settings, do not go higher than what they have defined.
                                if (QualitySettings.shadowResolution <= ShadowQualityLods[i].ShadowResolution)
                                {
                                    IsClamped = true;

                                    DesiredQuality = (int)QualitySettings.shadowResolution;
                                    light.shadowResolution = (LightShadowResolution)DesiredQuality;

                                    if (ShowLightColorAsDebugColor)
                                    {
                                        ClampedLOD = FindMatchingShadowQualityIndex(QualitySettings.shadowResolution);
                                        if (ClampedLOD == null)
                                        {
                                            Debug.LogWarning("Shadow Resolution is clamped to: " + QualitySettings.shadowResolution.ToString() + ", but no Light LOD step matches this quality!");
                                        }
                                        else
                                        {
                                            light.color = ClampedLOD.DebugColor;
                                        }
                                    }
                                }
                                else
                                {
                                    IsClamped = false;

                                    DesiredQuality = (int)ShadowQualityLods[i].ShadowResolution;
                                    light.shadowResolution = (LightShadowResolution)DesiredQuality;

                                    if (ShowLightColorAsDebugColor)
                                    {
                                        light.color = ShadowQualityLods[i].DebugColor;
                                    }
                                }
                            }

                            break;
                        }
                    }
                }
                else
                {
                    light.enabled = false;
                    LOD = 0;
                }

                yield return new WaitForSeconds(delay);
            }
        }

        private LODAdjustment FindMatchingShadowQualityIndex(ShadowResolution Quality)
        {
            return ShadowQualityLods.Find((lod) => lod.ShadowResolution.Equals(Quality));
        }

        [Serializable]
        public class LODAdjustment
        {
            public Vector2 DistanceRange;
            public ShadowResolution ShadowResolution;
            public bool CastNoShadows;
            public Color DebugColor;
        }
    }
}