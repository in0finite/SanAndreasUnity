using UnityEngine;
using System.Collections;

public class LazyClouds : MonoBehaviour {

	public float LS_CloudTimeScale = 2;
	public float LS_CloudScale = 4;
	public float LS_CloudScattering = 0.6f;
	public float LS_CloudIntensity = 4;
	public float LS_CloudSharpness = 0.75f;
	public float LS_CloudThickness = 1.0f;
	public float LS_ShadowScale = 0.75f;
	public float LS_DistScale = 10.0f;
	public Vector3 LS_CloudColor = new Vector3(1,0.9f,0.95f);

	void Start () {
	
	
	}
	
	void Update () {
	
		
	
		Shader.SetGlobalFloat("ls_time", Time.time*LS_CloudTimeScale*0.25f);
		Shader.SetGlobalFloat("ls_cloudscale", LS_CloudScale);
		Shader.SetGlobalFloat("ls_cloudscattering", LS_CloudScattering);
		Shader.SetGlobalFloat("ls_cloudintensity", LS_CloudIntensity);
		Shader.SetGlobalFloat("ls_cloudsharpness", LS_CloudSharpness);
		Shader.SetGlobalFloat("ls_shadowscale", LS_ShadowScale);
		Shader.SetGlobalFloat("ls_cloudthickness", LS_CloudThickness);
		Shader.SetGlobalVector("ls_cloudcolor", LS_CloudColor);
		Shader.SetGlobalFloat("ls_distScale", LS_DistScale);
	}
}
