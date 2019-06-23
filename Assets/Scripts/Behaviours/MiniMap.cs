using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Utilities;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SanAndreasUnity.Behaviours
{
    public class MiniMap : MonoBehaviour
    {
        public const int tileEdge = 12; // width/height of map in tiles
        public const int tileCount = tileEdge * tileEdge; // number of tiles
        public const int mapEdge = 6000; // width/height of map in world coordinates
        public const int texSize = 128; // width/height of single tile in px
        public const int mapSize = tileEdge * texSize; // width/height of whole map in px
        public const int uiSize = 256, uiOffset = 10;
        private const bool outputChunks = false, outputImage = false;

        public static bool toggleMap;

        public Canvas outlineCanvas,
                      iconCanvas,
                      canvas;

        public Image northImage,
                     playerImage,
                     outlineImage,
                     maskImage,
                     mapImage;

        public RectTransform mapTransform,
                             maskTransform,
                             mapContainer;

        public float zoom = 1.3f;
        private const float scaleConst = 1f;

        public const float maxVelocity = 300f;
        public static float[] zooms = new float[10] { .5f, .75f, 1f, 1.2f, 1.4f, 1.6f, 2f, 2.5f, 3f, 5f };

        // Why?
        [HideInInspector] [Obsolete] public float calibrator = 2.34f;

        public float zoomDuration = 1,
                     mapZoomScaler = 1,
                     mapMovement = 5;

        public bool debugActive = true;

        #region "Properties"

        public static MiniMap Instance { get; private set; }

        private float realZoom
        {
            get
            {
                return zoom * scaleConst;
            }
            set
            {
                zoom = value / scaleConst;
            }
        }

        private Vector3 pPos
        {
            get
            {
                if (m_ped != null)
                    return m_ped.transform.position;
                if (Camera.main != null)
                    return Camera.main.transform.position;
                return Vector3.zero;
            }
        }

        private float _gTimer;

        private string _zName;

        private string ZoneName
        {
            get
            {
                if (_gTimer == 0)
                {
                    Vector3 playerPos = Vector3.zero;
                    try
                    {
                        playerPos = pPos;
                    }
                    catch { }

                    _zName = SZone.GetZoneName(ZoneHelpers.zoneInfoList, playerPos);
                }

                return _zName;
            }
        }

        public Texture2D NorthBlip { get { return this.northBlip; } }

        public Texture2D PlayerBlip { get { return this.playerBlip; } }

        public Texture2D WaypointTexture { get { return this.waypointTexture; } }

        public Texture2D MapTexture { get { return this.mapTexture; } }

        public Texture2D BlackPixel { get { return this.blackPixel; } }

        public Texture2D SeaPixel { get { return this.seaPixel; } }

        #endregion "Properties"

        public static void AssingMinimap()
        {
            GameObject UI = GameObject.FindGameObjectWithTag("UI");
            Transform root = UI != null ? UI.transform : null;

            GameObject minimap = GameObject.FindGameObjectWithTag("Minimap");
            if (minimap == null)
            {
                minimap = new GameObject();

                minimap.name = "Minimap";
                minimap.tag = "Minimap";

                minimap.transform.parent = root;
            }

            MiniMap map = minimap.GetComponent<MiniMap>();

            if (map == null)
                map = minimap.AddComponent<MiniMap>();

            if (!map.isSetup) map.Setup();
        }

        private void loadTextures()
        {
            mapTexture = new Texture2D(mapSize, mapSize, TextureFormat.ARGB32, false, true);

            string folder = Path.Combine(Application.streamingAssetsPath, "map-chunks");

            if (outputChunks)
            {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }

            Debug.Log("Merging all map sprites into one sprite.");
            for (int i = 0; i < tileCount; i++)
            {
                // Offset
                int y = ((i / tileEdge) + 1) * texSize,
                    x = (i % tileEdge) * texSize;

                string name = "radar" + ((i < 10) ? "0" : "") + i;
                var texDict = TextureDictionary.Load(name);

                Texture2D tex = texDict.GetDiffuse(name).Texture;

                if (outputChunks)
                {
                    string id = name.Substring(5);
                    Texture2D image = new Texture2D(texSize, texSize, TextureFormat.ARGB32, false);

                    for (int xx = 0; xx < texSize; ++xx)
                        for (int yy = 0; yy < texSize; ++yy)
                            image.SetPixel(xx, texSize - yy - 1, tex.GetPixel(xx, yy));

                    image.Apply();

                    File.WriteAllBytes(Path.Combine(folder, string.Format("{0}.jpg", id)), ImageConversion.EncodeToPNG(image));
                }

                for (int ii = 0; ii < texSize; ++ii)
                    for (int jj = 0; jj < texSize; ++jj)
                        mapTexture.SetPixel(x + ii, texSize - (y + jj) - 1, tex.GetPixel(ii, jj));
            }

            Debug.Log("Finished merging minimap!");
            mapTexture.Apply();
            mapSprite = Sprite.Create(mapTexture, new Rect(0, 0, mapTexture.width, mapTexture.height), new Vector2(mapTexture.width, mapTexture.height) / 2);

            if (outputImage)
                File.WriteAllBytes(Path.Combine(Application.streamingAssetsPath, "gta-map.png"), mapTexture.EncodeToPNG());

            circleMask = Resources.Load<Sprite>("Sprites/MapCircle");

            huds = TextureDictionary.Load("hud");
            northBlip = huds.GetDiffuse("radar_north").Texture;
            playerBlip = huds.GetDiffuse("radar_centre").Texture;
            waypointTexture = huds.GetDiffuse("radar_waypoint").Texture;

            Debug.Log("Finished loading minimap textures!");
        }

        // --------------------------------

        #region Private fields

        // Texture & control flags
		private Ped m_ped => Ped.Instance;

        private PlayerController playerController => PlayerController.Instance;

        private TextureDictionary huds;

        private Texture2D northBlip, playerBlip, waypointTexture, mapTexture;
        private Sprite mapSprite, circleMask;

        private Transform northPivot;

        // Flags
        private bool enableMinimap, isReady, isSetup;

        // Zoom vars
        public float curZoomPercentage;

        private float lastZoom, lastLerpedZoom, lerpedZoomCounter;

        private int zoomSelector = 2;

        private Coroutine zoomCoroutine;

        // Toggle flags
        private bool toggleInfo = true;

        // GUI Elements
        private Texture2D blackPixel, seaPixel;

        private float fAlpha = 1;
        private bool showZoomPanel;

        private Vector2 mapScroll,
            screenCenter,
            screenDims,
            //baseMapRect,
            windowSize,
            //constantMapRect,
            mapUpperLeftCorner,
            mapRect,
            mapMousePosition,
            mapZoomPos; //, baseMapSize;

        private const float mapMaxScale = 1f,
                            mapMinScale = .25f,
                            constZoom = 1.25f;

        private float curZoom = 1;

        //private float mapScale = mapMaxScale / 1.5f,
        //              baseScale;

        #endregion Private fields

        private void Setup()
        {
            loadTextures();

            if (canvas != null && canvas.enabled)
                canvas.enabled = false;

            if (iconCanvas != null && iconCanvas.enabled)
                iconCanvas.enabled = false;

            if (outlineCanvas != null && outlineCanvas.enabled)
                outlineCanvas.enabled = false;

            blackPixel = new Texture2D(1, 1);
            blackPixel.SetPixel(0, 0, new Color(0, 0, 0, .5f));
            blackPixel.Apply();

            seaPixel = new Texture2D(1, 1);
            seaPixel.SetPixel(0, 0, new Color(.45f, .54f, .678f));
            seaPixel.Apply();

            isSetup = true;
            isReady = true;
            Debug.Log("Finished minimap setup!");
        }

        private void Awake()
        {
            Instance = this;

            if (!isReady)
                return;

            if (!isSetup)
                Setup();
        }

        private void Update()
        {
            if (!isReady) return;

            if (!enableMinimap)
            {
                Debug.Log("Starting to enable minimap!");

                string error = "{0} is null or disabled! (Please, keep it active!)";

                if (canvas != null && !canvas.enabled)
                    canvas.enabled = true;
                else
                    Debug.LogErrorFormat(error, "Canvas");

                if (iconCanvas != null && !iconCanvas.enabled)
                    iconCanvas.enabled = true;
                else
                    Debug.LogErrorFormat(error, "IconCanvas");

                if (outlineCanvas != null && !outlineCanvas.enabled)
                    outlineCanvas.enabled = true;
                else
                    Debug.LogErrorFormat(error, "OutlineCanvas");

                if (northBlip != null && northImage != null)
                    northImage.sprite = Sprite.Create(northBlip, new Rect(0, 0, northBlip.width, northBlip.height), new Vector2(northBlip.width, northBlip.height) / 2);
                else
                    Debug.LogErrorFormat(error, "NorthImage");

                if (playerBlip != null && playerImage != null)
                    playerImage.sprite = Sprite.Create(playerBlip, new Rect(0, 0, playerBlip.width, playerBlip.height), new Vector2(playerBlip.width, playerBlip.height) / 2);
                else
                    Debug.LogErrorFormat(error, "PlayerImage");

                if (mapImage != null)
                    mapImage.sprite = mapSprite;

                if (maskImage != null && maskImage.sprite == null)
                    maskImage.sprite = circleMask;

                if (mapContainer != null)
                    mapContainer.sizeDelta = new Vector2(uiSize, uiSize);
                else
                    Debug.LogErrorFormat(error, "MapContainer");

                if (maskTransform == null)
                    Debug.LogErrorFormat(error, "MaskTransform");

                curZoomPercentage = zooms[zoomSelector];

                enableMinimap = true;

                // Must review: For some reason values are Y-axis inverted

                float left = Screen.width - uiSize - uiOffset,
                      top = Screen.height - uiSize - uiOffset * 2;

                Vector3 globalPos = new Vector3(left, top, 0) / 2;

                if (maskTransform != null)
                    maskTransform.localPosition = globalPos;

                if (playerImage != null)
                {
                    playerImage.rectTransform.localPosition = globalPos;
                    playerImage.rectTransform.localScale = Vector3.one * .2f;
                    playerImage.rectTransform.localRotation = Quaternion.Euler(0, 0, 180);
                }

                if (northImage != null)
                {
                    northPivot = northImage.rectTransform.parent;

                    northImage.rectTransform.localPosition = new Vector3(0, uiSize / 2, 0) / .2f;
                    northImage.rectTransform.localRotation = Quaternion.Euler(0, 180, 0);
                }

                if (northPivot != null)
                {
                    northPivot.localPosition = globalPos;
                    northPivot.localScale = Vector3.one * .2f;
                }

                if (outlineImage != null)
                {
                    outlineImage.rectTransform.localPosition = globalPos;
                    outlineImage.rectTransform.sizeDelta = Vector2.one * uiSize;
                    outlineImage.rectTransform.localScale = Vector3.one * 1.05f;
                }

                //baseScale = Screen.width / (float)mapTexture.width;
                screenCenter = new Vector2(Screen.width, Screen.height) / 2;
                screenDims = screenCenter * 2;
                windowSize = new Vector2(Screen.width - 120, Screen.height - 120);
                //baseMapRect = new Vector2(mapTexture.width, mapTexture.height) * (baseScale * (mapScale / mapMaxScale) * 2);
                // This value is constant
                //constantMapRect = new Vector2(mapTexture.width, mapTexture.height) * (baseScale * 2);
                mapUpperLeftCorner = Vector2.one * 60;

                Debug.Log("Minimap started!");
            }

            if (Input.GetKeyDown(KeyCode.N))
                ++zoomSelector;
            else if (Input.GetKeyDown(KeyCode.B))
                --zoomSelector;

            if (Input.GetKeyDown(KeyCode.N) || Input.GetKeyDown(KeyCode.B))
            {
                if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
                zoomCoroutine = StartCoroutine(ChangeZoom(Input.GetKeyDown(KeyCode.N)));
            }

            if (Input.GetKeyDown(KeyCode.F8))
                toggleInfo = !toggleInfo;

            //if (Input.GetKeyDown(KeyCode.M))
            //    toggleMap = !toggleMap;

            float mousePosY = screenDims.y - Input.mousePosition.y;
            Vector2 movement = Vector2.zero, // WIP : + offset
                    centerOffset = new Vector2(Mathf.Lerp(0, 1, Mathf.Clamp01((Input.mousePosition.x - mapUpperLeftCorner.x) / windowSize.x)), Mathf.Lerp(0, 1, Mathf.Clamp01((mousePosY - mapUpperLeftCorner.y) / windowSize.y)));

            /*if (Input.mousePosition.x < mapUpperLeftCorner.x)
                centerOffset.x = 0;
            else if (Input.mousePosition.x > windowSize.x)
                centerOffset.x = 1;

            if (mousePosY < mapUpperLeftCorner.y)
                centerOffset.y = 0;
            else if (mousePosY > windowSize.y)
                centerOffset.y = 1;*/

            bool isScrolling = false;

            Vector2 realMousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            mapMousePosition = TransformPosition(new Vector2(Input.mousePosition.x, mousePosY) - mapUpperLeftCorner);

            if (Input.mouseScrollDelta != Vector2.zero)
            {
                //mapScale += Input.mouseScrollDelta.y * Time.fixedDeltaTime * mapZoomScaler;

                // WIP: I want to scroll to mouse position

                if (Input.mouseScrollDelta.y > 0)
                {
                    // WIP: I have to get the current map texture position (like a Raycast) and set the the center with this
                    //mapScroll.x += centerOffset.x * mapMovement * 5;
                    //mapScroll.y += centerOffset.y * mapMovement * 5;

                    //mapZoomPos = mapMousePosition - windowSize / 2; //Vector2.Lerp(windowSize / 2 - baseMapRect / 2, mapMousePosition - windowSize / 2, curZoomPercentage);
                    curZoom *= constZoom;
                    mapZoomPos = realMousePos - constZoom * (realMousePos - mapZoomPos);
                }

                curZoom = Mathf.Clamp(curZoom, mapMinScale, mapMaxScale);

                isScrolling = true;
            }

            //Debug.LogFormat("Zoom Center: {0}, Mouse Pos: {1}; Map Scroll: {2}", centerOffset * mapRect, new Vector2(Input.mousePosition.x, mousePosY), mapScroll);

            if (!isScrolling)
            {
                if (Input.GetMouseButton(2))
                {
                    movement.x = centerOffset.x * mapMovement;
                    movement.y = centerOffset.y * mapMovement;
                }
                else
                {
                    movement.x = Input.GetAxis("Horizontal") * mapMovement;
                    movement.y = Input.GetAxis("Vertical") * mapMovement;
                }

                mapScroll.x -= movement.x;
                mapScroll.y += movement.y;
            }
        }

        private Vector2 TransformPosition(Vector2 mousePos)
        {
            //Vector2 realMapScroll = new Vector2(-mapScroll.x, mapScroll.y);
            return mousePos - mapScroll;
        }

        public static Vector2 WorldPosToMapPos(Vector3 worldPos)
        {
            // map center is at (0,0) world coordinates
            // this, for example, means that the left edge of the world is at: -mapEdge / 2.0f

            // adjust world position, so that (0,0) world coordinates are mapped to (0,0) map coordinates
            worldPos += new Vector3(mapEdge / 2.0f, 0, mapEdge / 2.0f);

            float mul = mapSize / (float)mapEdge;
            return new Vector2(worldPos.x * mul, worldPos.z * mul);
        }

        public static Vector3 MapPosToWorldPos(Vector2 mapPos)
        {
            // adjust map position, so that (0,0) map coordinated are mapped to (0,0) world coordinates
            mapPos -= Vector2.one * (mapSize * 0.5f);

            float mul = mapEdge / (float)mapSize;

            return new Vector3(mapPos.x * mul, 0.0f, mapPos.y * mul);
        }

        private void FixedUpdate()
        {
            if (playerController != null && !GameManager.CanPlayerReadInput() && debugActive) return;

            if (playerController != null)
                realZoom = Mathf.Lerp(.9f * scaleConst, 1.3f * scaleConst, 1 - Mathf.Clamp(playerController.CurVelocity, 0, maxVelocity) / maxVelocity) * curZoomPercentage;

            _gTimer += Time.fixedDeltaTime;
            if (_gTimer > 1)
                _gTimer = 0;
        }

        private void LateUpdate()
        {
            if (!isReady) return;
            if (playerController != null && !GameManager.CanPlayerReadInput() && debugActive) return;

            if (mapTransform != null)
            {
                float deltaZoom = realZoom - lastZoom;

                mapTransform.localScale = new Vector3(realZoom, realZoom, 1);

                lastZoom = realZoom;
            }

            Vector3 defPos = (new Vector3(pPos.x, pPos.z, 0) * (uiSize / -1000f)) / scaleConst; // Why?
            // calibrator

            if (mapContainer != null)
            {
                // WIP: Make this static to avoid shakering
                float lerpedZoom = realZoom; //Mathf.Lerp(lastLerpedZoom, realZoom, lerpedZoomCounter);

                mapContainer.localPosition = new Vector3(defPos.x * lerpedZoom, defPos.y * lerpedZoom, 1);

                lerpedZoomCounter += Time.deltaTime;

                if (lerpedZoomCounter > 1)
                    lerpedZoomCounter = 0;
            }

            float relAngle = Camera.main != null ? Camera.main.transform.eulerAngles.y : 0f;

            if (maskTransform != null)
                maskTransform.localRotation = Quaternion.Euler(0, 0, relAngle);

            if (northPivot != null)
                northPivot.localRotation = Quaternion.Euler(0, 0, relAngle);

            if (playerImage != null && m_ped != null)
                playerImage.rectTransform.localRotation = Quaternion.Euler(0, 0, relAngle - (m_ped.transform.eulerAngles.y + 180));
        }

        private IEnumerator ChangeZoom(bool isIncreasing)
        {
            showZoomPanel = true;
            fAlpha = 1;

            zoomSelector = GetClampedZoomSelector(zoomSelector);
            float curZoom = zooms[zoomSelector % zooms.Length],
                  lastZoom = zooms[GetClampedZoomSelector(zoomSelector - 1 * (isIncreasing ? 1 : -1)) % zooms.Length];

            float t = 0;
            while (t < zoomDuration)
            {
                curZoomPercentage = Mathf.Lerp(lastZoom, curZoom, t / zoomDuration);
                yield return new WaitForFixedUpdate();
                t += Time.fixedDeltaTime;
                fAlpha -= Time.fixedDeltaTime / zoomDuration;
            }

            showZoomPanel = false;
            zoomCoroutine = null;
        }

        private int GetClampedZoomSelector(int? val = null)
        {
            int zoomVal = val == null ? zoomSelector : val.Value;

            if (zoomVal < 0)
                zoomVal = zooms.Length - 1;

            return zoomVal;
        }

        private void OnGUI()
        {
			if (!Loader.HasLoaded)
				return;

            if (!isReady || !toggleInfo) return;

            if (!toggleMap)
            {
                GUILayout.BeginArea(new Rect(Screen.width - uiSize - 10, uiSize + 20, uiSize, 80));

                GUIStyle style = new GUIStyle("label") { alignment = TextAnchor.MiddleCenter };

                Vector2 labelSize = new Vector2(uiSize, 25);
                Rect labelRect = new Rect(Vector2.zero, labelSize);

				// display player pos

//                GUI.DrawTexture(labelRect, blackPixel);
//                GUI.Label(labelRect,
//                    string.Format("x: {0}, y: {1}, z: {2}", pPos.x.ToString("F2"), pPos.y.ToString("F2"), pPos.z.ToString("F2")),
//                    style);

				// display current zone name

				Rect zoneRect = labelRect; //new Rect(uiSize / 2 - uiSize / (2 * 3), 25, uiSize / 3, 25);

                GUI.DrawTexture(zoneRect, blackPixel);
                GUI.Label(zoneRect, ZoneName, style);

                if (showZoomPanel)
                {
                    Color previousColor = GUI.color;

                    Rect zoomPanel = new Rect(uiSize / 2 - uiSize / (2 * 4), 55, uiSize / 4, 25);

                    GUI.color = new Color(0, 0, 0, fAlpha);

                    GUI.DrawTexture(zoomPanel, blackPixel);

                    GUI.color = new Color(255, 255, 255, fAlpha);

                    GUI.Label(zoomPanel, string.Format("x{0}", curZoomPercentage.ToString("F2")), style);

                    GUI.color = previousColor;
                }

                GUILayout.EndArea();
            }
            else
            {
                mapRect = new Vector2(mapTexture.width, mapTexture.height); //* (baseScale * (mapScale / mapMaxScale) * 2);

                GUI.DrawTexture(new Rect(50, 50, Screen.width - 100, Screen.height - 100), blackPixel);

                GUI.DrawTexture(new Rect(mapUpperLeftCorner, windowSize), seaPixel);

                GUILayout.BeginArea(new Rect(mapUpperLeftCorner, windowSize));

                GUILayout.BeginArea(new Rect(mapScroll, mapRect * curZoom));

                GUI.DrawTexture(new Rect(mapZoomPos, mapRect * curZoom), mapTexture);
                //if (Event.current.type.Equals(EventType.Repaint))
                //GUI.DrawTexture(new Rect(10, 10, 100, 100), mapTexture);

                GUI.DrawTexture(new Rect(Vector2.zero, Vector2.one * 16), blackPixel);

                // WIP: I have to load move cursor
                // WIP: I have to load map bars
                // WIP: I have to load marker
                // WIP: Draw player pointer & undescovered zones
                // + drag & drop

                GUILayout.EndArea();
                GUILayout.EndArea();
            }
        }
    }
}