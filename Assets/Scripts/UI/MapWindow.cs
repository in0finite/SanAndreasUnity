using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours;
using UGameCore.Utilities;
using System.Linq;
using SanAndreasUnity.Importing.Paths;
using UnityEngine.AI;

namespace SanAndreasUnity.UI {

	public class MapWindow : PauseMenuWindow {

		public static MapWindow Instance { get; private set; }

		//private	Rect	visibleMapRect = new Rect ();
		private	Vector2	m_focusPos = Vector2.one * MiniMap.mapSize / 2.0f;
		private	float	zoomLevel = 1;
		private	float	infoAreaHeight = 90;
		private	bool	m_clipMapItems = false;

		private	Texture2D	m_infoAreaTexture;

		private	float	m_playerPointerSize = 10;
		public float PlayerPointerSize { get => m_playerPointerSize; set { m_playerPointerSize = value; } }
		private	bool	m_drawZones = false;
		private bool m_drawEnexes = false;

		private	bool	m_isWaypointPlaced = false;
		private	Vector2	m_waypointMapPos = Vector2.zero;

		private List<PathNodeId> m_pathToWaypoint = null;
		private Vector3[] m_navMeshPathToWaypoint = null;
		private enum PathType
        {
			NavMeshFull,
			NavMesh,
			Ped,
        }
		private PathType m_pathType = PathType.NavMeshFull;
		private static readonly string[] s_pathTypeTexts = new string[] { "NavMeshFull", "NavMesh", "Ped" };
		private int m_selectedPathType = 0;

		private	Vector2	m_lastMousePosition = Vector2.zero;

		private	Vector2	m_infoAreaScrollViewPos = Vector2.zero;

		public event System.Action onDrawMapItems = delegate {};



		MapWindow() {

			// set default parameters

			this.windowName = "Map";
			this.useScrollView = false;
			this.isDraggable = false;
			this.isModal = true;
			this.m_hasExitButton = false;
			this.m_hasMinimizeButton = false;

		}

		protected override void Awake () {
			
			base.Awake();

			if (null == Instance)
				Instance = this;

			m_infoAreaTexture = F.CreateTexture (1, 1, Color.grey);

		}

		void Start () {

			this.RegisterButtonInPauseMenu ();

			// adjust rect
			this.windowRect = GUIUtils.GetCenteredRectPerc (new Vector2 (1.0f, 1.0f));

		}

		protected	override	void	OnWindowOpened() {

			this.FocusOnPlayer ();

		}


