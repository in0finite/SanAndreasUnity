using UnityEngine;
using System.Collections;
using NOcean;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public abstract class WaveRenderer : MonoBehaviour
{
    public Shader shader = null;
    public Shader shaderWriter = null;

    protected static RenderTexture trailmap = null;
    protected static RenderTexture wavemap = null;
    
    public const string trailCameraName = "TrailCamera";

    public Transform trailer = null;

    public const int resolution = 512;
    public const float interval = 0.033f;

    protected Material matWave = null;
    protected static Camera trailCamera = null;

    [Range(20, 100)]
    public float worldsize = 50f;

    public bool debug = false;

    protected Vector3 prevPos = Vector3.zero;

    protected Camera m_camera = null;

    protected CommandBuffer cmdBuffer = null;

    protected virtual void PhysicsUpdate()
    {
        Debug.LogWarning("PhysicsUpdate No Impl");
    }

    float time = 0;
    void LateUpdate()
    {
        if(Time.time - time > interval)
        {
            PhysicsUpdate();
            time = Time.time;
        }
    }
}
