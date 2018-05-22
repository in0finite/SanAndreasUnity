using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;
using System.IO;


/*

Notes: 
-The svg files must have exactly the same file name as in the names used in the LightingType class.
-The colors of the circles in the svg file is not the real light color but it represents the material and light type instead.

Abbreviations:
-HIRL - High Intensity Runway Light system
-MALSR - Medium intensity Approach Light System with Runway alignment indicator lights
-TDZ/CL - runway Touchdown Zone and Centerline Lighting system
-ALSF 1 - high intensity Approach Light System with Sequenced Flashing lights, system length 2,400 to 3,000 feet
-ALSF 2 - high intensity Approach Light System with Sequenced Flashing lights and red side row lights the last 1,000 feet, system length 2,400 to 3,000 feet
-SALS/SALSF - Short Approach Lighting System, high intensity (same as inner 1,500 feet of ALSF 1)
-SSALF - Simplified Short Approach Lighting system with sequenced Flashing lights and runway alignment indicator lights, system length 2,400 to 3,000 feet
-MALD/MASLF - Medium intensity Approach Lighting, with and without Sequenced Flashing lights, system length 1,400 feet
-ODALS - OmniDirectional Approach Lighting System with sequenced flashing lights, system length 1,400 feet
-RAIL - Runway Alignment Indicator Lighted sequence flashing lights (which are only installed in combination with other light systems)
-REIL - Runway End Identifier Lights (threshold strobes)
-LDIN - sequenced flashing LeaD-IN lights
-VASI - Visual Approach Slope Indicator
-PAPI - Precision Approach Path Indicator

-Layout examples:
https://www.tc.gc.ca/eng/civilaviation/publications/tp312-chapter5-5-3-931.htm
*/


public class RunwayLights : MonoBehaviour {

	[ColorUsageAttribute(true,true,0f,8f,0.125f,3f)]
	public Color greenLight;
	[ColorUsageAttribute(true,true,0f,8f,0.125f,3f)]
	public Color redLight;
	[ColorUsageAttribute(true,true,0f,8f,0.125f,3f)]
	public Color whiteLight;
	[ColorUsageAttribute(true,true,0f,8f,0.125f,3f)]
	public Color amberLight;

	[Range(-1.0f, 1.0f)]
	public float globalBrightnessOffset = 0.0f;

	public Material directionalLightsMat;
	public Material omnidirectionalRunwayLightsMat;
	public Material papiLightsMat;
	public Material strobeLightsMat;

	private float thresholdLightsSpacing = 2f;
	private string whiteDirectionalSinglesideCString = "ffc700";
	private string redDirectionalSinglesideCString = "ff0200";
	private string strobeCString = "00ffff";

	private float strobeTimeStep = 0;
	private float FOV = 60f;
	private float screenHeight = 600f;

	public void Start(){

		//Randomize the random function.
		Random.seed = Mathf.RoundToInt(System.DateTime.Now.Millisecond); 

		//Fetch the camera parameters.
		//TODO: make this work for VR.
		FOV = Camera.main.fieldOfView;
		screenHeight = Screen.height;

		CreateRunwayLights();
	}

	public void Update () {

		//For debugging purposes, the global brightness offset might be changed. This is not normally done each frame.
		SpriteLights.Init(strobeTimeStep, globalBrightnessOffset, FOV, screenHeight);

		/*
		//Create runway lights at runtime.
		if(Input.GetKeyDown("space")){

			CreateRunwayLights();
		}
		*/
	}

	//This is used to store the data which is read from an svg file.
	public class SvgData{

		public Vector3 position = Vector3.zero;
		public string materialString = "";
		public int id = 0;
	}

	//All runway approach light types.
	private enum LightingType{

		NONE,
		ALSF1, //Approach Lighting System with Sequenced Flashing Lights configuration 1.
		ALSF2, //Approach Lighting System with Sequenced Flashing Lights configuration 2.
		CALVERT1, //or ICAO-1 HIALS: ICAO-compliant configuration 1 High Intensity Approach Lighting System.
		CALVERT2,//or ICAO-2 HIALS: ICAO-compliant configuration 2 High Intensity Approach Lighting System.
		MALS, //medium intensity approach lighting system.
		SALS, //Short Approach Lighting System.
		SSALS, //Simplified Short Approach Lighting System.
		MALSF, //Medium-intensity Approach Lighting System with Sequenced Flashing lights.
		MALSR, //Medium-intensity Approach Lighting System with Runway Alignment Indicator Lights.
		SSALF, //Simplified Short Approach Lighting System with Sequenced Flashing Lights.
		SSALR, //Simplified Short Approach Lighting System with Runway Alignment Indicator Lights.
		TDZ //Touch Down Zone lights.
	};

	//Type of strobe.
	private enum StrobeType{

		NONE,
		REIL, //Runway End Identifier Lights, Edge light strobes.
		ODALS, //Omnidirectional Approach Lighting System, Centerline walking strobes.
		BOTH
	};

	//Type of threshold wing bar.
	private enum ThresholdWingbar{

		NONE,
		SMALL,
		LARGE
	}

	//Type of PAPI light.
	private enum PapiType{

		NONE,
		PAPILEFT,
		PAPIRIGHT,
		PAPIBOTH,
		VASILEFT,
		VASIRIGHT,
		VASIBOTH
	};

