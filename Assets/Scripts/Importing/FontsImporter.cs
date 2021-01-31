using SanAndreasUnity.Importing.Conversion;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#pragma warning disable 618

namespace SanAndreasUnity.Importing
{

    public static class FontsImporter
    {
        private static List<Font> _loadedFonts = new List<Font>();

        public static Font GetFont(int font)
        {
            return _loadedFonts.ElementAtOrDefault(font);
        }

        public static void LoadFonts()
        {
            for (int i = 1; i < 3; i++)
            {
                Texture2D fontTexture = TextureDictionary.Load("fonts").GetDiffuse($"font{i}", new TextureLoadParams() {makeNoLongerReadable = false}).Texture;
                Texture2D flippedTexture = FlipTexture(fontTexture);

                Font font = Resources.Load<Font>($"font{i}");

                font.material = new Material(Shader.Find("Standard"))
                {
                    mainTexture = flippedTexture
                };

                CharacterInfo[] characterInfo = new CharacterInfo[94];

                float xSpacing = 32;
                float ySpacing = 40;

                float x = 1;
                float y = 480;

                float letterSize = 30f;

                Rect vertRect = new Rect(0, 10, 10, -10);

                //whitespace character
                characterInfo[0].index = 32;
                characterInfo[0].advance = 7;
                characterInfo[0].vert = vertRect;
                characterInfo[0].uv = new Rect(0, 0, letterSize / flippedTexture.width, letterSize / flippedTexture.height);

                for (int j = 1; j < characterInfo.Length; j++)
                {
                    characterInfo[j].index = 32 + j;
                    characterInfo[j].advance = GetAdvanceForIndex(i, characterInfo[j].index);
                    characterInfo[j].vert = vertRect;
                    if (i == 1)
                    {
                        characterInfo[j].uv = new Rect((x * xSpacing) / flippedTexture.width, (y - 4) / flippedTexture.height, letterSize / flippedTexture.width, letterSize / flippedTexture.height);
                    }
                    else
                    {
                        characterInfo[j].uv = new Rect((x * xSpacing) / flippedTexture.width, (y - 4) / flippedTexture.height, letterSize / flippedTexture.width, (letterSize + 5) / flippedTexture.height);
                    }

                    x++;
                    if (x >= 16)
                    {
                        x = 0;
                        y -= ySpacing;
                    }
                }

                font.characterInfo = characterInfo;
                _loadedFonts.Add(font);

                Font secondaryFont = Resources.Load<Font>($"font{i + 2}");
                secondaryFont.material = font.material;

                x = 0;
                y = 120;

                int index = 0;
                bool skip = false;
                for (int j = 1; j < 38; j++)
                {
                    if (skip)
                    {
                        characterInfo[j].index = 96 + index;
                    }
                    else
                    {
                        characterInfo[j].index = 48 + index;
                    }

                    characterInfo[j].advance = GetAdvanceForIndexSecondary(i, characterInfo[j].index);
                    characterInfo[j].vert = vertRect;
                    characterInfo[j].uv = new Rect((x * xSpacing) / flippedTexture.width, (y - 7) / flippedTexture.height, letterSize / flippedTexture.width, (letterSize + 7) / flippedTexture.height);

                    x++;
                    if (x >= 16)
                    {
                        x = 0;
                        y -= ySpacing;
                    }

                    index++;
                    if (index == 10 && !skip)
                    {
                        skip = true;
                        index = 0;
                    }
                }

                index = 0;

                x = 10;
                y = 120;
                //assign capital letters
                for (int j = 38; j < 65; j++)
                {
                    characterInfo[j].index = 64 + index;
                    characterInfo[j].advance = GetAdvanceForIndexSecondary(i, characterInfo[j].index);
                    characterInfo[j].vert = vertRect;
                    characterInfo[j].uv = new Rect((x * xSpacing) / flippedTexture.width, (y - 7) / flippedTexture.height, letterSize / flippedTexture.width, (letterSize + 7) / flippedTexture.height);

                    x++;
                    if (x >= 16)
                    {
                        x = 0;
                        y -= ySpacing;
                    }

                    index++;
                }

                secondaryFont.characterInfo = characterInfo;
                _loadedFonts.Add(secondaryFont);
            }
        }

