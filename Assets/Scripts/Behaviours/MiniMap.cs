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
                tiles[i] = TextureDictionary.Load("radar" + ((i < 10) ? "0" : "") + i);
            }

            huds = TextureDictionary.Load("hud");
            northBlip = huds.GetDiffuse("radar_north").Texture;
            playerBlip = huds.GetDiffuse("radar_centre").Texture;
        }

        private static Texture2D getTile(int i)
        {
            if ((i < 0) || (i >= tileCount))
                return null;

            if (!Loader.HasLoaded)
                return null;

            return tiles[i].GetDiffuse("radar" + ((i < 10) ? "0" : "") + i).Texture;
        }

        private static void drawTexturePart(Texture2D tex, Vector2 pos, Vector2 tileStart, Vector2 tileSize)
        {
            GUI.DrawTextureWithTexCoords(new Rect(pos, tileSize), tex,
                new Rect(tileStart.x / (float)tex.width, (tileSize.y - tileStart.y) / (float)tex.height, tileSize.x / (float)tex.width, -tileSize.y / (float)tex.height));
        }

        // Draw a tile at screen position pos with size tileSize, starting with tileSize pixels in the tile at tileStart
        private static void drawTilePart(int tile, Vector2 pos, Vector2 tileStart, Vector2 tileSize)
        {
            Texture2D tex = getTile(tile);
            if (tex != null)
            {
                drawTexturePart(tex, pos, tileStart, tileSize);
            }
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
            {
                pos.x = mapSize - 1;
            }
            if (pos.y >= mapSize)
            {
                pos.y = mapSize - 1;
            }
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
            // Draw current map tile
            Vector2 pxPos = coordinatesWorldToPixel(mapPos);
            drawTilePart(coordinatesToTileNumber(pxPos), screenPos, new Vector2(0, 0), new Vector2(texSize, texSize));

            // Draw player blip
            Vector2 tilePos = coordinatesInTile(pxPos);
            Matrix4x4 matrixBackup = GUI.matrix;
            GUIUtility.RotateAroundPivot(player.transform.rotation.eulerAngles.y, new Vector2(screenPos.x + tilePos.x, screenPos.y + tilePos.y));
            drawTexturePart(playerBlip, new Vector2(screenPos.x + tilePos.x - (playerBlip.width / 2), screenPos.y + tilePos.y - (playerBlip.height / 8)), new Vector2(0, 0), new Vector2(playerBlip.width, playerBlip.height));
            GUI.matrix = matrixBackup;

            // Draw 'N' north marker at top of map
            drawTexturePart(northBlip, new Vector2(screenPos.x + ((texSize - northBlip.width) / 2), screenPos.y - (northBlip.height / 2)), new Vector2(0, 0), new Vector2(northBlip.width, northBlip.height));
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