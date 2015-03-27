using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.Sections;
using UnityEngine;

namespace SanAndreasUnity.Importing.Conversion
{
    internal class TextureDictionary
    {
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

            var data = src.ImageData;

            switch (src.Compression) {
                case CompressionMode.None:
                    break;
                case CompressionMode.DXT1:
                    format = TextureFormat.DXT1;
                    break;
                case CompressionMode.DXT3:
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

            tex.LoadRawTextureData(data);
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

        private class Texture
        {
            private readonly TextureNative _native;
            private Texture2D _converted;
            private bool _attemptedConversion;

            public string DiffuseName { get { return _native.DiffuseName; } }
            public string AlphaName { get { return _native.AlphaName; } }

            public bool IsDiffuse { get { return !string.IsNullOrEmpty(DiffuseName); } }
            public bool IsAlpha { get { return !string.IsNullOrEmpty(AlphaName); } }

            public Texture2D Converted
            {
                get
                {
                    if (_attemptedConversion) return _converted;
                    _attemptedConversion = true;
                    _converted = Convert(_native);
                    return _converted;
                }
            }

            public Texture(TextureNative native)
            {
                _native = native;
            }
        }

        private readonly Dictionary<string, Texture> _diffuse;
        private readonly Dictionary<string, Texture> _alpha;

        private TextureDictionary(Sections.TextureDictionary txd)
        {
            _diffuse = new Dictionary<string, Texture>();
            _alpha = new Dictionary<string, Texture>();

            foreach (var native in txd.Textures) {
                var tex = new Texture(native);

                if (tex.IsDiffuse) {
                    _diffuse.Add(tex.DiffuseName, tex);
                }

                if (tex.IsAlpha) {
                    if (_alpha.ContainsKey(tex.AlphaName)) {
                        Debug.LogWarningFormat("Tried to re-add {0} (diffuse {1} vs {2})",
                            tex.AlphaName, tex.DiffuseName, _alpha[tex.AlphaName].DiffuseName);
                        continue;
                    }

                    _alpha.Add(tex.AlphaName, tex);
                }
            }
        }

        public Texture2D GetDiffuse(string name)
        {
            name = name.ToLower();

            if (!_diffuse.ContainsKey(name)) return null;
            return _diffuse[name].Converted;
        }

        public Texture2D GetAlpha(string name)
        {
            name = name.ToLower();

            if (!_alpha.ContainsKey(name)) {
                Debug.LogWarningFormat("Couldn't find alpha texture {0}", name);
                return null;
            }

            return _alpha[name].Converted;
        }
    }
}
