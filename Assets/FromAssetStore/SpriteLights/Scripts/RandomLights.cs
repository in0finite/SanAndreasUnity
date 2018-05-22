using UnityEngine;
using System.Collections;

public class RandomLights : MonoBehaviour {


	public Material material;
	public int lightAmount = 10000;
	[Range(-1.0f, 1.0f)]
	public float globalBrightnessOffset = 0.0f;

	void Start () {

		//Randomize the random function.
		Random.seed = Mathf.RoundToInt(System.DateTime.Now.Millisecond); 
	
		//Generate a square of random lights.
		GenerateRandomLights(lightAmount, new Vector2(0, 300), material); 
	}
	

	//Generates random lights in the specified area.
	private void GenerateRandomLights(int amount, Vector2 area, Material material){

		SpriteLights.LightData[] lightData = new SpriteLights.LightData[amount];

		for(int i = 0; i < amount; i++){

			lightData[i] = new SpriteLights.LightData();
			lightData[i].position = new Vector3(Random.Range(area.x, area.y), 0, Random.Range(area.x, area.y));
		}

		SpriteLights.CreateLights("RandomLights", lightData, material);
	}
}
