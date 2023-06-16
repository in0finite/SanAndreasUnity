using UnityEngine;
using System.Collections;
using NOcean;
using System.Collections.Generic;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class WaterWaveRenderer : WaveRenderer
{
    [Range(0, 1f)]
    public float stopWhenStay = 0.5f;

    public static WaterWaveRenderer instance = null;
    private Material matWater = null;

    private Material matWriter = null;

    private Renderer[] rds = null;

    private List<Material> matTrans = new List<Material>();

    private NeoNormalGrid ng = null;

    void Awake()
    {
        instance = this;

        m_camera = Camera.main;
    }

    private void RefreshRenderers()
    {
        matTrans.Clear();

        if (trailer == null)
            return;

        rds = trailer.GetComponentsInChildren<Renderer>();

        foreach (Renderer rd in rds)
        {
            if (rd == null)
                continue;

            for (int i = 0; i < rd.sharedMaterials.Length; i++)
            {
                Material mat = rd.sharedMaterials[i];
                if (mat == null)
                    continue;

                matTrans.Add(mat);
            }
        }
    }

    void OnEnable()
    {
        instance = this;

        RefreshRenderers();
    }

    // Use this for initialization
    public void Start()
    {
        if (matWave == null)
        {
            matWave = new Material(shader);
        }

        if (matWater == null)
        {
            ng = GetComponent<NeoNormalGrid>();
            if (ng != null)
                matWater = ng.oceanMaterial;
        }

        if (trailCamera == null)
        {
            GameObject go = GameObject.Find(trailCameraName);

            if (!go)
            {
                go = new GameObject(trailCameraName, typeof(Camera));
                if(NeoOcean.instance != null)
                   go.transform.parent = NeoOcean.instance.transform;
            }
            if (!go.GetComponent(typeof(Camera)))
                go.AddComponent(typeof(Camera));
            trailCamera = go.GetComponent<Camera>();

            trailCamera.backgroundColor = Color.black;
            trailCamera.clearFlags = CameraClearFlags.SolidColor;
            trailCamera.renderingPath = RenderingPath.Forward;
            trailCamera.allowHDR = false;
            trailCamera.allowMSAA = false;
            trailCamera.cullingMask = 0;
            trailCamera.enabled = false;
            trailCamera.transform.parent = this.transform.parent;

            go.hideFlags = HideFlags.HideAndDontSave;
        }

        if (wavemap == null || !wavemap.IsCreated())
        {
            wavemap = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBHalf);
            wavemap.filterMode = FilterMode.Bilinear;
            wavemap.useMipMap = false;
            wavemap.wrapMode = TextureWrapMode.Repeat;
            wavemap.Create();
#if UNITY_EDITOR
            wavemap.hideFlags = HideFlags.DontSave;
#endif
        }
        wavemap.DiscardContents();
        // to clear color
        RenderTexture.active = wavemap;
        GL.Clear(true, true, Color.clear);

        if (trailmap == null || !trailmap.IsCreated())
        {
            trailmap = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.RHalf);
            trailmap.filterMode = FilterMode.Bilinear;
            trailmap.useMipMap = true;
            trailmap.wrapMode = TextureWrapMode.Clamp;
            trailmap.Create();
#if UNITY_EDITOR
            trailmap.hideFlags = HideFlags.DontSave;
#endif
        }

        trailmap.DiscardContents();

        RenderTexture.active = trailmap;
        GL.Clear(true, true, Color.clear);

        if (trailer == null)
            return;

        prevPos = trailer.position;

        RefreshRenderers();
    }

    public void ForceClear()
    {
        if (wavemap == null)
            return;

        Graphics.SetRenderTarget(wavemap);

        GL.Clear(true, true, Color.clear);
    }

    public void OnDisable()
    {
        ForceClear();

        if (matWater)
            matWater.DisableKeyword("_WATERWAVE_ON");
    }

    void OnDestroy()
    {
        if (RenderTexture.active == wavemap)
            RenderTexture.active = null;

        if (wavemap != null)
            wavemap.Release();
        Object.Destroy(wavemap);
        wavemap = null;

        if (trailmap != null)
            trailmap.Release();
        DestroyImmediate(trailmap);
        trailmap = null;

        Object.Destroy(matWave);

        if(cmdBuffer != null)
        {
            cmdBuffer.Release();
            cmdBuffer = null;
        }
    }

