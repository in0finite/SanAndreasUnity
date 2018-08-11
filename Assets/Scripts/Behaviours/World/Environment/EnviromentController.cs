using UnityEngine;

public class EnviromentController
{
    public static void SetSkyColor(Color color)
    {
        if (RenderSettings.skybox.HasProperty("_Tint"))
            RenderSettings.skybox.SetColor("_Tint", color);
        else if (RenderSettings.skybox.HasProperty("_SkyTint"))
            RenderSettings.skybox.SetColor("_SkyTint", color);
    }
}