using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Importing.Conversion;
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
        private const int tileEdge = 12; // width/height of map in tiles
        private const int tileCount = tileEdge * tileEdge; // number of tiles
        private const int mapEdge = 6000; // width/height of map in world coordinates
        private const int texSize = 128; // width/height of single tile in px
        private const int mapSize = tileEdge * texSize; // width/height of whole map in px
        private const int uiSize = 256, uiOffset = 10;
        private const bool outputChunks = false, outputImage = true;

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

        public float zoomDuration = 1;

        public bool debugActive = true;

        #region "Properties"

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
                return player.transform.position;
            }
        }

        private int _vCount = 0;
        private float _vTimer;

        private int VehicleCount
        {
            get
            {
                if (_vTimer == 0)
                    _vCount = Object.FindObjectsOfType<Vehicle>().Length;

                return _vCount;
            }
        }

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
            //mapTexture.wrapMode = TextureWrapMode.Repeat;
            //mapTexture.filterMode = FilterMode.Point;

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

            Debug.Log("Finished loading minimap textures!");
        }

        // --------------------------------

        #region Private fields

        // Texture & control flags
        private Player player;

        private PlayerController playerController;

        private TextureDictionary huds;

        private Texture2D northBlip, playerBlip, mapTexture;
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
        private Texture2D blackPixel;

        #endregion Private fields

        private void Setup()
        {
            loadTextures();

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            player = playerObj.GetComponent<Player>();
            playerController = playerObj.GetComponent<PlayerController>();

            if (canvas != null && canvas.enabled)
                canvas.enabled = false;

            if (iconCanvas != null && iconCanvas.enabled)
                iconCanvas.enabled = false;

            if (outlineCanvas != null && outlineCanvas.enabled)
                outlineCanvas.enabled = false;

            blackPixel = new Texture2D(1, 1);
            blackPixel.SetPixel(0, 0, new Color(0, 0, 0, .5f));
            blackPixel.Apply();

            isSetup = true;
            isReady = true;
            Debug.Log("Finished minimap setup!");
        }

        private void Awake()
        {
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

                Debug.Log("Minimap started!");

                //Debug.Log("Lossy scale: " + mapTransform.lossyScale);
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
        }

        private void FixedUpdate()
        {
            if (playerController != null && !playerController.CursorLocked && debugActive) return;

            if (playerController != null)
                realZoom = Mathf.Lerp(.9f * scaleConst, 1.3f * scaleConst, 1 - Mathf.Clamp(playerController.CurVelocity, 0, maxVelocity) / maxVelocity) * curZoomPercentage;

            _vTimer += Time.fixedDeltaTime;
            if (_vTimer > 1)
                _vTimer = 0;
        }

        private void LateUpdate()
        {
            if (!isReady) return;
            if (playerController != null && !playerController.CursorLocked && debugActive) return;

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

            float relAngle = Camera.main.transform.eulerAngles.y;

            if (maskTransform != null)
                maskTransform.localRotation = Quaternion.Euler(0, 0, relAngle);

            if (northPivot != null)
                northPivot.localRotation = Quaternion.Euler(0, 0, relAngle);

            if (playerImage != null)
                playerImage.rectTransform.localRotation = Quaternion.Euler(0, 0, relAngle - (player.transform.eulerAngles.y + 180));
        }

        private IEnumerator ChangeZoom(bool isIncreasing)
        {
            zoomSelector = GetClampedZoomSelector(zoomSelector);
            float curZoom = zooms[zoomSelector % zooms.Length],
                  lastZoom = zooms[GetClampedZoomSelector(zoomSelector - 1 * (isIncreasing ? 1 : -1)) % zooms.Length];

            float t = 0;
            while (t < zoomDuration)
            {
                curZoomPercentage = Mathf.Lerp(lastZoom, curZoom, t / zoomDuration);
                yield return new WaitForFixedUpdate();
                t += Time.fixedDeltaTime;
            }

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
            if (!toggleInfo) return;

            GUILayout.BeginArea(new Rect(Screen.width - uiSize - 10, uiSize + 20, uiSize, 50));

            Vector2 labelSize = new Vector2(uiSize, 25);
            Rect labelRect = new Rect(Vector2.zero, labelSize);

            GUI.DrawTexture(labelRect, blackPixel);
            GUI.Label(labelRect,
                string.Format("x: {0}, y: {1}, z: {2} ({3})", pPos.x.ToString("F2"), pPos.y.ToString("F2"), pPos.z.ToString("F2"), VehicleCount),
                new GUIStyle("label") { alignment = TextAnchor.MiddleCenter });

            GUILayout.EndArea();
        }
    }
}