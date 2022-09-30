using SanAndreasUnity.Importing.Conversion;
using UGameCore.Utilities;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SanAndreasUnity.Behaviours
{
    public class MiniMap : StartupSingleton<MiniMap>
    {
        public const int tileEdge = 12; // width/height of map in tiles
        public const int tileCount = tileEdge * tileEdge; // number of tiles
        public const int mapEdge = 6000; // width/height of map in world coordinates
        public const int texSize = 128; // width/height of single tile in px
        public const int mapSize = tileEdge * texSize; // width/height of whole map in px
        public const int uiSize = 256, uiOffset = 10;

        public static MiniMap Instance => Singleton;

        public Image northImage,
                     outlineImage,
                     maskImage;

        public RawImage mapImage, playerImage;

        public RectTransform mapTransform,
                             maskTransform,
                             mapContainer,
                             northPivot;

        public Text zoneNameLabel;

        private Canvas _canvas;

        private const float maxVelocityForZooming = 300f;
        public static readonly float[] availableZooms = new float[] { .5f, .75f, 1f, 1.2f, 1.4f, 1.6f, 2f, 2.5f, 3f, 5f };

        public float zoom = 1.3f;
        public float zoomDuration = 1;
        private float curZoomPercentage;
        public int zoomSelector = 2;
        private Coroutine zoomCoroutine;

        public Vector3 FocusPos { get; set; } = Vector3.zero;

        public bool IsMinimapVisible => _canvas.enabled && this.gameObject.activeInHierarchy;

        private double _timeWhenRetrievedZoneName = 0f;

        private string _lastZoneName = "";

        private string ZoneName
        {
            get
            {
                if (Time.timeAsDouble - _timeWhenRetrievedZoneName > 2f)
                {
                    _timeWhenRetrievedZoneName = Time.timeAsDouble;
                    _lastZoneName = Importing.Zone.GetZoneName(this.FocusPos);
                }

                return _lastZoneName;
            }
        }

        public Texture2D NorthBlip { get; private set; }
        public Texture2D PlayerBlip { get; private set; }
        public Texture2D WaypointTexture { get; private set; }
        public Texture2D VehicleTexture { get; private set; }
        public Texture2D GreenHouseTexture { get; private set; }
        public Texture2D MapTexture { get; private set; }

        public Texture2D BlackPixel { get; private set; }
        public Texture2D SeaPixel { get; private set; }



        public void Load()
        {

            LoadGameTextures();

            BlackPixel = new Texture2D(1, 1);
            BlackPixel.SetPixel(0, 0, new Color(0, 0, 0, .5f));
            BlackPixel.Apply();

            SeaPixel = new Texture2D(1, 1);
            SeaPixel.SetPixel(0, 0, new Color(.45f, .54f, .678f));
            SeaPixel.Apply();

        }

        private void LoadGameTextures()
        {
            LoadMapTexture();

            var huds = TextureDictionary.Load("hud");
            NorthBlip = huds.GetDiffuse("radar_north").Texture;
            PlayerBlip = huds.GetDiffuse("radar_centre").Texture;
            WaypointTexture = huds.GetDiffuse("radar_waypoint").Texture;
            VehicleTexture = huds.GetDiffuse("radar_impound").Texture;
            GreenHouseTexture = huds.GetDiffuse("radar_propertyG").Texture;

            northImage.sprite = Sprite.Create(NorthBlip, new Rect(0, 0, NorthBlip.width, NorthBlip.height), new Vector2(NorthBlip.width, NorthBlip.height) / 2);
            playerImage.texture = this.PlayerBlip;
            mapImage.texture = MapTexture;

        }

        void LoadMapTexture()
        {
            MapTexture = new Texture2D(mapSize, mapSize, TextureFormat.ARGB32, false, true);

            if (Config.GetBool("skip_minimap_load"))
                return;

            TextureLoadParams textureLoadParams = new TextureLoadParams() { makeNoLongerReadable = false };

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
                        MapTexture.SetPixel(x + ii, texSize - (y + jj) - 1, tex.GetPixel(ii, jj));

                // unload the texture (don't destroy it, because it can be a dummy texture)

            }

            MapTexture.Apply(false, true);
        }

        protected override void OnSingletonAwake()
        {
            _canvas = this.GetComponentInParent<Canvas>();

            curZoomPercentage = availableZooms[zoomSelector];

        }

        private void ReadInput()
        {
            bool zoomIn = Input.GetKeyDown(KeyCode.N);
            bool zoomOut = Input.GetKeyDown(KeyCode.B);

            if (zoomIn)
                ++zoomSelector;
            else if (zoomOut)
                --zoomSelector;

            if (zoomIn || zoomOut)
            {
                if (zoomCoroutine != null)
                    StopCoroutine(zoomCoroutine);
                zoomCoroutine = StartCoroutine(ChangeZoom(zoomIn));
            }

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

        private void LateUpdate()
        {
            if (!this.IsMinimapVisible)
                return;


            // update focus position

            var ped = Ped.Instance;
            if (ped != null)
                this.FocusPos = ped.transform.position;
            else if (Camera.main != null)
                this.FocusPos = Camera.main.transform.position;

            // update zoom based on ped's velocity

            var playerController = PlayerController.Instance;
            if (playerController != null)
                zoom = Mathf.Lerp(.9f, 1.3f, 1 - Mathf.Clamp(playerController.CurVelocity, 0, maxVelocityForZooming) / maxVelocityForZooming) * curZoomPercentage;

            // read input

            if (GameManager.CanPlayerReadInput())
            {
                this.ReadInput();
            }

            // update position of UI

            Vector3 mapPos = - new Vector3(this.FocusPos.x, this.FocusPos.z, 0f) * (mapSize / (float)mapEdge);
            mapImage.rectTransform.localPosition = mapPos;

            // update rotation of UI

            float relAngle = Camera.main != null ? Camera.main.transform.eulerAngles.y : 0f;

            //mapContainer.pivot = new Vector2(mapPos.x, mapPos.y);
            mapContainer.localRotation = Quaternion.Euler(0, 0, relAngle);

            mapContainer.localScale = new Vector3(zoom, zoom, 1);
            
            if (northPivot != null)
                northPivot.localRotation = Quaternion.Euler(0, 0, relAngle);

            if (playerImage != null && ped != null)
                playerImage.rectTransform.localRotation = Quaternion.Euler(0, 0, relAngle - (ped.transform.eulerAngles.y + 180));

            // update zone name label

            string currentZoneName = this.ZoneName;
            if (currentZoneName != this.zoneNameLabel.text)
                this.zoneNameLabel.text = currentZoneName;

        }

        private IEnumerator ChangeZoom(bool isIncreasing)
        {
            float fAlpha = 1;

            zoomSelector = GetClampedZoomSelector(zoomSelector);
            float curZoom = availableZooms[zoomSelector % availableZooms.Length],
                  lastZoom = availableZooms[GetClampedZoomSelector(zoomSelector - 1 * (isIncreasing ? 1 : -1)) % availableZooms.Length];

            float t = 0;
            while (t < zoomDuration)
            {
                curZoomPercentage = Mathf.Lerp(lastZoom, curZoom, t / zoomDuration);
                yield return new WaitForFixedUpdate();
                t += Time.fixedDeltaTime;
                fAlpha -= Time.fixedDeltaTime / zoomDuration;
            }

            zoomCoroutine = null;
        }

        private int GetClampedZoomSelector(int? val = null)
        {
            int zoomVal = val == null ? zoomSelector : val.Value;

            if (zoomVal < 0)
                zoomVal = availableZooms.Length - 1;

            return zoomVal;
        }

    }
}