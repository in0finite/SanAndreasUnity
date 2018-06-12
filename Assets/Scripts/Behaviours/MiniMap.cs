using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace SanAndreasUnity.Behaviours
{
    /*[RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasRenderer))]
    [RequireComponent(typeof(ScrollRect))]
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Mask))]*/

    public class MiniMap : MonoBehaviour
    {
        private const int tileEdge = 12; // width/height of map in tiles
        private const int tileCount = tileEdge * tileEdge; // number of tiles
        private const int mapEdge = 6000; // width/height of map in world coordinates
        private const int texSize = 128; // width/height of single tile in px
        private const int mapSize = tileEdge * texSize; // width/height of whole map in px
        private const int uiSize = 256, uiOffset = 10;

        //private static TextureDictionary[] tiles = new TextureDictionary[tileCount];
        private TextureDictionary huds;

        private Texture2D northBlip, playerBlip, mapTexture;
        private Sprite mapSprite, circleMask;
        private bool enableMinimap;

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

            if (minimap.GetComponent<MiniMap>() == null)
                minimap.AddComponent<MiniMap>();
        }

        private void loadTextures()
        {
            mapTexture = new Texture2D(mapSize, mapSize);

            Debug.Log("Merging all map sprites into one sprite.");
            for (int i = 0; i < tileCount; i++)
            {
                // Offset
                int x = (i / tileEdge) * texSize,
                    y = (i % tileEdge) * texSize;

                string name = "radar" + ((i < 10) ? "0" : "") + i;
                var texDict = TextureDictionary.Load(name);

                Texture2D tex = texDict.GetDiffuse(name).Texture;
                for (int ii = 0; ii < texSize; ++ii)
                    for (int jj = 0; jj < texSize; ++jj)
                        mapTexture.SetPixel(x + ii, y + jj, tex.GetPixel(ii, jj));

                //tex.filterMode = FilterMode.Point;

                //tiles[i] = texDict;
            }

            mapTexture.Apply();
            mapSprite = Sprite.Create(mapTexture, new Rect(0, 0, mapTexture.width, mapTexture.height), new Vector2(mapTexture.width, mapTexture.height) / 2);

            circleMask = Resources.Load<Sprite>("Sprites/MapCircle");

            huds = TextureDictionary.Load("hud");
            northBlip = huds.GetDiffuse("radar_north").Texture;
            playerBlip = huds.GetDiffuse("radar_centre").Texture;

            //Debug.Log(new Vector2(playerBlip.width, playerBlip.height));
        }

        /*private static Texture2D getTile(int i)
        {
            if ((i < 0) || (i >= tileCount))
                return null;

            if (!Loader.HasLoaded)
                return null;

            return tiles[i].GetDiffuse("radar" + ((i < 10) ? "0" : "") + i).Texture;
        }

        private static void drawTexturePart(Texture2D tex, Vector2 pos, Vector2 tileStart, Vector2 tileSize, float zoom = 1)
        {
            GUI.DrawTextureWithTexCoords(new Rect(pos, tileSize), tex,
                new Rect(tileStart.x / (float)tex.width, (tileSize.y - tileStart.y) / (float)tex.height, tileSize.x / (float)tex.width, -tileSize.y / (float)tex.height));
            //GUI.DrawTextureWithTexCoords(new Rect(pos, tileSize * zoom), tex,
            //    new Rect(tileStart.x / ((float)tex.width * zoom), (tileSize.y - tileStart.y) / ((float)tex.height * zoom), tileSize.x / ((float)tex.width * zoom), -tileSize.y / ((float)tex.height * zoom)));
            //GUI.DrawTexture(new Rect(pos, tileSize), tex);
        }

        // Draw a tile at screen position pos with size tileSize, starting with tileSize pixels in the tile at tileStart
        private static void drawTilePart(int tile, Vector2 pos, Vector2 tileStart, Vector2 tileSize, float zoom = 1)
        {
            Texture2D tex = getTile(tile);

            if (tex != null)
                drawTexturePart(tex, pos, tileStart, tileSize, zoom);
        }

        private static Vector2 coordinatesWorldToPixel(Vector2 pos)
        {
            // Convert from world-space (0 - 6000) to map pixel space (0 - 1536)
            return new Vector2(pos.x / (float)mapEdge * (float)mapSize, pos.y / (float)mapEdge * (float)mapSize);
        }

        private static int coordinatesToTileNumber(Vector2 pos)
        {
            int x = (int)pos.x * tileEdge / mapSize;
            int y = (int)pos.y * tileEdge / mapSize;
            int tile = x + (tileEdge * y);
            return tile < tileCount ? tile : tileCount - 1;
        }

        private static Vector2 coordinatesInTile(Vector2 pos)
        {
            if (pos.x >= mapSize)
                pos.x = mapSize - 1;

            if (pos.y >= mapSize)
                pos.y = mapSize - 1;

            return new Vector2(pos.x % texSize, pos.y % texSize);
        }*/

        // --------------------------------

        #region Private fields

        private Player player;
        private PlayerController playerController;
        private Canvas canvas;
        private RectTransform mapTransform, maskTransform;

        #endregion Private fields

        private void Awake()
        {
            loadTextures();

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            player = playerObj.GetComponent<Player>();
            playerController = playerObj.GetComponent<PlayerController>();

            // Start object setup

            //Check if parent is a canvas
            canvas = transform.parent.GetComponent<Canvas>();
            if (canvas != null)
            {
                maskTransform = GetComponent<RectTransform>();
                mapTransform = transform.Find("Image").GetComponent<RectTransform>();

                // Setup mapSprite
                if (GetComponent<Image>().sprite == null)
                    GetComponent<Image>().sprite = circleMask;

                transform.Find("Image").GetComponent<Image>().sprite = mapSprite;
            }
            else
            {
                GameObject canvasObject = new GameObject();
                canvasObject.name = "Canvas";

                canvasObject.AddComponent<RectTransform>();
                canvas = canvasObject.AddComponent<Canvas>();
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();

                transform.parent = canvasObject.transform;

                if (GetComponent<RectTransform>() == null)
                    mapTransform = gameObject.AddComponent<RectTransform>();

                if (GetComponent<CanvasRenderer>() == null)
                    gameObject.AddComponent<CanvasRenderer>();

                if (GetComponent<Image>() == null)
                {
                    Image img = gameObject.AddComponent<Image>();
                    img.sprite = circleMask;
                }

                if (GetComponent<Mask>() == null)
                    gameObject.AddComponent<Mask>();

                if (transform.Find("Image") == null)
                {
                    GameObject image = new GameObject();
                    image.name = "Image";

                    image.transform.parent = transform;

                    mapTransform = image.AddComponent<RectTransform>();
                    image.AddComponent<CanvasRenderer>();

                    Image mapImage = image.AddComponent<Image>();
                    mapImage.sprite = mapSprite;
                }
            }

            canvas.enabled = false;
            maskTransform.position = new Vector3(Screen.width - uiSize - uiOffset, Screen.height - uiSize - uiOffset);

            maskTransform.localScale = new Vector3(uiSize, uiSize, 1);
            mapTransform.localScale = new Vector3(1f / uiSize, 1f / uiSize, 1);
        }

        /*private void drawMapWindow(Vector2 screenPos, Vector2 mapPos)
        {
            float zoom = 1,
                  rTexSize = texSize; // WIP: There is a problem when you increase the size (because of the tiling)

            // Draw current map tile
            Vector2 pxPos = coordinatesWorldToPixel(mapPos),
            // Draw player blip
                    tilePos = coordinatesInTile(pxPos),
                    texSizeDim = new Vector2(rTexSize, rTexSize),
                    playerBlipDim = new Vector2(playerBlip.width, playerBlip.height);

            GUI.BeginGroup(GUIUtils.GetCornerRect(ScreenCorner.BottomLeft, texSizeDim * zoom, new Vector2(5, 5)));

            //Vector2 rot = Vector2.zero;
            //Matrix4x4 matrixBackup = GUI.matrix;

            for (int i = -1; i <= 1; ++i)
                for (int j = -1; j <= 1; ++j)
                {
                    Vector2 v = new Vector2(rTexSize * i, rTexSize * j),
                            pos = screenPos - tilePos + v + texSizeDim / 2 - playerBlipDim / 2;

                    // WIP: I have to rotate the tiles
                    //GUIUtility.RotateAroundPivot(player.transform.rotation.eulerAngles.y, rot);

                    int ii = coordinatesToTileNumber(pxPos + v);

                    // - playerBlipDim / 2
                    drawTilePart(ii, pos, Vector2.zero, texSizeDim, zoom);
                }

            //GUI.matrix = matrixBackup;

            //GUI.Label(new Rect(Screen.width / 2 - 500 / 2, 5, 500, 100), string.Format("PxPos: {0}\nTilePos: {1}\nScreenPos: {2}", pxPos, tilePos, screenPos), new GUIStyle("label") { alignment = TextAnchor.MiddleCenter });

            Matrix4x4 matrixBackup = GUI.matrix;
            Vector2 rot = screenPos + (texSizeDim * zoom) / 2 - playerBlipDim / 2;

            GUIUtility.RotateAroundPivot(player.transform.rotation.eulerAngles.y, rot);
            drawTexturePart(playerBlip, rot - playerBlipDim / 2, Vector2.zero, playerBlipDim);
            GUI.matrix = matrixBackup;

            // Draw 'N' north marker at top of map
            drawTexturePart(northBlip, new Vector2(screenPos.x + ((rTexSize - northBlip.width) / 2), screenPos.y - (northBlip.height / 2)), Vector2.zero, new Vector2(northBlip.width, northBlip.height));

            GUI.EndGroup();
        }*/

        private void Update()
        {
            if (!Loader.HasLoaded) return;
            if (!playerController.CursorLocked) return;

            if (!enableMinimap)
            {
                canvas.enabled = true;
                enableMinimap = true;
            }

            // Player coordinates on map, (0; 0) moved from center to top-left
            //Vector2 pos = new Vector2(player.transform.position.x + (mapEdge / 2), mapEdge - (player.transform.position.z + (mapEdge / 2)));
            //drawMapWindow(new Vector2(10, 10), pos);
        }
    }
}