	//Fetch and cache all svg based approach light data.
	private class ApproachLightData{

		//The y scale is different than the x scale in the svg file for better readability.
		private float yScaleFactor = 10f;

		private string materialPropertyString = ";fill:#";

		public SvgData[] ALSF1;
		public SvgData[] ALSF2;
		public SvgData[] CALVERT1;
		public SvgData[] CALVERT2;
		public SvgData[] MALS;
		public SvgData[] SALS;
		public SvgData[] SSALS;
		public SvgData[] MALSF;
		public SvgData[] MALSR;
		public SvgData[] SSALF;
		public SvgData[] SSALR;
		public SvgData[] TDZ;

		//The coordinates in the SVG file are different then the visual presentation.
		private Vector3 ConvertCoordinates(float x, float y, Vector2 viewBox){

			float yFactored = (viewBox.y - y) * yScaleFactor;			
			return new Vector3(x, 0, yFactored);
		}
		
		//Get the test out of an svg file.
    	private string getSvgText(string fileName){

			string path;
			string svgText = "";

			#if UNITY_EDITOR
				path = Application.dataPath + "/Resources/" + fileName;
			#else
				path = Application.dataPath +"/" + fileName;
			#endif

#if !UNITY_WEBPLAYER
			if(System.IO.File.Exists(path)){

				svgText = System.IO.File.ReadAllText(path);
			}
#endif

			if(string.IsNullOrEmpty(svgText)){

				string name = Path.GetFileNameWithoutExtension(fileName);
				TextAsset txt = Resources.Load(name) as TextAsset;

				if(txt != null){

					svgText = txt.text;
				}
			}

			return svgText;
    	}

		//Extract the light positions from an svg file.
		private void GetLightArray(out SvgData[] svgData, string svgText){

			if(!string.IsNullOrEmpty(svgText)){

				Vector2 viewBox = Vector2.one;
				XmlDocument xmlDoc = new XmlDocument();		
		
				xmlDoc.LoadXml(svgText);

				//Get the circle nodes.
				XmlNodeList circles = xmlDoc.GetElementsByTagName("circle");		
				XmlNodeList svgNodes = xmlDoc.GetElementsByTagName("svg");

				//Get the view box.
				string viewBoxString = svgNodes[0].Attributes["viewBox"].Value;
				string[] boxSize = viewBoxString.Split(' ');
				viewBox = new Vector2(float.Parse(boxSize[2]), float.Parse(boxSize[3]));

				svgData = new SvgData[circles.Count];

				for(int i = 0; i < circles.Count; i++){

					//Get coordinates.
					float x = float.Parse(circles[i].Attributes["cx"].Value);
					float y = float.Parse(circles[i].Attributes["cy"].Value);

					//Get the id.
					string idString = circles[i].Attributes["id"].Value;
					string numberString = Regex.Match(idString, @"\d+").Value;
					int id = int.Parse(numberString);

					//Get the material string.
					string style = circles[i].Attributes["style"].Value;
					int start = style.IndexOf(materialPropertyString) + materialPropertyString.Length;
					int end = style.IndexOf(';', start);
					string materialString = style.Substring(start, end - start);

					//Store the data we are interested in.
					svgData[i] = new SvgData();
					svgData[i].materialString = materialString;
					svgData[i].position = ConvertCoordinates(x, y, viewBox);	
					svgData[i].id = id;
				}
			}

			else{

				svgData = null;
			}
		}

		public void StoreApproachLightData(){

			//Get all svg files.
			//string svgFileNames = "ALSF1.svg ALSF2.svg CALVERT1.svg CALVERT2.svg MALS.svg SALS.svg SSALS.svg MALSF.svg MALSR.svg SSALF.svg SSALR.svg TDZ.svg";
			//string svgFileNames = "ALSF1 ALSF2 CALVERT1 CALVERT2 MALS SALS SSALS MALSF MALSR SSALF SSALR TDZ";
			string svgFileNames = "ALSF1.txt ALSF2.txt CALVERT1.txt CALVERT2.txt MALS.txt SALS.txt SSALS.txt MALSF.txt MALSR.txt SSALF.txt SSALR.txt TDZ.txt";
		
			SvgData[] alsData;
			string svgText;

			string[] fileNameArray = svgFileNames.Split(' ');

			//Loop through all svg files.
			for(int i = 0; i < fileNameArray.Length; i++){

				//Get the svg file name.
				string fileNameWithExtension = fileNameArray[i];
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileNameWithExtension);

				//Get the text in the svg file.
				svgText = getSvgText(fileNameWithExtension);
				GetLightArray(out alsData, svgText);

				//Route the data to the correct variable.
				switch(fileNameWithoutExtension){
					case "ALSF1":
						ALSF1 = alsData;
					break;

					case "ALSF2":
						ALSF2 = alsData;
					break;

					case "CALVERT1":
						CALVERT1 = alsData;
					break;

					case "CALVERT2":
						CALVERT2 = alsData;
					break;

					case "MALS":
						MALS = alsData;
					break;

					case "SALS":
						SALS = alsData;
					break;

					case "SSALS":
						SSALS = alsData;
					break;

					case "MALSF":
						MALSF = alsData;
					break;

					case "MALSR":
						MALSR = alsData;
					break;

					case "SSALF":
						SSALF = alsData;
					break;

					case "SSALR":
						SSALR = alsData;
					break;

					case "TDZ":
						TDZ = alsData;
					break;

					default:
					break;
				}
			}
		}

