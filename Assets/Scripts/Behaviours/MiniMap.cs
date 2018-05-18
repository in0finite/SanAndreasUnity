using SanAndreasUnity.Importing.Conversion;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    [RequireComponent(typeof(Player))]
    public class MiniMap : MonoBehaviour
    {
        private const int tileEdge = 12; // width/height of map in tiles
        private const int tileCount = tileEdge * tileEdge; // number of tiles
        private const int mapEdge = 6000; // width/height of map in world coordinates
        private const int texSize = 128; // width/height of single tile in px
        private const int mapSize = tileEdge * texSize; // width/height of whole map in px

        private static TextureDictionary[] tiles = new TextureDictionary[tileCount];
        private static TextureDictionary huds;
        private static Texture2D northBlip;
        private static Texture2D playerBlip;

        public static void loadTextures()
        {
            for (int i = 0; i < tileCount; i++)
            {
                string name = "radar" + ((i < 10) ? "0" : "") + i;
                var texDict = TextureDictionary.Load(name);

                Texture2D tex = texDict.GetDiffuse(name).Texture;
                tex.filterMode = FilterMode.Point;

                tiles[i] = texDict;
            }

            huds = TextureDictionary.Load("hud");
            northBlip = huds.GetDiffuse("radar_north").Texture;
            playerBlip = huds.GetDiffuse("radar_centre").Texture;

            Debug.Log(new Vector2(playerBlip.width, playerBlip.height));
        }

        private static Texture2D getTile(int i)
        {
            if ((i < 0) || (i >= tileCount))
                return null;

            if (!Loader.HasLoaded)
                return null;

            return tiles[i].GetDiffuse("radar" + ((i < 10) ? "0" : "") + i).Texture;
        }

        private static void drawTexturePart(Texture2D tex, Vector2 pos, Vector2 tileStart, Vector2 tileSize, float zoom = 1)
        {
            GUI.DrawTextureWithTexCoords(new Rect(pos, tileSize * zoom), tex,
                new Rect(tileStart.x / ((float)tex.width * zoom), (tileSize.y - tileStart.y) / ((float)tex.height * zoom), tileSize.x / ((float)tex.width * zoom), -tileSize.y / ((float)tex.height * zoom)));
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
        }

        // --------------------------------

        #region Private fields

        private Player player;
        private PlayerController playerController;

        #endregion Private fields

        private void Awake()
        {
            player = GetComponent<Player>();
            playerController = GetComponent<PlayerController>();
        }

        private void drawMapWindow(Vector2 screenPos, Vector2 mapPos)
        {
            float zoom = 1,
                  rTexSize = texSize; // There is a problem when you increase the size (because of the tiling)

            // Draw current map tile
            Vector2 pxPos = coordinatesWorldToPixel(mapPos),
            // Draw player blip
                    tilePos = coordinatesInTile(pxPos),
                    texSizeDim = new Vector2(rTexSize, rTexSize),
                    playerBlipDim = new Vector2(playerBlip.width, playerBlip.height);

            GUI.BeginGroup(GUIUtils.GetCornerRect(ScreenCorner.BottomLeft, texSizeDim * zoom, new Vector2(5, 5)));

            Vector2 rot = Vector2.zero;
            Matrix4x4 matrixBackup = GUI.matrix;

            for (int i = -1; i <= 1; ++i)
                for (int j = -1; j <= 1; ++j)
                {
                    Vector2 v = new Vector2(rTexSize * i, rTexSize * j);
                    rot = screenPos - tilePos + v + texSizeDim / 2 - playerBlipDim / 2;

                    //GUIUtility.RotateAroundPivot(player.transform.rotation.eulerAngles.y, rot);

                    int ii = coordinatesToTileNumber(pxPos + v);

                    drawTilePart(ii, rot - playerBlipDim / 2, Vector2.zero, texSizeDim, zoom);
                    //Debug.Log(string.Format("{0}: {1}", ii, screenPos - tilePos + v));
                }

            GUI.matrix = matrixBackup;

            //GUI.Label(new Rect(Screen.width / 2 - 500 / 2, 5, 500, 100), string.Format("PxPos: {0}\nTilePos: {1}\nScreenPos: {2}", pxPos, tilePos, screenPos), new GUIStyle("label") { alignment = TextAnchor.MiddleCenter });

            //Debug.Break();

            matrixBackup = GUI.matrix;
            rot = screenPos + (texSizeDim * zoom) / 2 + playerBlipDim / 2;

            GUIUtility.RotateAroundPivot(player.transform.rotation.eulerAngles.y, rot);
            drawTexturePart(playerBlip, rot - playerBlipDim / 2, Vector2.zero, playerBlipDim);
            GUI.matrix = matrixBackup;

            // Draw 'N' north marker at top of map
            drawTexturePart(northBlip, new Vector2(screenPos.x + ((rTexSize - northBlip.width) / 2), screenPos.y - (northBlip.height / 2)), Vector2.zero, new Vector2(northBlip.width, northBlip.height));

            GUI.EndGroup();
        }

        private void OnGUI()
        {
            if (!Loader.HasLoaded) return;
            if (!playerController.CursorLocked) return;

            // Player coordinates on map, (0; 0) moved from center to top-left
            Vector2 pos = new Vector2(player.transform.position.x + (mapEdge / 2), mapEdge - (player.transform.position.z + (mapEdge / 2)));
            drawMapWindow(new Vector2(10, 10), pos);
        }
    }
}