        private static int GetAdvanceForIndexSecondary(int font, int index)
        {
            if (font == 1)
            {
                switch (index)
                {
                    // 73 I
                    case 73:
                    // 105 i
                    case 105: return 3;

                    // 77 M
                    case 77:
                    // 109 m
                    case 109:

                    //87 W
                    case 87:
                    //119 w
                    case 119: return 10;

                    //76 L
                    case 76:
                    //108 l
                    case 108: return 5;

                    default: return 7;
                }
            }
            else
            {
                switch (index)
                {
                    // 73 I
                    case 73:
                    // 105 i
                    case 105: return 3;

                    // 77 M
                    case 77:
                    // 109 m
                    case 109:

                    //87 W
                    case 87:
                    //119 w
                    case 119: return 10;

                    //76 L
                    case 76:
                    //108 l
                    case 108: return 7;

                    default: return 8;
                }
            }
        }

        private static int GetAdvanceForIndex(int font, int index)
        {
            if (font == 1)
            {
                if (char.IsUpper((char) index))
                {
                    switch (index)
                    {
                        //78 N
                        case 78:
                        //65 A
                        case 65: return 8;

                        // 73 I
                        case 73:
                            return 4;

                        //79 O
                        case 79:
                        //81 Q
                        case 81: return 9;

                        //77 M
                        case 77:
                        // 87 W
                        case 87: return 10;

                        //80 P
                        case 80:
                        //76 L
                        case 76:
                        //70 F
                        case 70:
                        // 69 E
                        case 69:
                            return 6;

                        //74 J
                        //case 106: return 2;

                        default: return 7;
                    }
                }
                else
                {
                    switch (index)
                    {
                        // 109 m
                        case 109:
                        //119 w
                        case 119: return 10;

                        // 105 i
                        case 105:
                        //106 j
                        case 106: return 3;
                        //102 f
                        case 102:
                        //114 r
                        case 114:
                        //115 s
                        case 115:
                        // 99 c
                        case 99:
                        //116 t
                        case 116:
                            return 5;

                        //108 l
                        case 108: return 4;

                        default: return 7;
                    }
                }
            }
            else
            {
                if (char.IsUpper((char) index))
                {
                    switch (index)
                    {
                        //79 O
                        case 79:
                        //81 Q
                        case 81: return 11;

                        //77 M
                        case 77:
                        // 87 W
                        case 87: return 12;

                        default: return 9;
                    }
                }
                else
                {
                    switch (index)
                    {
                        // 109 m
                        case 109:
                        //119 w
                        case 119: return 7;

                        //120 x
                        case 115: return 6;

                        // 105 i
                        case 105:
                        //106 j
                        case 106:
                        //102 f
                        case 102:
                        //114 r
                        case 114:

                        // 99 c
                        case 99:
                        //116 t
                        case 116:
                            return 4;

                        //108 l
                        case 108: return 3;
                        default: return 5;
                    }
                }
            }
        }

        private static Texture2D FlipTexture(Texture2D original, bool upSideDown = true)
        {
            Texture2D flipped = new Texture2D(original.width, original.height);

            int xN = original.width;
            int yN = original.height;

            for (int i = 0; i < xN; i++)
            {
                for (int j = 0; j < yN; j++)
                {
                    if (upSideDown)
                    {
                        flipped.SetPixel(j, xN - i - 1, original.GetPixel(j, i));
                    }
                    else
                    {
                        flipped.SetPixel(xN - i - 1, j, original.GetPixel(i, j));
                    }
                }
            }

            flipped.Apply(true, true);

            return flipped;
        }
    }

}
#pragma warning restore 618