		public	void	FocusOnPlayer() {

			if (null == Ped.Instance)
				return;

			Vector3 playerWorldPos = Ped.Instance.transform.position;
			//	Vector2 focusWorldPos = new Vector2 (playerWorldPos.x, playerWorldPos.z);
			//	Vector2 focusPosNormalized = focusPos / MiniMap.mapEdge;
			Vector2 focusPos = MiniMap.WorldPosToMapPos(playerWorldPos);

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


		/// <summary>
		/// Rect in which the map is displayed, relative to window area.
		/// </summary>
		public	Rect	GetMapDisplayRect() {

			float mapDisplayWidth = this.windowRect.width;
			float mapDisplayHeight = this.windowRect.height - this.infoAreaHeight;
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


		public	Vector2	ScreenPosToDisplayPos(Vector2 screenPos) {

			Rect displayRect = this.GetMapDisplayRect ();

			Vector2 displayPos = screenPos - displayRect.position;
			// flip Y axis
			displayPos.y = Screen.height - displayPos.y;

			return displayPos;
		}

		public	bool	GetMapPosUnderMouse(out Vector2 mapPos) {

			mapPos = Vector2.zero;

			Vector2 displayPos = ScreenPosToDisplayPos (m_lastMousePosition);

			// check if it is inside display rect
			if (!this.GetMapDisplayRect ().Contains (displayPos))
				return false;

			mapPos = DisplayPosToMapPos (displayPos);

			return true;
		}

		public	bool	GetWorldPosUnderMouse(out Vector3 worldPos) {

			worldPos = Vector3.zero;

			Vector2 mapPos;
			if(!this.GetMapPosUnderMouse(out mapPos))
				return false;

			worldPos = MiniMap.MapPosToWorldPos (mapPos);

			return true;
		}

		public	Vector2	DisplayPosToMapPos(Vector2 displayPos) {

			Rect mapDisplayRect = this.GetMapDisplayRect ();
			Rect visibleMapRect = this.GetVisibleMapRect ();

			// don't know why is this needed
			// flip Y axis
			displayPos.y = mapDisplayRect.height - displayPos.y;

			Vector2 normalizedPos = Rect.PointToNormalized (mapDisplayRect, displayPos);

			Vector2 mapPos = Rect.NormalizedToPoint (visibleMapRect, normalizedPos);

			return mapPos;
		}


		private	void	TeleportToWaypoint() {

			if (!m_isWaypointPlaced)
				return;

			if (null == Ped.Instance)
				return;

			Vector3 worldPos = MiniMap.MapPosToWorldPos (m_waypointMapPos);

			Chat.ChatManager.SendChatMessageToAllPlayersAsLocalPlayer($"/teleport {worldPos.x} {worldPos.y} {worldPos.z}");

		}

		private void FindPathToWaypoint()
        {
			if (!m_isWaypointPlaced)
				return;

			m_pathToWaypoint = null;
			m_navMeshPathToWaypoint = null;

			if (null == Ped.LocalPed)
				return;

			Vector3 sourcePos = Ped.LocalPed.transform.position;
			Vector3 targetPos = MiniMap.MapPosToWorldPos(m_waypointMapPos);

			if (m_pathType == PathType.Ped)
            {
				PathfindingManager.Singleton.FindPath(
					sourcePos,
					targetPos,
					OnPathToWaypointFound);
			}
			else if (m_pathType == PathType.NavMeshFull)
            {
				var sw = System.Diagnostics.Stopwatch.StartNew();

				if (NavMesh.SamplePosition(targetPos, out var hit, 300f, -1))
					targetPos = hit.position;

				m_navMeshPathToWaypoint = PathfindingManager.CalculateFullNavMeshPath(sourcePos, targetPos, -1).ToArray();

				LogPathCalculationInfo(m_navMeshPathToWaypoint, null, sw.Elapsed.TotalMilliseconds);
            }
			else if (m_pathType == PathType.NavMesh)
            {
				var sw = System.Diagnostics.Stopwatch.StartNew();

				if (NavMesh.SamplePosition(targetPos, out var hit, 300f, -1))
					targetPos = hit.position;

				var path = new NavMeshPath();
				if (NavMesh.CalculatePath(sourcePos, targetPos, -1, path))
					m_navMeshPathToWaypoint = path.corners;

				LogPathCalculationInfo(m_navMeshPathToWaypoint, path.status, sw.Elapsed.TotalMilliseconds);
			}
		}

		private void OnPathToWaypointFound(PathfindingManager.PathResult pathResult)
        {
			if (m_pathType != PathType.Ped)
				return;
			if (!m_isWaypointPlaced)
				return;

			if (pathResult.IsSuccess)
				m_pathToWaypoint = pathResult.Nodes;
			else
				m_pathToWaypoint = null;
        }

		private static void LogPathCalculationInfo(
			Vector3[] corners, NavMeshPathStatus? pathStatus, double timeElapsedMs)
        {
			Debug.Log($"Nav mesh path calculation done - " +
					(pathStatus.HasValue ? $"status {pathStatus.Value}, " : "") +
					$"num corners {corners?.Length ?? 0}, " +
					$"total distance {corners?.Skip(1).Select((corner, i) => Vector3.Distance(corner, corners[i])).Sum()}, " +
					$"time {timeElapsedMs} ms");
		}


		void Update() {

			if (Input.GetKeyDown (KeyCode.M)) {
				// toggle map

				if (this.IsOpened || GameManager.CanPlayerReadInput())
				{

					if (this.IsOpened)
					{
						this.IsOpened = false;
						// also close pause menu
						PauseMenu.IsOpened = false;
					}
					else
					{
						this.IsOpened = true;
						// also open pause menu
						PauseMenu.IsOpened = true;
					}

				}

			}


			if (!PauseMenu.IsOpened || !this.IsOpened)
				return;
			

			// move focused position

			this.m_focusPos += new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw("Vertical")) 
				* 100 * Time.deltaTime / this.zoomLevel;

			this.ClampFocusPos ();


			// zoom

			float oldZoomLevel = this.zoomLevel;

			if (Input.GetKey (KeyCode.KeypadPlus)) {
				zoomLevel *= 1.1f;
			}

			if (Input.GetKey (KeyCode.KeypadMinus)) {
				zoomLevel /= 1.1f;
			}

			// mouse scroll
			if (Input.mouseScrollDelta.y > 0)
				zoomLevel *= 1.1f;
			else if(Input.mouseScrollDelta.y < 0)
				zoomLevel /= 1.1f;

			zoomLevel = Mathf.Clamp (zoomLevel, 0.25f, 10f);

			if (oldZoomLevel != this.zoomLevel) {
			//	this.AdjustVisibleMapRectAfterZooming ();
			}


			// toggle waypoint on map

			if (Input.GetMouseButtonDown (1)) {

				Vector2 mouseMapPos;
				if (this.GetMapPosUnderMouse (out mouseMapPos)) {
					m_isWaypointPlaced = !m_isWaypointPlaced;

					m_pathToWaypoint = null;
					m_navMeshPathToWaypoint = null;

					if (m_isWaypointPlaced)
                    {
                        m_waypointMapPos = mouseMapPos;
						this.FindPathToWaypoint();
                    }
                }

			}

			// focus on player key shortcut
			if (Input.GetKeyDown (KeyCode.F)) {
				this.FocusOnPlayer ();
			}

			// teleport to waypoint key shortcut
			if (Input.GetKeyDown (KeyCode.T)) {
				this.TeleportToWaypoint ();
			}

			// remember last mouse position
			m_lastMousePosition = Input.mousePosition;


		}


		protected override void OnWindowGUI ()
		{

			if (null == MiniMap.Instance)
				return;


			int uiSize = (int) this.windowRect.width;
			Texture2D mapTexture = MiniMap.Instance.MapTexture;
			Texture2D blackPixel = MiniMap.Instance.BlackPixel;
			Texture2D seaPixel = MiniMap.Instance.SeaPixel;


			Rect mapDisplayRect = this.GetMapDisplayRect ();
			Rect visibleMapRect = this.GetVisibleMapRect ();
		//	this.visibleMapRect.size = this.GetVisibleMapSize();


			// fill everything with sea
			if (seaPixel != null)
				GUI.DrawTexture (mapDisplayRect, seaPixel);

			// draw the map texture
			this.DrawMapTexture( mapDisplayRect, visibleMapRect );

			// draw 2 lines crossing under cursor
			Vector2 mouseDisplayPos = ScreenPosToDisplayPos( m_lastMousePosition );
			float linesWidth = 4;
			Color linesColor = (Color.yellow + Color.black) / 2.0f;
			// vertical line
			GUIUtils.DrawRect (new Rect(mouseDisplayPos.x - linesWidth / 2.0f, 0, linesWidth, mapDisplayRect.height), linesColor);
			// horizontal line
			GUIUtils.DrawRect (new Rect(0, mouseDisplayPos.y - linesWidth / 2.0f, mapDisplayRect.width, linesWidth), linesColor);

			// draw map items
			this.DrawMapItems (mapDisplayRect);

			// draw info area
			this.DrawInfoArea( mapDisplayRect );

		}

		private	void	DrawMapTexture(Rect mapDisplayRect, Rect visibleMapRect) {

			Texture2D mapTexture = MiniMap.Instance.MapTexture;

			if (null == mapTexture)
				return;


			//GUI.DrawTexture (new Rect (mapZoomPos, mapRect), mapTexture);
			//GUI.DrawTexture (new Rect (Vector2.zero, this.windowRect.size), MiniMap.Instance.MapTexture);

			Rect texCoords = new Rect(visibleMapRect.x / mapTexture.width, visibleMapRect.y / mapTexture.height,
				visibleMapRect.width / mapTexture.width, visibleMapRect.height / mapTexture.height);

			texCoords = MathUtils.Clamp01 (texCoords);

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

		}

		private	void	DrawInfoArea (Rect mapDisplayRect) {


			Rect infoAreaRect = new Rect (3, mapDisplayRect.yMax, mapDisplayRect.width - 3, this.infoAreaHeight);

			GUILayout.BeginArea (infoAreaRect);
			if (m_infoAreaTexture != null)
				GUI.DrawTexture (new Rect(new Vector2(-infoAreaRect.x, 0), infoAreaRect.size), m_infoAreaTexture);

			m_infoAreaScrollViewPos = GUILayout.BeginScrollView (m_infoAreaScrollViewPos);

			GUILayout.Space (10);

			// first row - controls
			GUILayout.BeginHorizontal (GUILayout.MaxWidth (infoAreaRect.width));

			if (GUILayout.Button ("Focus on player [F]", GUILayout.MinHeight(25))) {
				this.FocusOnPlayer ();
			}
			if (GUILayout.Button ("Teleport to waypoint [T]", GUILayout.MinHeight(25))) {
				this.TeleportToWaypoint ();
			}
			GUILayout.Space(10);
			GUILayout.Label ("Player size: " + (int) m_playerPointerSize, GUILayout.ExpandWidth(false));
			m_playerPointerSize = GUILayout.HorizontalSlider (m_playerPointerSize, 1, 50, GUILayout.MinWidth(40));
			GUILayout.Space(10);
			m_drawZones = GUILayout.Toggle (m_drawZones, "Draw zones");
			GUILayout.Space(10);
			m_drawEnexes = GUILayout.Toggle(m_drawEnexes, "Draw enexes");

			GUILayout.Space(10);
			GUILayout.Label("Path:");
			int newSelectedPathType = GUILayout.SelectionGrid(m_selectedPathType, s_pathTypeTexts, s_pathTypeTexts.Length);
			if (newSelectedPathType != m_selectedPathType)
            {
				m_selectedPathType = newSelectedPathType;
				m_pathType = (PathType)System.Enum.GetValues(typeof(PathType)).GetValue(m_selectedPathType);
				this.FindPathToWaypoint();
            }

			GUILayout.FlexibleSpace();

			GUILayout.EndHorizontal ();

			// second row - info
			GUILayout.BeginHorizontal (GUILayout.MaxWidth (infoAreaRect.width));

			if (Ped.Instance != null)
			{
				GUILayout.Label ("Player world pos: " + Ped.Instance.transform.position);
				GUILayout.Space (5);
				GUILayout.Label ("Player minimap pos: " + MiniMap.WorldPosToMapPos (Ped.Instance.transform.position));
				GUILayout.Space (5);
			}
			GUILayout.Label ("Focus pos: " + this.GetFocusPosition ());
			GUILayout.Space (5);
			Vector2 cursorMapPos;
			if (this.GetMapPosUnderMouse (out cursorMapPos))
				GUILayout.Label ("Cursor pos: " + cursorMapPos);
			GUILayout.Space (5);
			GUILayout.Label ("Zoom: " + this.zoomLevel);
			// zone name under cursor
			GUILayout.Space (5);
			Vector3 mouseWorldPos;
			if (this.GetWorldPosUnderMouse (out mouseWorldPos)) {
				GUILayout.Label ("cursor world pos: " + mouseWorldPos);
				GUILayout.Label ("Zone: " + Importing.Zone.GetZoneName (mouseWorldPos, true), GUILayout.Width(80));
			}

			GUILayout.EndHorizontal ();

			GUILayout.Space (5);
			GUILayout.Label ("Controls: arrows/WASD - move, +/-/scroll - zoom, right click - place waypoint");

			GUILayout.EndScrollView ();

			GUILayout.EndArea ();

		}

		private	void	DrawMapItems(Rect mapDisplayRect) {


			if (!m_clipMapItems) {
				GUI.EndClip ();
				//	GUI.EndGroup ();
			} else {
				GUI.BeginGroup (mapDisplayRect);	// ensure that all map items are drawn inside this rect - doesn't work when items are rotated
			}


			// draw registered items
			onDrawMapItems();

			// draw enexes
			if (m_drawEnexes)
			{
				bool worldExists = Behaviours.World.Cell.Instance != null;
				foreach (var enex in Importing.Items.Item.Enexes.Where(enex => Behaviours.World.Cell.IsExteriorLevel(enex.TargetInterior)))
				{
					this.DrawItemOnMap(
						MiniMap.Instance.GreenHouseTexture,
						worldExists
							? Behaviours.World.Cell.Instance.GetEnexEntranceTransform(enex).position
							: enex.EntrancePos,
						10);
				}
			}

			// draw player pointer
			if (Ped.Instance != null)
			{
				this.DrawItemOnMapRotated( MiniMap.Instance.PlayerBlip, Ped.Instance.transform.position, Ped.Instance.transform.forward, (int) m_playerPointerSize );
			}

			// draw all zones
			if (m_drawZones) {
				
				foreach (var zone in Importing.Zone.AllZones) {
					
					Vector2 min = MiniMap.WorldPosToMapPos (zone.vmin);
					Vector2 max = MiniMap.WorldPosToMapPos (zone.vmax);
					Rect rect = new Rect (min, max - min);

					Rect renderRect;
					if (this.GetMapItemRenderRect (rect, out renderRect)) {
						if (renderRect.height > 2 && renderRect.width > 2)
						{
							// add some space between zones
							renderRect.xMin ++;
							renderRect.xMax --;
							renderRect.yMin ++;
							renderRect.yMax --;

							GUI.Box (renderRect, "");
							GUIUtils.CenteredLabel (renderRect.center, zone.name);
						}
					}

				}
			}

			// draw waypoint
			if (m_isWaypointPlaced) {
				this.DrawItemOnMap (MiniMap.Instance.WaypointTexture, m_waypointMapPos, 12);
			}

			// draw ped path to waypoint
			if (m_pathToWaypoint != null)
            {
                foreach (PathNodeId nodeId in m_pathToWaypoint)
                {
					Vector2 pos = MiniMap.WorldPosToMapPos(NodeReader.GetNodeById(nodeId).Position);
					if (GetMapItemRenderRect(MathUtils.CreateRect(pos, Vector2.one * 2), out Rect renderRect))
						GUIUtils.DrawRect(renderRect, Color.yellow);
                }
            }

			// draw navmesh path to waypoint
			if (m_navMeshPathToWaypoint != null)
			{
				foreach (Vector3 worldPos in m_navMeshPathToWaypoint)
				{
					Vector2 pos = MiniMap.WorldPosToMapPos(worldPos);
					if (GetMapItemRenderRect(MathUtils.CreateRect(pos, Vector2.one * 2), out Rect renderRect))
						GUIUtils.DrawRect(renderRect, Color.yellow);
				}
			}


			if (!m_clipMapItems) {
				//	GUI.BeginGroup (new Rect (0, 0, Screen.width, Screen.height));
				GUI.BeginClip (this.windowRect);
			} else {
				GUI.EndGroup ();
			}


		}


		public	bool	GetMapItemRenderRect( Rect itemMapBoundsRect, out Rect renderRect ) {

			renderRect = Rect.zero;

		//	Texture2D mapTexture = MiniMap.Instance.MapTexture;

			Rect visibleMapRect = this.GetVisibleMapRect ();

			if (!visibleMapRect.Overlaps (itemMapBoundsRect) && !visibleMapRect.Contains (itemMapBoundsRect)) {
			//	Debug.LogFormat ("Item rect {0} is not within visible rect {1}", itemRect, visibleMapRect);
				return false;
			}

			// just convert map pos to screen pos
			Rect displayRectNormalized = itemMapBoundsRect.Normalized( visibleMapRect );



			/*
			// get intersection between these rects
			Rect intersectionRect = itemRect.Intersection (visibleMapRect);

			Rect displayRectNormalized = intersectionRect.Normalized (visibleMapRect);
		//	displayRectNormalized.y = 1.0f - displayRectNormalized.y;
			Rect texCoords = intersectionRect.Normalized (itemRect);


			// just in case
			displayRectNormalized = displayRectNormalized.Clamp01();
			texCoords = texCoords.Clamp01 ();
			*/


			// adjust display rect

			Rect mapDisplayRect = this.GetMapDisplayRect ();
			Vector2 renderRectPos = mapDisplayRect.position + Vector2.Scale (mapDisplayRect.size, displayRectNormalized.position);
			renderRectPos.y = mapDisplayRect.height - renderRectPos.y;
			Vector2 renderRectSize = Vector2.Scale (mapDisplayRect.size, displayRectNormalized.size);
			renderRectPos.y -= renderRectSize.y;
			renderRect = new Rect (renderRectPos, renderRectSize);


		//	GUI.DrawTextureWithTexCoords(renderRect, itemTexture, texCoords);


		//	Debug.LogFormat ("Drawn item: item rect {0} visible map rect {1} intersection rect {2} displayRectNormalized {3} " +
		//	"texCoords {4} mapDisplayRect {5} renderRect {6}", itemRect, visibleMapRect, intersectionRect, displayRectNormalized,
		//		texCoords, mapDisplayRect, renderRect);

			return true;
		}

		public	void	DrawItemOnMap( Texture2D itemTexture, Rect itemRect ) {

			Rect renderRect;
			if (GetMapItemRenderRect (itemRect, out renderRect)) {
				GUI.DrawTexture (renderRect, itemTexture);
			}

		}

		public	void	DrawItemOnMap( Texture2D itemTexture, Vector2 mapPos, int itemSize ) {

			this.DrawItemOnMap (itemTexture, MathUtils.CreateRect (mapPos, Vector2.one * itemSize));

		}

		public	void	DrawItemOnMap( Texture2D itemTexture, Vector3 worldPos, int itemSize ) {

			Vector2 mapPos = MiniMap.WorldPosToMapPos (worldPos);

			this.DrawItemOnMap (itemTexture, mapPos, itemSize);

		}

		public	void	DrawItemOnMapRotated( Texture2D itemTexture, Vector3 worldPos, Vector3 worldDir, int itemSize ) {

			Vector2 mapPos = MiniMap.WorldPosToMapPos (worldPos);

			this.DrawItemOnMapRotated (itemTexture, MathUtils.CreateRect (mapPos, Vector2.one * itemSize), worldDir);

		}

		public	void	DrawItemOnMapRotated( Texture2D itemTexture, Rect itemRect, Vector3 worldDir ) {

			Rect renderRect;
			if (!GetMapItemRenderRect (itemRect, out renderRect)) {
				return;
			}


			// find angle around Y axis
			Vector3 dir = new Vector3(worldDir.x, 0, worldDir.z).normalized;
			Quaternion q = Quaternion.LookRotation (dir, Vector3.up);
			float angle = q.eulerAngles.y - 180.0f;

			// save matrix
			var oldMatrix = GUI.matrix;

			// rotate around center of item
			GUIUtility.RotateAroundPivot( angle, renderRect.center );

			// draw
			GUI.DrawTexture (renderRect, itemTexture);

			// restore matrix
			GUI.matrix = oldMatrix;


		}


	}

}