		//Get the approach light data.
		public SvgData[] GetApproachLightData(LightingType lightingType){

			switch(lightingType){
				case LightingType.ALSF1:
					return ALSF1;

				case LightingType.ALSF2:
					return ALSF2;

				case LightingType.CALVERT1:
					return CALVERT1;

				case LightingType.CALVERT2:
					return CALVERT2;

				case LightingType.MALS:
					return MALS;

				case LightingType.SALS:
					return SALS;

				case LightingType.SSALS:
					return SSALS;

				case LightingType.MALSF:
					return MALSF;

				case LightingType.MALSR:
					return MALSR;

				case LightingType.SSALF:
					return SSALF;

				case LightingType.SSALR:
					return SSALR;

				case LightingType.TDZ:
					return TDZ;
			}

			return null;
		}
	}


	//This class holds all parameter relating to the type of runway and lighting. 
	private class RunwayData{

		private int strobeAmountInRow = 22;
		private float strobeTimeStep;
		
		public RunwayData(){

			this.strobeTimeStep = 1.0f / strobeAmountInRow;
		}

		public int GetAmountInRow(){

			return strobeAmountInRow;
		}

		public float GetTimeStep(){

			return strobeTimeStep;
		}

		public void SetThresholdPosition(Vector3 position, float lengthIn){

			//Set the length
			length = lengthIn;

			//Set the first threshold position.
			thresholdPosition[0] = position;

			//Set the other side threshold position.
			Vector3 forwardOffsetDir = Math3d.GetForwardVector(rotation[0]);	
			Vector3 thresholdAtoBVec = Math3d.SetVectorLength(forwardOffsetDir, length);		
			thresholdPosition[1] = thresholdPosition[0] + thresholdAtoBVec;
		}

		public void SetThresholdRotation(float angle){

			rotation[0] = Quaternion.Euler(0, angle, 0);
			rotation[1] = Quaternion.Euler(0, angle - 180f, 0);
		}

		public Vector3[] thresholdPosition = new Vector3[2]; //The position is relative to the Reference Position.
		public float[] thresholdHeight = new float[2];
		public float midHeight;
		public Quaternion[] rotation = new Quaternion[2];
		public float width;
		public float length;
		public LightingType[] lightingType = new LightingType[2];
		public StrobeType[] strobeType = new StrobeType[2];
		public PapiType[] papiType = new PapiType[2];
		public bool centerlineLights;
		public float centerlineLightsSpacing;
		public bool edgeLights;
		public float edgeLightsSpacing;
		public ThresholdWingbar[] thresholdWingbar = new ThresholdWingbar[2];
		public bool[] TDZ = new bool[2];
		public float lightAngle = 3f;
	}


	//Procedurally crate the runway lights.
	public void CreateRunwayLights(){

		int index;

		//Create an array which holds every possible runway.
		RunwayData[] runways = new RunwayData[2];
		bool randomBrightness = false;
		float brightness = 0.8f; //0.6f
		float smallerLightSize = 0.7f;  //0.8f

		index = 0;
		runways[index] = new RunwayData();
		runways[index].SetThresholdPosition(new Vector3(0, 0, 0), 3000);
		runways[index].thresholdHeight[0] = 0;
		runways[index].thresholdHeight[1] = 0;
		runways[index].midHeight = 0;
		runways[index].SetThresholdRotation(0);
		runways[index].width = 45;
		runways[index].lightingType[0] = LightingType.ALSF2;
		runways[index].lightingType[1] = LightingType.ALSF2;
		runways[index].strobeType[0] = StrobeType.ODALS;
		runways[index].strobeType[1] = StrobeType.BOTH;
		runways[index].papiType[0] = PapiType.PAPIBOTH;
		runways[index].papiType[1] = PapiType.PAPIBOTH;
		runways[index].centerlineLights = true;
		runways[index].centerlineLightsSpacing = 15;
		runways[index].edgeLights = true;
		runways[index].edgeLightsSpacing = 60;
		runways[index].thresholdWingbar[0] = ThresholdWingbar.LARGE;
		runways[index].thresholdWingbar[1] = ThresholdWingbar.LARGE;
		runways[index].TDZ[0] = true;
		runways[index].TDZ[1] = true;

		index = 1;
		runways[index] = new RunwayData();
		runways[index].SetThresholdPosition(new Vector3(200, 0, 0), 3000);
		runways[index].thresholdHeight[0] = 0;
		runways[index].thresholdHeight[1] = 0;
		runways[index].midHeight = 0;
		runways[index].SetThresholdRotation(0);
		runways[index].width = 45;
		runways[index].lightingType[0] = LightingType.ALSF1;
		runways[index].lightingType[1] = LightingType.ALSF1;
		runways[index].strobeType[0] = StrobeType.BOTH;
		runways[index].strobeType[1] = StrobeType.BOTH;
		runways[index].papiType[0] = PapiType.PAPILEFT;
		runways[index].papiType[1] = PapiType.PAPILEFT;
		runways[index].centerlineLights = true;
		runways[index].centerlineLightsSpacing = 15;
		runways[index].edgeLights = true;
		runways[index].edgeLightsSpacing = 60;
		runways[index].thresholdWingbar[0] = ThresholdWingbar.LARGE;
		runways[index].thresholdWingbar[1] = ThresholdWingbar.LARGE;
		runways[index].TDZ[0] = true;
		runways[index].TDZ[1] = true;

		/*
		index = 2;
		runways[index] = new RunwayData();
		runways[index].SetThresholdPosition(new Vector3(400, 0, 0), 2500);
		runways[index].thresholdHeight[0] = 0;
		runways[index].thresholdHeight[1] = 0;
		runways[index].midHeight = 0;
		runways[index].SetThresholdRotation(0);
		runways[index].width = 40;
		runways[index].lightingType[0] = LightingType.SALS;
		runways[index].lightingType[1] = LightingType.SALS;
		runways[index].strobeType[0] = StrobeType.BOTH;
		runways[index].strobeType[1] = StrobeType.BOTH;
		runways[index].papiType[0] = PapiType.PAPILEFT;
		runways[index].papiType[1] = PapiType.PAPILEFT;
		runways[index].centerlineLights = false;
		runways[index].centerlineLightsSpacing = 15;
		runways[index].edgeLights = true;
		runways[index].edgeLightsSpacing = 60;
		runways[index].thresholdWingbar[0] = ThresholdWingbar.SMALL;
		runways[index].thresholdWingbar[1] = ThresholdWingbar.SMALL;
		runways[index].TDZ[0] = false;
		runways[index].TDZ[1] = false;

		index = 3;
		runways[index] = new RunwayData();
		runways[index].SetThresholdPosition(new Vector3(600, 0, 0), 3000);
		runways[index].thresholdHeight[0] = 0;
		runways[index].thresholdHeight[1] = 0;
		runways[index].midHeight = 0;
		runways[index].SetThresholdRotation(0);
		runways[index].width = 45;
		runways[index].lightingType[0] = LightingType.CALVERT2;
		runways[index].lightingType[1] = LightingType.CALVERT2;
		runways[index].strobeType[0] = StrobeType.NONE;
		runways[index].strobeType[1] = StrobeType.NONE;
		runways[index].papiType[0] = PapiType.PAPIBOTH;
		runways[index].papiType[1] = PapiType.PAPIBOTH;
		runways[index].centerlineLights = true;
		runways[index].centerlineLightsSpacing = 15;
		runways[index].edgeLights = true;
		runways[index].edgeLightsSpacing = 60;
		runways[index].thresholdWingbar[0] = ThresholdWingbar.LARGE;
		runways[index].thresholdWingbar[1] = ThresholdWingbar.LARGE;
		runways[index].TDZ[0] = true;
		runways[index].TDZ[1] = true;

		index = 4;
		runways[index] = new RunwayData();
		runways[index].SetThresholdPosition(new Vector3(800, 0, 0), 3000);
		runways[index].thresholdHeight[0] = 0;
		runways[index].thresholdHeight[1] = 0;
		runways[index].midHeight = 0;
		runways[index].SetThresholdRotation(0);
		runways[index].width = 45;
		runways[index].lightingType[0] = LightingType.CALVERT1;
		runways[index].lightingType[1] = LightingType.CALVERT1;
		runways[index].strobeType[0] = StrobeType.NONE;
		runways[index].strobeType[1] = StrobeType.NONE;
		runways[index].papiType[0] = PapiType.PAPIRIGHT;
		runways[index].papiType[1] = PapiType.PAPIRIGHT;
		runways[index].centerlineLights = true;
		runways[index].centerlineLightsSpacing = 15;
		runways[index].edgeLights = true;
		runways[index].edgeLightsSpacing = 60;
		runways[index].thresholdWingbar[0] = ThresholdWingbar.LARGE;
		runways[index].thresholdWingbar[1] = ThresholdWingbar.LARGE;
		runways[index].TDZ[0] = true;
		runways[index].TDZ[1] = true;
		*/

		//Get the strobe timestep variable.
		strobeTimeStep = InitRunwayLights(runways);

		//Create temporary lists to store the light data,.
		List<SpriteLights.LightData> directionalLightData = new List<SpriteLights.LightData>();
		List<SpriteLights.LightData> omnidirectionalLightData = new List<SpriteLights.LightData>();
		List<SpriteLights.LightData> strobeLightData = new List<SpriteLights.LightData>();
		List<SpriteLights.LightData> papiLightData = new List<SpriteLights.LightData>();

		ApproachLightData allApproachLightData = new ApproachLightData();		

		//Get the position and color of the lights from an svg file.
		allApproachLightData.StoreApproachLightData();

		//Loop through all runways.
		for(int i = 0; i < runways.Length; i++){
			
			//Create the light data and store in a temporary buffer.
			SetupApproachLights(ref directionalLightData, allApproachLightData, runways[i], randomBrightness, brightness, 1f);
			SetupTDZLights(ref directionalLightData, allApproachLightData, runways[i], randomBrightness, brightness, smallerLightSize);
			SetupStrobeLights(ref strobeLightData, allApproachLightData, runways[i], strobeTimeStep);
			SetupThresholdLights(ref directionalLightData, runways[i], false, 0.6f, 1f);
			SetupCenterlineLights(ref directionalLightData, runways[i], randomBrightness, brightness, smallerLightSize);
			SetupEdgeLights(ref omnidirectionalLightData, runways[i], randomBrightness, brightness, 1f);
			SetupPapiLights(ref papiLightData, runways[i], false);
		}

		//Create a parent object.
		GameObject parentObject = new GameObject("RunwayLights");

		//Take all the light data lists and create the actual ligth mesh from it.
		GameObject[] lightObjects = SpriteLights.CreateLights("Runway directional lights", directionalLightData.ToArray(), directionalLightsMat);
		MakeChild(parentObject, lightObjects);

		lightObjects = SpriteLights.CreateLights("Runway omnidirectional lights", omnidirectionalLightData.ToArray(), omnidirectionalRunwayLightsMat);
		MakeChild(parentObject, lightObjects);

		lightObjects = SpriteLights.CreateLights("Runway strobe lights", strobeLightData.ToArray(), strobeLightsMat);
		MakeChild(parentObject, lightObjects);

		lightObjects = SpriteLights.CreateLights("Runway PAPI lights", papiLightData.ToArray(), papiLightsMat);
		MakeChild(parentObject, lightObjects);

		parentObject.transform.position = new Vector3(-2941, 0, 10936);
		parentObject.transform.rotation = Quaternion.Euler(0, -111.5163f, 0);
	}

	//Make the game objects children of the parent.
	private void MakeChild(GameObject parent, GameObject[] children){

		foreach(GameObject child in children){

			child.transform.parent = parent.transform;
		}
	}

	//Setup all the shader variables.
	private float InitRunwayLights(RunwayData[] runways){

		float strobeTimeStep = runways[0].GetTimeStep();

		SpriteLights.Init(strobeTimeStep, globalBrightnessOffset, FOV, screenHeight);

		return strobeTimeStep;
	}

	//Setup all the strobe lights.
	private void SetupStrobeLights(ref List<SpriteLights.LightData> lightData, ApproachLightData allApproachLightData, RunwayData runwayData, float strobeTimeStep){

		float groupID = Random.Range(0.0f, 0.99f); 

		for(int side = 0; side < 2; side++){

			if(runwayData.lightingType[side] != LightingType.NONE){

				SvgData[] svgData = allApproachLightData.GetApproachLightData(runwayData.lightingType[side]);

				if((runwayData.strobeType[side] == StrobeType.ODALS) || (runwayData.strobeType[side] == StrobeType.BOTH)){

					for(int i = 0; i < svgData.Length; i++){

						SpriteLights.LightData data = new SpriteLights.LightData();
						Vector3 position = (runwayData.rotation[side] * svgData[i].position) + runwayData.thresholdPosition[side];

						if(svgData[i].materialString == strobeCString){				

							data.position = position;

							//Set the direction and upward rotation of the light.
							data.rotation = runwayData.rotation[side] * Quaternion.Euler(Vector3.right * runwayData.lightAngle);

							data.strobeID = svgData[i].id * strobeTimeStep;
							data.strobeGroupID = groupID;
							lightData.Add(data);
						}			
					}
				}

				if((runwayData.strobeType[side] == StrobeType.REIL) || (runwayData.strobeType[side] == StrobeType.BOTH)){

					float distanceOffset = 0;
					Vector3 sideOffsetDir = Math3d.GetRightVector(runwayData.rotation[side]);	

					//If threshold wing bars are used, the REIL distance must be bigger, otherwise they overlap.
					if(runwayData.thresholdWingbar[side] == ThresholdWingbar.LARGE){

						distanceOffset = 15.5f;
					}

					if((runwayData.thresholdWingbar[side] == ThresholdWingbar.SMALL) || (runwayData.thresholdWingbar[side] == ThresholdWingbar.NONE)){

						distanceOffset = 12f;
					}					

					//Right strobe.
					Vector3 startOffsetAVec = Math3d.SetVectorLength(sideOffsetDir, (runwayData.width * 0.5f) + distanceOffset);
					Vector3 position = runwayData.thresholdPosition[side] + startOffsetAVec;
					SpriteLights.LightData data = new SpriteLights.LightData();

					data.position = position;

					//Set the direction and upward rotation of the light.
					data.rotation = runwayData.rotation[side] * Quaternion.Euler(Vector3.right * runwayData.lightAngle);

					//The strobe ID is 0, so it will flash at the same time as all other strobes with ID 0.
					//The group ID is the same as the walking strobe, so it is synchronized with that.
					data.strobeID = 0;
					data.strobeGroupID = groupID;

					lightData.Add(data);

					//Left strobe.
					position = runwayData.thresholdPosition[side] - startOffsetAVec;
					data = new SpriteLights.LightData();

					data.position = position;

					//Set the direction and upward rotation of the light.
					data.rotation = runwayData.rotation[side] * Quaternion.Euler(Vector3.right * runwayData.lightAngle);

					data.strobeGroupID = groupID;
					lightData.Add(data);
				}
			}
		}
	}

	//Setup all the approach lights.
	private void SetupApproachLights(ref List<SpriteLights.LightData> lightData, ApproachLightData allApproachLightData, RunwayData runwayData, bool randomBrightness, float brightness, float size){

		for(int side = 0; side < 2; side++){

			if(runwayData.lightingType[side] != LightingType.NONE){

				SvgData[] svgData = allApproachLightData.GetApproachLightData(runwayData.lightingType[side]);

				//Set approach lights.
				for(int i = 0; i < svgData.Length; i++){

					bool useLight = false;

					SpriteLights.LightData data = new SpriteLights.LightData();
					Vector3 position = (runwayData.rotation[side] * svgData[i].position) + runwayData.thresholdPosition[side];

					if(svgData[i].materialString == whiteDirectionalSinglesideCString){

						data.frontColor = whiteLight;
						data.brightness = SetBrightness(randomBrightness, brightness);
						useLight = true;
					}

					if(svgData[i].materialString == redDirectionalSinglesideCString){

						data.frontColor = redLight;
						data.brightness = SetBrightness(randomBrightness, brightness);
						useLight = true;
					}		

					if(useLight){

						data.position = position;

						//Set the direction and upward rotation of the light.
						data.rotation = runwayData.rotation[side] * Quaternion.Euler(Vector3.right * runwayData.lightAngle);
						data.size = size;

						lightData.Add(data);
					}
				}
			}
		}
	}

	//Setup all the touch down zone lights.
	private void SetupTDZLights(ref List<SpriteLights.LightData> lightData, ApproachLightData allApproachLightData, RunwayData runwayData, bool randomBrightness, float brightness, float size){

		for(int side = 0; side < 2; side++){

			if(runwayData.lightingType[side] != LightingType.NONE){

				SvgData[] svgData = allApproachLightData.GetApproachLightData(runwayData.lightingType[side]);

				//Set TDZ lights.
				if(runwayData.TDZ[side]){

					svgData = allApproachLightData.GetApproachLightData(LightingType.TDZ);

					for(int i = 0; i < svgData.Length; i++){

						if(svgData[i].materialString == whiteDirectionalSinglesideCString){

							SpriteLights.LightData data = new SpriteLights.LightData();
							Vector3 position = (runwayData.rotation[side] * svgData[i].position) + runwayData.thresholdPosition[side];

							data.frontColor = whiteLight;
							data.brightness = SetBrightness(randomBrightness, brightness);
							data.position = position;

							//Set the direction and upward rotation of the light.
							data.rotation = runwayData.rotation[side] * Quaternion.Euler(Vector3.right * runwayData.lightAngle);
							data.size = size;

							lightData.Add(data);
						}
					}
				}
			}
		}
	}

	//Setup all the threshold lights.
	private void SetupThresholdLights(ref List<SpriteLights.LightData> lightData, RunwayData runwayData, bool randomBrightness, float brightness, float size){

		for(int side = 0; side < 2; side++){

			//Calculate the amount of lights.
			int lightAmountInRow = (int)Mathf.Ceil(runwayData.width / thresholdLightsSpacing);

			//Calculate the width of the threshold light row.
			float rowWidth = (lightAmountInRow - 1) * thresholdLightsSpacing;

			//Set start position.
			Vector3 sideOffsetDir = Math3d.GetRightVector(runwayData.rotation[side]);		
			Vector3 startOffsetAVec = Math3d.SetVectorLength(sideOffsetDir, rowWidth * 0.5f);
			Vector3 startPosition = runwayData.thresholdPosition[side] + startOffsetAVec;
			Vector3 thresholdACorner = startPosition;
			Vector3 currentPosition = startPosition;

			Vector3 sideOffsetVec = Math3d.SetVectorLength(-sideOffsetDir, thresholdLightsSpacing);

			SpriteLights.LightData data;

			//Create lights.
			for(int i = 0; i < lightAmountInRow; i++){

				data = new SpriteLights.LightData();

				data.position = currentPosition;
				data.rotation = runwayData.rotation[side];
				data.frontColor = greenLight;
				data.brightness = SetBrightness(randomBrightness, brightness);
				data.backColor = redLight;
				data.size = size;

				lightData.Add(data);

				currentPosition += sideOffsetVec;
			}

			//Create wing bars.
			if(runwayData.thresholdWingbar[side] != ThresholdWingbar.NONE){

				int lightAmount = 0;

				if(runwayData.thresholdWingbar[side] == ThresholdWingbar.LARGE){

					lightAmount = 9;
				}

				if(runwayData.thresholdWingbar[side] == ThresholdWingbar.SMALL){

					lightAmount = 4;
				}

				Vector3 barSideOffsetVec = Math3d.SetVectorLength(sideOffsetDir, 1.5f);

				//Set start position.	
				currentPosition = thresholdACorner;

				//Create wing bars.
				for(int i = 0; i < lightAmount * 2; i++){

					data = new SpriteLights.LightData();

					//Set the new start position.
					if(i == lightAmount){

						currentPosition = runwayData.thresholdPosition[side] - startOffsetAVec;
						barSideOffsetVec *= -1;
					}

					currentPosition += barSideOffsetVec;
					data.position = currentPosition;
					data.rotation = runwayData.rotation[side];
					data.frontColor = greenLight;
					data.brightness = SetBrightness(randomBrightness, brightness);
					data.size = size;

					lightData.Add(data);
				}
			}
		}
	}

	//Setup all the centerline lights.
	private void SetupCenterlineLights(ref List<SpriteLights.LightData> lightData, RunwayData runwayData, bool randomBrightness, float brightness, float size){

		if(runwayData.centerlineLights){

			//Calculate the amount of lights.
			int lightAmountInRow = (int)Mathf.Floor(runwayData.length / runwayData.centerlineLightsSpacing);

			//Calculate the offset direction.
			Vector3 lengthOffsetDir = Math3d.GetForwardVector(runwayData.rotation[0]);

			//Calculate the start position.
			float rowLength = (lightAmountInRow - 1) * runwayData.centerlineLightsSpacing;
			float edgeOffset = (runwayData.length - rowLength) * 0.5f;

			Vector3 startOffsetVec = Math3d.SetVectorLength(lengthOffsetDir, edgeOffset);
			Vector3 startPosition = runwayData.thresholdPosition[0] + startOffsetVec;
			Vector3 currentPosition = startPosition;

			Vector3 lengthOffsetVec = Math3d.SetVectorLength(lengthOffsetDir, runwayData.centerlineLightsSpacing);

			//Calculate the point where the lights should change color.
			float redA = 300f + runwayData.centerlineLightsSpacing;		
			float redB = (runwayData.length - (300f + runwayData.centerlineLightsSpacing));
			float alternateA = 900f + runwayData.centerlineLightsSpacing;
			float alternateB = (runwayData.length - (900f + runwayData.centerlineLightsSpacing));

			for(int i = 0; i < lightAmountInRow; i++){

				bool passedRedA = false;
				bool passedAlternateA = false;
				bool passedRedB = false;
				bool passedAlternateB = false;

				SpriteLights.LightData data = new SpriteLights.LightData();

				//Is the current light index even or odd?
				bool even = IsEven(i);

				data.position = currentPosition;
				data.rotation = runwayData.rotation[0];

				//Calculate the distance to the threshold.
				float currentDistance = (i * runwayData.centerlineLightsSpacing) + edgeOffset;

				//The last 900 meter of the runway has alternating red and white lights.
				if(currentDistance <= alternateA){

					passedAlternateA = true;
				}

				//The last 900 meter of the runway has alternating red and white lights.
				if(currentDistance >= alternateB){

					passedAlternateB = true;
				}

				//The last 300 meter of the runway has red centerline lights.
				if(currentDistance <= redA){

					data.frontColor = whiteLight;
					data.brightness = SetBrightness(randomBrightness, brightness);
					data.backColor = redLight;

					passedRedA = true;
				}

				//The last 300 meter of the runway has red centerline lights.
				if(currentDistance >= redB){

					passedRedB = true;

					data.frontColor = redLight;
					data.brightness = SetBrightness(randomBrightness, brightness);
					data.backColor = whiteLight;
				}

				//The last 900 meter of the runway has alternating red and white lights.
				if(passedAlternateA && !passedRedA){

					if(even){

						data.frontColor = whiteLight;
						data.brightness = SetBrightness(randomBrightness, brightness);
						data.backColor = redLight;
					}

					else{

						data.frontColor = whiteLight;
						data.brightness = SetBrightness(randomBrightness, brightness);
						data.backColor = whiteLight;
					}
				}

				//The last 900 meter of the runway has alternating red and white lights.
				if(passedAlternateB && !passedRedB){

					if(even){

						data.frontColor = redLight;
						data.brightness = SetBrightness(randomBrightness, brightness);
						data.backColor = whiteLight;
					}

					else{

						data.frontColor = whiteLight;
						data.brightness = SetBrightness(randomBrightness, brightness);
						data.backColor = whiteLight;
					}
				}

				//Middle of the runway
				if(!passedRedA && !passedRedB && !passedAlternateA && !passedAlternateB){

					data.frontColor = whiteLight;
					data.brightness = SetBrightness(randomBrightness, brightness);
					data.backColor = whiteLight;
				}

				data.size = size;

				lightData.Add(data);

				currentPosition += lengthOffsetVec;
			}
		}
	}

	//Setup all the edge lights.
	private void SetupEdgeLights(ref List<SpriteLights.LightData> lightData, RunwayData runwayData, bool randomBrightness, float brightness, float size){

		float sideFactor = 0;

		//Calculate the amount of lights.
		int lightAmountInRow = (int)Mathf.Floor(runwayData.length / runwayData.edgeLightsSpacing);

		//Calculate the offset direction.
		Vector3 lengthOffsetDir = Math3d.GetForwardVector(runwayData.rotation[0]);
		Vector3 sideOffsetDir = Math3d.GetRightVector(runwayData.rotation[0]);

		//Calculate the start position.
		float rowLength = (lightAmountInRow - 1) * runwayData.edgeLightsSpacing;
		float edgeForwardOffset = (runwayData.length - rowLength) * 0.5f;

		Vector3 lengthEdgeOffsetVec = Math3d.SetVectorLength(lengthOffsetDir, edgeForwardOffset);
		Vector3 startOffsetVec = lengthEdgeOffsetVec + Math3d.SetVectorLength(sideOffsetDir, runwayData.width * 0.5f);
		Vector3 startPosition = runwayData.thresholdPosition[0] + startOffsetVec;
		Vector3 currentPosition = startPosition;

		Vector3 lengthOffsetVec = Math3d.SetVectorLength(lengthOffsetDir, runwayData.edgeLightsSpacing);

		//Calculate the point where the lights should change color.
		float amberA = 600f + runwayData.edgeLightsSpacing;		
		float amberB = (runwayData.length - (600f + runwayData.edgeLightsSpacing));

		int doubleLightAmountInRow = lightAmountInRow * 2;

		for(int i = 0; i < doubleLightAmountInRow; i++){

			bool passedAmberA = false;
			bool passedAmberB = false;

			SpriteLights.LightData data = new SpriteLights.LightData();

			//Shift the current position for the other side.
			if(i == lightAmountInRow){

				startOffsetVec = lengthEdgeOffsetVec + Math3d.SetVectorLength(-sideOffsetDir, runwayData.width * 0.5f);
				currentPosition = runwayData.thresholdPosition[0] + startOffsetVec;

				//Reset flags.
				passedAmberA = false;
				passedAmberB = false;
				sideFactor = lightAmountInRow;
			}

			data.position = currentPosition;
			data.rotation = runwayData.rotation[0];

			//Calculate the distance to the threshold.
			float currentDistance = ((i - sideFactor) * runwayData.edgeLightsSpacing) + edgeForwardOffset;

			//The last 600 meter of the runway has red centerline lights.
			if(currentDistance <= amberA){

				data.frontColor = whiteLight;
				data.brightness = SetBrightness(randomBrightness, brightness);
				data.backColor = amberLight;

				passedAmberA = true;
			}

			//The last 300 meter of the runway has red centerline lights.
			if(currentDistance >= amberB){

				passedAmberB = true;

				data.frontColor = amberLight;
				data.brightness = SetBrightness(randomBrightness, brightness);
				data.backColor = whiteLight;
			}

			//Middle of the runway
			if(!passedAmberA && !passedAmberB){

				data.frontColor = whiteLight;
				data.brightness = SetBrightness(randomBrightness, brightness);
				data.backColor = whiteLight;
			}

			data.size = size;

			lightData.Add(data);

			currentPosition += lengthOffsetVec;
		}
	}

	//Setup all the papi lights.
	private void SetupPapiLights(ref List<SpriteLights.LightData> lightData, RunwayData runwayData, bool randomBrightness){

		for(int side = 0; side < 2; side++){

			if(runwayData.papiType[side] != PapiType.NONE){

				//Calculate the offset direction.
				Vector3 lengthOffsetDir = Math3d.GetForwardVector(runwayData.rotation[side]);
				Vector3 sideOffsetDir = Math3d.GetRightVector(runwayData.rotation[side]);
				Vector3 lengthEdgeOffsetVec = Math3d.SetVectorLength(lengthOffsetDir, 337f);

				if((runwayData.papiType[side] == PapiType.PAPIRIGHT) || (runwayData.papiType[side] == PapiType.PAPIBOTH)){
			
					Vector3 startOffsetVec = lengthEdgeOffsetVec + Math3d.SetVectorLength(sideOffsetDir, (runwayData.width * 0.5f) + 15f);
					Vector3 startPosition = runwayData.thresholdPosition[side] + startOffsetVec;
					Vector3 currentPosition = startPosition;

					Vector3 sideOffsetVec = Math3d.SetVectorLength(sideOffsetDir, 9f);

					for(int i = 0; i < 4; i++){

						SpriteLights.LightData data = new SpriteLights.LightData();

						float angle = GetPapiAngle(i);

						data.position = currentPosition;
						data.rotation = runwayData.rotation[side] * Quaternion.Euler(Vector3.right * angle);
						lightData.Add(data);

						currentPosition += sideOffsetVec;
					}
				}

				if((runwayData.papiType[side] == PapiType.PAPILEFT) || (runwayData.papiType[side] == PapiType.PAPIBOTH)){

					sideOffsetDir *= -1;

					Vector3 startOffsetVec = lengthEdgeOffsetVec + Math3d.SetVectorLength(sideOffsetDir, (runwayData.width * 0.5f) + 15f);
					Vector3 startPosition = runwayData.thresholdPosition[side] + startOffsetVec;
					Vector3 currentPosition = startPosition;

					Vector3 sideOffsetVec = Math3d.SetVectorLength(sideOffsetDir, 9f);

					for(int i = 0; i < 4; i++){

						SpriteLights.LightData data = new SpriteLights.LightData();

						float angle = GetPapiAngle(i);

						data.position = currentPosition;
						data.rotation = runwayData.rotation[side] * Quaternion.Euler(Vector3.right * angle);
						lightData.Add(data);

						currentPosition += sideOffsetVec;
					}
				}
			}
		}
	}
	
	//Get the angle for each PAPI light.
	private float GetPapiAngle(int index){

		float angle = 0;

		if(index == 0){

			angle = 3.3f;
		}

		if(index == 1){

			angle = 3.1f;
		}

		if(index == 2){

			angle = 2.5f;
		}

		if(index == 3){

			angle = 2.3f;
		}

		return angle;
	}

	//Set the brightness variable.
	private float SetBrightness(bool randomBrightness, float brightness){

		if(randomBrightness){

			return Random.Range(brightness, 1f);
		}

		else{

			return brightness;
		}
	}

	//Returns true if value is an even number.
	private bool IsEven(int value){

		return value % 2 == 0;
	}
}
