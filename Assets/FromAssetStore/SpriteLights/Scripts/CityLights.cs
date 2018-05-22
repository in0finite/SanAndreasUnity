using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class CityLights : MonoBehaviour {

	public Material omnidirectionalCityLightsMat;
	[ColorUsageAttribute(true,true,0f,8f,0.125f,3f)]
	public Color amberColor;
	[ColorUsageAttribute(true,true,0f,8f,0.125f,3f)]
	public Color whiteColor;
	public float spacing = 30f;
	
	/*
	void OnEnable(){

		// Register Map-ity's Loaded Event
		Mapity.MapityLoaded += OnMapityLoaded;
	}

	void OnDisable(){

		// Un-Register Map-ity's Loaded Event
		Mapity.MapityLoaded -= OnMapityLoaded;
	}

	void OnMapityLoaded(){	

		List<SpriteLights.LightData> omnidirectionalLightData = new List<SpriteLights.LightData>();
		Color color;
		float brightness = 1;

		//Loop through all highways and roads.
		foreach(Mapity.Highway highway in Mapity.Singleton.highways.Values){
	
			for(int i = 0; i < highway.wayMapNodes.Count - 1; i++){

				//Get the from-to nodes.
				Mapity.MapNode fromNode = (Mapity.MapNode)highway.wayMapNodes[i];
				Mapity.MapNode toNode = (Mapity.MapNode)highway.wayMapNodes[i+1];

				//Get the road segment start and end point.
				Vector3 from = fromNode.position.world.ToVector();
				Vector3 to = toNode.position.world.ToVector();
				Vector3 fromToVec = to - from;
				float length = fromToVec.magnitude;
				int lightAmount = (int)Mathf.Ceil(length / spacing);
				Vector3 currentPosition = fromNode.position.world.ToVector();

				//Get a translation vector.
				Vector3 offsetVec = Math3d.SetVectorLength(fromToVec, spacing);

				//Give the small roads white lights.
				if((highway.classification == Mapity.HighwayClassification.Residential) || (highway.classification == Mapity.HighwayClassification.Road) || (highway.classification == Mapity.HighwayClassification.Pedestrian) || (highway.classification == Mapity.HighwayClassification.Residential)){

					color = whiteColor;
					brightness = Random.Range(0.7f, 0.9f);
				}

				//Give the big roads amber lights.
				else{

					color = amberColor;
					brightness = Random.Range(0.8f, 1.0f);
				}

				//Place light at a certain interval between the road nodes.
				for(int e = 0; e < lightAmount; e++){

					SpriteLights.LightData data = new SpriteLights.LightData();
					data.frontColor = color;
					data.brightness = brightness;
					data.position = currentPosition;
					omnidirectionalLightData.Add(data);

					currentPosition += offsetVec;
				}

			}	

		}

		GameObject parentObject = new GameObject("CityLights");		 

		GameObject[] lightObjects = SpriteLights.CreateLights("City omnidirectional lights", omnidirectionalLightData.ToArray(), omnidirectionalCityLightsMat);

		//Parenting
		foreach(GameObject lightObject in lightObjects){

			lightObject.transform.parent = parentObject.transform;
		}
	}
	*/
}
