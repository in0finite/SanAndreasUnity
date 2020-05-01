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

        public static bool toggleMap;

        public Image northImage,
                     outlineImage,
                     maskImage,
                     mapImage;

        public RawImage playerImage;

        public RectTransform mapTransform,
                             maskTransform,
                             mapContainer;

        public Text zoneNameLabel;

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

                    _zName = Zone.GetZoneName(ZoneHelpers.zoneInfoList, playerPos);
                }

                return _zName;
            }
        }

        public Texture2D NorthBlip { get { return this.northBlip; } }
        public Texture2D PlayerBlip { get { return this.playerBlip; } }
        public Texture2D WaypointTexture { get { return this.waypointTexture; } }
        public Texture2D VehicleTexture => this.vehicleTexture;
        public Texture2D GreenHouseTexture { get; private set; }
        public Texture2D MapTexture { get { return this.mapTexture; } }

        public Texture2D BlackPixel { get { return this.blackPixel; } }
        public Texture2D SeaPixel { get { return this.seaPixel; } }

        #endregion "Properties"

        public static void AssingMinimap()
        {
            if (!Instance.isSetup)
                Instance.Setup();
        }

        private void loadTextures()
        {
            mapTexture = new Texture2D(mapSize, mapSize, TextureFormat.ARGB32, false, true);

            TextureLoadParams textureLoadParams = new TextureLoadParams(){makeNoLongerReadable = false};
            
            for (int i = 0; i < tileCount; i++)
            {
                // Offset
                int y = ((i / tileEdge) + 1) * texSize,
                    x = (i % tileEdge) * texSize;

                string name = "radar" + ((i < 10) ? "0" : "") + i;
                var texDict = TextureDictionary.Load(name);

                Texture2D tex = texDict.GetDiffuse(name, textureLoadParams).Texture;

                for (int ii = 0; ii < texSize; ++ii)
                    for (int jj = 0; jj < texSize; ++jj)
                        mapTexture.SetPixel(x + ii, texSize - (y + jj) - 1, tex.GetPixel(ii, jj));

                // unload the texture (don't destroy it, because it can be a dummy texture)

            }

            mapTexture.Apply(false, true);
            
            mapSprite = Sprite.Create(mapTexture, new Rect(0, 0, mapTexture.width, mapTexture.height), new Vector2(mapTexture.width, mapTexture.height) / 2);

            circleMask = Resources.Load<Sprite>("Sprites/MapCircle");

            huds = TextureDictionary.Load("hud");
            northBlip = huds.GetDiffuse("radar_north").Texture;
            playerBlip = huds.GetDiffuse("radar_centre").Texture;
            waypointTexture = huds.GetDiffuse("radar_waypoint").Texture;
            vehicleTexture = huds.GetDiffuse("radar_impound").Texture;
            GreenHouseTexture = huds.GetDiffuse("radar_propertyG").Texture;

        }

        // --------------------------------

        #region Private fields

		private Ped m_ped => Ped.Instance;

        private PlayerController playerController => PlayerController.Instance;

        private TextureDictionary huds;

        private Texture2D northBlip, playerBlip, waypointTexture, vehicleTexture, mapTexture;
        private Sprite mapSprite, circleMask;

        public RectTransform northPivot;

        // Flags
        private bool enabledMinimap, isReady, isSetup;

        // Zoom vars
        public float curZoomPercentage;

        private float lastZoom;

        private int zoomSelector = 2;

        private Coroutine zoomCoroutine;

        // Toggle flags
        private bool toggleInfo = true;

        // GUI Elements
        private Texture2D blackPixel, seaPixel;

        private float fAlpha = 1;
        private bool showZoomPanel;

        private float curZoom = 1;

        #endregion Private fields

        private void Setup()
        {
            loadTextures();

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

            if (!enabledMinimap)
            {
                enabledMinimap = true;

                Debug.Log("Starting to enable minimap!");

                northImage.sprite = Sprite.Create(northBlip, new Rect(0, 0, northBlip.width, northBlip.height), new Vector2(northBlip.width, northBlip.height) / 2);
                playerImage.texture = this.PlayerBlip;
                mapImage.sprite = mapSprite;
                if (maskImage.sprite == null)
                    maskImage.sprite = circleMask;
                
                curZoomPercentage = zooms[zoomSelector];



                /*
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
                */



                Debug.Log("Minimap started!");
            }

            if (Input.GetKeyDown(KeyCode.N) && GameManager.CanPlayerReadInput())
                ++zoomSelector;
            else if (Input.GetKeyDown(KeyCode.B) && GameManager.CanPlayerReadInput())
                --zoomSelector;

            if ((Input.GetKeyDown(KeyCode.N) || Input.GetKeyDown(KeyCode.B)) && GameManager.CanPlayerReadInput())
            {
                if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
                zoomCoroutine = StartCoroutine(ChangeZoom(Input.GetKeyDown(KeyCode.N)));
            }

            if (Input.GetKeyDown(KeyCode.F8) && GameManager.CanPlayerReadInput())
                toggleInfo = !toggleInfo;



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

            
            //Vector3 defPos = (new Vector3(pPos.x, pPos.z, 0) * (uiSize / -1000f)) / scaleConst; // Why?

            //if (mapContainer != null)
            //{

            //    mapContainer.localPosition = new Vector3(defPos.x * 1f, defPos.y * 1f, 1);

            //    lerpedZoomCounter += Time.deltaTime;

            //    if (lerpedZoomCounter > 1)
            //        lerpedZoomCounter = 0;
            //}


            Vector3 focusPos = pPos;
            int worldSize = mapSize * 4;
            Vector3 mapPos = - new Vector3(focusPos.x, focusPos.z, 0f) * mapSize / (float)worldSize;
            mapImage.rectTransform.localPosition = mapPos;


            float relAngle = Camera.main != null ? Camera.main.transform.eulerAngles.y : 0f;

            //mapContainer.pivot = new Vector2(mapPos.x, mapPos.y);
            mapContainer.localRotation = Quaternion.Euler(0, 0, relAngle);

            mapContainer.localScale = new Vector3(realZoom, realZoom, 1);
            lastZoom = realZoom;

            if (northPivot != null)
                northPivot.localRotation = Quaternion.Euler(0, 0, relAngle);

            if (playerImage != null && m_ped != null)
                playerImage.rectTransform.localRotation = Quaternion.Euler(0, 0, relAngle - (m_ped.transform.eulerAngles.y + 180));

            // update zone name label

            string currentZoneName = this.ZoneName;
            if (currentZoneName != this.zoneNameLabel.text)
                this.zoneNameLabel.text = currentZoneName;

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

				// display current zone name


                if (showZoomPanel)
                {

                    //string.Format("x{0}", curZoomPercentage.ToString("F2"))

                }

            }
            
        }
    }
}