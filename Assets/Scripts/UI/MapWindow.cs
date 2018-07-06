using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours;

namespace SanAndreasUnity.UI {

	public class MapWindow : PauseMenuWindow {

		//private	Rect	visibleMapRect = new Rect ();
		private	Vector2	m_focusPos = Vector2.one * MiniMap.mapSize / 2.0f;
		private	float	zoomLevel = 1;
		private	float	controlsAreaHeight = 60;



		MapWindow() {

			// set default parameters

			this.isOpened = false;
			this.windowName = "Map";
			this.useScrollView = false;

		}

		void Start () {

			this.RegisterButtonInPauseMenu ();

			// adjust rect
			this.windowRect = Utilities.GUIUtils.GetCenteredRectPerc (new Vector2 (0.9f, 0.9f));

		}


		public	void	FocusOnPlayer() {

			Vector3 playerWorldPos = Player.Instance.transform.position;
			//	Vector2 focusWorldPos = new Vector2 (playerWorldPos.x, playerWorldPos.z);
			//	Vector2 focusPosNormalized = focusPos / MiniMap.mapEdge;
			Vector2 focusPos = MiniMap.WorldPosToMapTexturePos(playerWorldPos);

			// flip y axis
		//	focusPos.y = MiniMap.mapSize - focusPos.y;

			this.SetFocusPosition (focusPos);

		}

		public	void	SetFocusPosition(Vector2 pos) {

		//	Vector2 bottomLeftPos = pos;
		//	bottomLeftPos.x -= this.visibleMapRect.width / 2.0f;
		//	bottomLeftPos.y -= this.visibleMapRect.height / 2.0f;

		//	this.visibleMapRect.position = bottomLeftPos;

			this.m_focusPos = pos;

		}

		public	Vector2	GetFocusPosition() {

		//	return this.visibleMapRect.position + this.visibleMapRect.size / 2.0f;
			return this.m_focusPos;
		}


		public	Rect	GetMapDisplayRect() {

			float mapDisplayWidth = this.windowRect.width;
			float mapDisplayHeight = this.windowRect.height - this.controlsAreaHeight;
			Rect mapDisplayRect = new Rect (0, 0, mapDisplayWidth, mapDisplayHeight);
			return mapDisplayRect;
		}

		public	Vector2	GetVisibleMapSize() {

			Rect mapDisplayRect = this.GetMapDisplayRect ();

			float visibleMapWidth = Mathf.Min( MiniMap.mapSize / 4.0f / this.zoomLevel, MiniMap.mapSize);
			float visibleMapHeight = Mathf.Min (visibleMapWidth * mapDisplayRect.height / mapDisplayRect.width, MiniMap.mapSize);

			return new Vector2 (visibleMapWidth, visibleMapHeight);
		}

		public	Rect	GetVisibleMapRect() {

			Vector2 size = GetVisibleMapSize ();

			return new Rect (this.m_focusPos - size / 2.0f, size);
		}

//		private	void	AdjustVisibleMapRectAfterZooming() {
//
//			Vector2 focusPos = this.GetFocusPosition ();
//
//			// visible map size is changed after zooming
//			Vector2 size = this.GetVisibleMapSize ();
//
//			this.visibleMapRect = new Rect(focusPos - size / 2.0f, size);
//
//		}


		public	static	Vector2	ClampPositionInsideMap(Vector2 pos) {

			pos.x = Mathf.Clamp( pos.x, 0.0f, MiniMap.mapSize);
			pos.y = Mathf.Clamp( pos.y, 0.0f, MiniMap.mapSize);

			return pos;
		}

		private	void	ClampFocusPos() {

			// clamp focus position inside map
			this.SetFocusPosition (ClampPositionInsideMap (this.GetFocusPosition ()));

			// again, clamp focus position, but based on visible map rectangle - the rectangle must not go out of map boundaries, or the texture will stretch
//			Vector2 visibleMapSize = this.GetVisibleMapSize();
//			if (visibleMapSize.x < GetMapSize ()) {
//				this.m_focusPos.x = Mathf.Clamp (this.m_focusPos.x, GetMinMapPos ().x + visibleMapSize.x / 2.0f, GetMaxMapPos ().x - visibleMapSize.x / 2.0f);
//			}
//			if (visibleMapSize.y < GetMapSize ()) {
//				this.m_focusPos.y = Mathf.Clamp (this.m_focusPos.y, GetMinMapPos ().y + visibleMapSize.y / 2.0f, GetMaxMapPos ().y - visibleMapSize.y / 2.0f);
//			}

		}