#if UNITY_EDITOR
    public void OnGUI()
    {
        if(debug)
        {
            if (wavemap != null)
                GUI.DrawTexture(new Rect(0, 0, wavemap.width / 2, wavemap.height / 2), wavemap, ScaleMode.ScaleToFit, false);

        }
    }
#endif

    [Range(0.1f, 1f)]
    public static float c = 1f;
    [Range(0.01f, 0.02f)]
    public float t = 0.02f;
    [Range(0, 0.1f)]
    public float mu = 0.1f;
    [Range(0, 1f)]
    public float intensity = 1f;

    [Range(0, 1f)]
    public float height = 1f;

    [Range(0f, 0.2f)]
    public float intervalTimeMax = 0.2f;

    [Range(10f, 100f)]
    public float fade = 10f;

    [Range(0.9f, 0.995f)]
    public float decay = 0.995f;

    private float pluseTime = 0f;

    public static void Blit(RenderTexture src, RenderTexture dst, Material mat)
    {
        if (dst != null)
            dst.DiscardContents();
        else
            return;

        if (mat != null)
            Graphics.Blit(src, dst, mat);
        else
            Graphics.Blit(src, dst);

    }

    public static void Blit(RenderTexture src, RenderTexture dst, Material mat, int pass)
    {
        if (dst != null)
            dst.DiscardContents();
        else
            return;

        if (mat != null)
            Graphics.Blit(src, dst, mat, pass);
        else
            Graphics.Blit(src, dst);
    }

    public static void ComputeBlit(RenderTexture dest, Material mat, int pass)
    {
        Graphics.SetRenderTarget(dest);

        GL.Clear(true, true, Color.clear);

        GL.PushMatrix();
        GL.LoadOrtho();

        mat.SetPass(pass);

        GL.Begin(GL.QUADS);
        GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(0.0f, 0.0f, 0.1f);
        GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1.0f, 0.0f, 0.1f);
        GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1.0f, 1.0f, 0.1f);
        GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(0.0f, 1.0f, 0.1f);
        GL.End();

        GL.PopMatrix();
    }

    // Update is called once per frame
    protected override void PhysicsUpdate()
    {
        if (trailer == null || m_camera == null || (!m_camera.gameObject.activeSelf))
        {
            ForceClear();
            return;
        }

        if (wavemap == null || !wavemap.IsCreated())
        {
            return;
        }

        if (ng == null)
            return;

        float useWorldSize = worldsize;

        //trailCamera.enabled = true;
        // render in project space
        trailCamera.orthographicSize = useWorldSize * 0.5f;
        trailCamera.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
        trailCamera.transform.position = new Vector3(trailer.position.x, trailer.position.y + useWorldSize * 0.5f, trailer.position.z);

        trailCamera.nearClipPlane = m_camera.nearClipPlane;
        trailCamera.farClipPlane = useWorldSize;
        trailCamera.orthographic = true;
        trailCamera.aspect = 1;
        trailCamera.depthTextureMode = DepthTextureMode.None;
        trailCamera.backgroundColor = Color.black;
        trailCamera.clearFlags = CameraClearFlags.SolidColor;
        trailCamera.cullingMask = 1 << trailer.gameObject.layer;
        trailCamera.renderingPath = RenderingPath.Forward;
        trailCamera.allowHDR = false;
        trailCamera.allowMSAA = false;
        trailCamera.targetTexture = trailmap;

        if (cmdBuffer == null)
        {
            cmdBuffer = new CommandBuffer();
            cmdBuffer.name = "TrailRenderBuffer";
        }

        cmdBuffer.Clear();

        {
            trailCamera.enabled = false;

            cmdBuffer.SetRenderTarget(trailmap);
            cmdBuffer.ClearRenderTarget(true, true, Color.black);

            if (matWriter == null && shaderWriter != null)
                matWriter = new Material(shaderWriter);

            cmdBuffer.SetViewport(new Rect(0f, 0f, trailmap.width, trailmap.height));
            cmdBuffer.SetViewProjectionMatrices(trailCamera.worldToCameraMatrix, trailCamera.projectionMatrix);

#if UNITY_EDITOR
            RefreshRenderers();
#endif

            //render
            foreach (Renderer rd in rds)
            {
                if (rd == null)
                    continue;

                for (int i = 0; i < rd.sharedMaterials.Length; i++)
                {
                    Material mat = rd.sharedMaterials[i];
                    if (mat == null)
                        continue;
                    
                    if (matTrans.Contains(mat))
                    {
                        cmdBuffer.DrawRenderer(rd, matWriter, i, 0);
                    }
                }
            }

            Graphics.ExecuteCommandBuffer(cmdBuffer);
        }

        //trailCamera.RenderWithShader(shaderWriter, useRenderType ? "RenderType" : "WaterWrite");

        {
            RenderTexture rt = RenderTexture.GetTemporary(wavemap.width, wavemap.height, 0, wavemap.format);
            rt.wrapMode = TextureWrapMode.Repeat;
            rt.filterMode = FilterMode.Bilinear;
            if(prevPos == Vector3.zero)
            {
                prevPos = trailer.position;
            }
            Vector3 offsetV3 = prevPos - trailer.position;
            matWave.SetVector("_WaveOffset", new Vector4(offsetV3.x, offsetV3.z, 0, 0)  / (useWorldSize));

            matWave.SetTexture("_WaveTex", wavemap);
            Blit(wavemap, rt, matWave, 0);

            matWave.SetTexture("_WaveTex", rt);

            float d = useWorldSize / wavemap.width;

            c = Mathf.Clamp(c, 0, d * Mathf.Sqrt(mu * t + 2) / (2*t));
            float maxt = (mu + Mathf.Sqrt(mu * mu + 32 * c * c / (d * d))) / (8 * c * c / (d * d));
            float t1 = Mathf.Clamp(t, 0, maxt * 0.99f);

            float halfworldsize = useWorldSize * 0.5f;

            float f1 = c * c * t1 * t1 / (d * d);
            float f2 = 1.0F / (mu * t1 + 2);
            float k1 = (4.0F - 8.0F * f1) * f2;
            float k2 = (mu * t1 - 2) * f2;
            float k3 = 2.0F * f1 * f2;
            matWave.SetVector("_WaveParam", new Vector4(1f / wavemap.width, k1, k2, k3));
            matWave.SetTexture("_WaveTex_Make", trailmap);
            if (pluseTime < Time.time)
            {
                float vel = (offsetV3.x * offsetV3.x + offsetV3.z * offsetV3.z);
                pluseTime = Time.time + ((vel > Mathf.Epsilon) ? intervalTimeMax : 0);
                float inta = intensity * Mathf.Max(vel, stopWhenStay) / Mathf.Max(Time.smoothDeltaTime,interval);
                inta = Mathf.Clamp01(inta);
                matWave.SetVector("_WaveMake", new Vector4(inta, height, 1f / d, fade));
            }
            else
                matWave.SetVector("_WaveMake", new Vector4(0, height, 1f / d, fade));

            matWave.SetFloat("_WaveDecay", decay);
 
            ComputeBlit(wavemap, matWave, 1);
            RenderTexture.ReleaseTemporary(rt);

            if (matWater)
            {
                matWater.SetTexture("_WaveTex", wavemap);
                matWater.SetVector("_WaveCoord", new Vector4(trailer.position.x - halfworldsize,
                trailer.position.z - halfworldsize, 1f / useWorldSize, 1f / useWorldSize));
                matWater.EnableKeyword("_WATERWAVE_ON");
            }
        }

        prevPos = trailer.position;
    }
    
}
