using System;
using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Sections;
using UnityEngine;

namespace SanAndreasUnity.Importing.Conversion
{
    internal class TextureDictionary
    {
        private static void ConvertDXT3ToDXT5(IList<byte> data)
        {
            var a = new byte[16];
            for (var i = 0; i < data.Count; i += 8) {
                for (var j = 0; j < 8; ++j) {
                    a[j << 1] = (byte) (data[i + j] & 0xf);
                    a[(j << 1) + 1] = (byte) ((data[i + j] >> 4) & 0xf);

                    data[i + j] = 0xff;
                }
            }
        }

        private static Texture2D Convert(TextureNative src)
        {
            TextureFormat format;

            var precMips = (src.Format & RasterFormat.ExtMipMap) == RasterFormat.ExtMipMap;
            var autoMips = (src.Format & RasterFormat.ExtAutoMipMap) == RasterFormat.ExtAutoMipMap;

            switch (src.Format & RasterFormat.NoExt) {
                case RasterFormat.B8G8R8A8:
                    format = TextureFormat.BGRA32;
                    break;
                case RasterFormat.LUM8:
                    format = TextureFormat.Alpha8;
                    break;
                case RasterFormat.R4G4B4A4:
                    format = TextureFormat.RGBA4444;
                    break;
                case RasterFormat.R5G6B5:
                    format = TextureFormat.RGB565;
                    break;
                default:
                    throw new NotImplementedException(string.Format("RasterFormat.{0}", src.Format & RasterFormat.NoExt));
            }

            switch (src.Compression) {
                case CompressionMode.None:
                    break;
                case CompressionMode.DXT1:
                    format = TextureFormat.DXT1;
                    break;
                case CompressionMode.DXT3:
                    //ConvertDXT3ToDXT5(src.ImageLevelData);
                    format = TextureFormat.DXT5;
                    break;
                default:
                    throw new NotImplementedException(string.Format("CompressionMode.{0}", src.Compression));
            }

            var tex = new Texture2D(src.Width, src.Height, format, false /*precMips*/);

            switch (src.FilterFlags) {
                case Filter.None:
                case Filter.Nearest:
                case Filter.MipNearest:
                    tex.filterMode = FilterMode.Point;
                    break;
                case Filter.Linear:
                case Filter.MipLinear:
                case Filter.LinearMipNearest:
                    tex.filterMode = FilterMode.Bilinear;
                    break;
                case Filter.LinearMipLinear:
                case Filter.Unknown:
                    tex.filterMode = FilterMode.Trilinear;
                    break;
            }

            tex.LoadRawTextureData(src.ImageData);
            tex.Apply(precMips || autoMips, true);

            return tex;
        }

        private static readonly Dictionary<string, TextureDictionary> _sLoaded = new Dictionary<string, TextureDictionary>();

        public static TextureDictionary Load(string name)
        {
            name = name.ToLower();

            if (_sLoaded.ContainsKey(name)) return _sLoaded[name];

            var txd = new TextureDictionary(ResourceManager.ReadFile<Sections.TextureDictionary>(name + ".txd"));
            _sLoaded.Add(name, txd);

            return txd;
        }

        private readonly TextureNative[] _natives;

        private readonly Dictionary<string, Texture2D> _diffuse;
        private readonly Dictionary<string, Texture2D> _alpha;

        private TextureDictionary(Sections.TextureDictionary txd)
        {
            _natives = txd.Textures;
            _diffuse = new Dictionary<string, Texture2D>();

            foreach (var native in _natives) {
                if (native.DiffuseName != null) {
                    _diffuse.Add(native.DiffuseName, null);
                }
            }
        }

        public Texture2D GetDiffuse(string name)
        {
            if (!_diffuse.ContainsKey(name)) return null;
            if (_diffuse[name] != null) return _diffuse[name];

            var tex = _diffuse[name] = Convert(_natives.First(x => x.DiffuseName.Equals(name)));

            return tex;
        }
    }
}
