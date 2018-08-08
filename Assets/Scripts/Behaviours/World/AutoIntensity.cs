using SanAndreasUnity.Behaviours;
using UnityEngine;

public class AutoIntensity : MonoBehaviour
{
    public Gradient nightDayColor, nightDaySkyTint;

    public AnimationCurve nightDayAmbientIntensity;
    //public float maxIntensity = 3f;
    //public float minIntensity = 0f;
    public float minPoint = -0.2f;

    //public float maxAmbient = 1f;
    //public float minAmbient = 0f;
    public float minAmbientPoint = -0.2f;

    public Gradient nightDayFogColor;
    public AnimationCurve fogDensityCurve;
    public float fogScale = 1f;

    public float dayAtmosphereThickness = 0.4f;
    public float nightAtmosphereThickness = 0.87f;

    public Vector3 dayRotateSpeed;
    public Vector3 nightRotateSpeed;

    private float skySpeed = 1;

    private Light mainLight;
    private Skybox sky;
    private Material skyMat;

    private void Start()
    {
        mainLight = GetComponent<Light>();
        skyMat = RenderSettings.skybox;
    }

    private void Update()
    {
        transform.position = Player.InstancePos;

        float tRange = 1 - minPoint;
        float dot = Mathf.Clamp01((Vector3.Dot(mainLight.transform.forward, Vector3.down) - minPoint) / tRange);

        tRange = 1 - minAmbientPoint;
        dot = Mathf.Clamp01((Vector3.Dot(mainLight.transform.forward, Vector3.down) - minAmbientPoint) / tRange);

        RenderSettings.ambientIntensity = nightDayAmbientIntensity.Evaluate(dot);

        mainLight.color = nightDayColor.Evaluate(dot);
        RenderSettings.ambientLight = mainLight.color;

        RenderSettings.fogColor = nightDayFogColor.Evaluate(dot);
        RenderSettings.fogDensity = fogDensityCurve.Evaluate(dot) * fogScale;

        float i = ((dayAtmosphereThickness - nightAtmosphereThickness) * dot) + nightAtmosphereThickness;
        skyMat.SetFloat("_AtmosphereThickness", i);
        EnviromentController.SetSkyColor(nightDaySkyTint.Evaluate(dot));

        if (dot > 0)
            transform.Rotate(dayRotateSpeed * Time.deltaTime * skySpeed);
        else
            transform.Rotate(nightRotateSpeed * Time.deltaTime * skySpeed);

        //if (Input.GetKeyDown(KeyCode.Q)) skySpeed *= 0.5f;
        //if (Input.GetKeyDown(KeyCode.E)) skySpeed *= 2f;
    }
}