		public	static	Vector2	GetMinMapPos() {
			return Vector2.zero;
		}

		public	static	Vector2	GetMaxMapPos() {
			return Vector2.one * MiniMap.mapSize;
		}

		public	static	float	GetMapSize() {
			return MiniMap.mapSize;
		}


		void Update() {

			if (!PauseMenu.IsOpened || !this.isOpened)
				return;
			

			this.m_focusPos += new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw("Vertical")) 
				* 100 * Time.deltaTime / this.zoomLevel;

			this.ClampFocusPos ();


			float oldZoomLevel = this.zoomLevel;

			if (Input.GetKey (KeyCode.KeypadPlus)) {
				zoomLevel *= 1.1f;
			}

			if (Input.GetKey (KeyCode.KeypadMinus)) {
				zoomLevel /= 1.1f;
			}

			zoomLevel = Mathf.Clamp (zoomLevel, 0.05f, 10f);

			if (oldZoomLevel != this.zoomLevel) {
			//	this.AdjustVisibleMapRectAfterZooming ();
			}

		}


		protected override void OnWindowGUI ()
		{

			if (null == MiniMap.Instance)
				return;
			if (null == Player.Instance)
				return;


			int uiSize = (int) this.windowRect.width;
			Texture2D mapTexture = MiniMap.Instance.MapTexture;
			Texture2D blackPixel = MiniMap.Instance.BlackPixel;
			Texture2D seaPixel = MiniMap.Instance.SeaPixel;


			Rect mapDisplayRect = this.GetMapDisplayRect ();
			Rect visibleMapRect = this.GetVisibleMapRect ();
		//	this.visibleMapRect.size = this.GetVisibleMapSize();



			//if (!toggleMap) {
			if(false) {
				
				//GUILayout.BeginArea (new Rect (Screen.width - uiSize - 10, uiSize + 20, uiSize, 80));

				GUIStyle style = new GUIStyle ("label") { alignment = TextAnchor.MiddleCenter };


				// draw some info in upper left corner

				Vector2 labelSize = new Vector2 (uiSize, 25);
				Rect labelRect = new Rect (Vector2.zero, labelSize);

				GUI.DrawTexture (labelRect, blackPixel);
				if (Player.Instance != null) {
					Vector3 pPos = Player.Instance.transform.position;
					GUI.Label (labelRect,
						string.Format ("x: {0}, y: {1}, z: {2}", pPos.x.ToString ("F2"), pPos.y.ToString ("F2"), pPos.z.ToString ("F2")),
						style);
				}


				// draw zone name

				//Rect zoneRect = new Rect (uiSize / 2 - uiSize / (2 * 3), 25, uiSize / 3, 25);

				//GUI.DrawTexture (zoneRect, blackPixel);
				//GUI.Label (zoneRect, ZoneName, style);


				bool showZoomPanel = true;
				if (showZoomPanel) {
					// display zoom panel

					Color previousColor = GUI.color;

					Rect zoomPanel = new Rect (uiSize / 2 - uiSize / (2 * 4), 55, uiSize / 4, 25);

					float fAlpha = 1;

					GUI.color = new Color (0, 0, 0, fAlpha);

					// fill everything with black
					GUI.DrawTexture (zoomPanel, blackPixel);

					GUI.color = new Color (255, 255, 255, fAlpha);

					// display zoom percentage
					float curZoomPercentage = 1;
					GUI.Label (zoomPanel, string.Format ("x{0}", curZoomPercentage.ToString ("F2")), style);

					GUI.color = previousColor;
				}


				//GUILayout.EndArea ();

			} else {
				
				//mapRect = new Vector2 (mapTexture.width, mapTexture.height) * (baseScale * (mapScale / mapMaxScale) * 2);

				// fill everything with black - why ?
				//GUI.DrawTexture (new Rect (50, 50, Screen.width - 100, Screen.height - 100), blackPixel);

				// fill everything with sea
				GUI.DrawTexture (mapDisplayRect, seaPixel);

				//GUILayout.BeginArea (new Rect (mapUpperLeftCorner, windowSize));

				//GUILayout.BeginArea (new Rect (mapScroll, mapRect));

				// draw the map texture

				//GUI.DrawTexture (new Rect (mapZoomPos, mapRect), mapTexture);
				//GUI.DrawTexture (new Rect (Vector2.zero, this.windowRect.size), MiniMap.Instance.MapTexture);

				Rect texCoords = new Rect(visibleMapRect.x / mapTexture.width, visibleMapRect.y / mapTexture.height,
					visibleMapRect.width / mapTexture.width, visibleMapRect.height / mapTexture.height);

				texCoords = Utilities.F.Clamp01 (texCoords);

				// adjust display rect
				Rect renderRect = mapDisplayRect;
			//	float mulX = mapDisplayRect.width / GetMapSize ();
			//	float mulY = mapDisplayRect.height / GetMapSize ();

				if(visibleMapRect.xMax > GetMapSize()) {
					// reduce display width
				//	renderRect.width -= (visibleMapRect.xMax - GetMapSize()) * mulX ;
					float perc = (visibleMapRect.xMax - GetMapSize()) / visibleMapRect.width;
					renderRect.xMax -= perc * mapDisplayRect.width;
				}
				if (visibleMapRect.xMin < 0) {
					// increase x pos
				//	renderRect.xMin += - visibleMapRect.xMin * mulX;
					float perc = - visibleMapRect.xMin / visibleMapRect.width;
					renderRect.xMin += perc * mapDisplayRect.width;
				}
				if(visibleMapRect.yMax > GetMapSize()) {
					// reduce display height from top
				//	renderRect.yMin += (visibleMapRect.yMax - GetMapSize()) * mulY ;
					float perc = (visibleMapRect.yMax - GetMapSize()) / visibleMapRect.height;
					renderRect.yMin += perc * mapDisplayRect.height;
				}
				if (visibleMapRect.yMin < 0) {
					// reduce display height from bottom
				//	renderRect.yMax -= - visibleMapRect.yMin * mulY;
					float perc = - visibleMapRect.yMin / visibleMapRect.height;
					renderRect.yMax -= perc * mapDisplayRect.height;
				}

			//	mapTexture.wrapMode = TextureWrapMode.Clamp;

				GUI.DrawTextureWithTexCoords(renderRect, mapTexture, texCoords);

				// what's this ?
				//GUI.DrawTexture (new Rect (Vector2.zero, Vector2.one * 16), blackPixel);

				// TODO: load move cursor, load map bars, load marker, draw player pointer & undiscovered zones, drag & drop

				// TODO: focus on player when map is opened
				// TODO: close map on Esc


				//GUILayout.EndArea ();
				//GUILayout.EndArea ();


				// draw controls

				GUILayout.BeginArea(new Rect(3, mapDisplayRect.yMax, mapDisplayRect.width - 3, this.controlsAreaHeight));
				GUILayout.Space (10);
				GUILayout.BeginHorizontal ();

				if (GUILayout.Button ("Focus on player")) {
					FocusOnPlayer ();
				}
				GUILayout.Space (10);
				GUILayout.Label ("Player world pos: " + Player.Instance.transform.position);
				GUILayout.Space (5);
				GUILayout.Label ("Player minimap pos: " + MiniMap.WorldPosToMapTexturePos (Player.Instance.transform.position));
				GUILayout.Space (5);
				GUILayout.Label ("Focus pos: " + this.GetFocusPosition ());
				GUILayout.Space (5);
				GUILayout.Label ("Zoom: " + this.zoomLevel);
				GUILayout.Space (5);
				GUILayout.Label ("Press arrows to move, +/- to zoom");

				GUILayout.EndHorizontal ();
				GUILayout.EndArea ();

			}

		}



	}

}
