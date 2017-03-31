using System;
using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.RenderWareStream;
using UnityEngine;

namespace SanAndreasUnity.Importing.Conversion
{
    public static class TextureDictionaryExtensions
    {
        public static LoadedTexture GetDiffuse(this TextureDictionary[] txds, string name)
        {
            foreach (var txd in txds) {
                var tex = txd.GetDiffuse(name);
                if (tex != null) return tex;
            }

            return null;
        }

        public static LoadedTexture GetAlpha(this TextureDictionary[] txds, string name)
        {
            foreach (var txd in txds) {
                var tex = txd.GetAlpha(name);
                if (tex != null) return tex;
            }

            return null;
        }
    }

    public class LoadedTexture
    {
        public readonly Texture2D Texture;
        public readonly bool HasAlpha;

        public LoadedTexture(Texture2D tex, bool hasAlpha)
        {
            Texture = tex;
            HasAlpha = hasAlpha;
        }
    }

    public class TextureDictionary
    {
        private static byte[] ConvertDXT3ToDXT5(byte[] data)
        {
            for (var i = 0; i < data.Length; i += 16) {
                ulong packed = 0;

                for (var j = 0; j < 16; ++j) {
                    var s = 1 | ((j & 1) << 2);
                    var c = (data[i + (j >> 1)] >> s) & 0x7;

                    switch (c) {
                        case 0: c = 1; break;
                        case 7: c = 0; break;
                        default: c = 8 - c; break;
                    }

                    packed |= ((ulong) c << (3 * j));
                }

                data[i + 0] = 0xff;
                data[i + 1] = 0x00;

                for (var j = 0; j < 6; ++j) {
                    data[i + 2 + j] = (byte) ((packed >> (j << 3)) & 0xff);
                }
            }

            return data;
        }

        private static byte[] ConvertBGRToRGB(byte[] data)
        {
            for (var i = 0; i < data.Length; i += 4) {
                var t = data[i];
                data[i] = data[i + 2];
                data[i + 2] = t;
            }

            return data;
        }

        private static readonly byte[] _sR5G5B5Lookup = Enumerable.Range(0, 0x20)
            .Select(x => (byte) Math.Round(((double) x / 0x1f) * 0xff))
            .ToArray();

        private static byte[] ConvertA1R5G5B5ToARGB32(byte[] data)
        {
            var dest = new byte[data.Length << 1];

            for (var i = 0; i < data.Length - 1; i += 2) {
                var val = data[i] | (data[i + 1] << 8);
                var d = i << 1;

                dest[d + 0] = (byte) ((val & 1) == 1 ? 0xff : 0);
                dest[d + 1] = _sR5G5B5Lookup[(val >> 1) & 0x1f];
                dest[d + 2] = _sR5G5B5Lookup[(val >> 6) & 0x1f];
                dest[d + 3] = _sR5G5B5Lookup[(val >> 11) & 0x1f];
            }

            return dest;
        }

        private static LoadedTexture Convert(TextureNative src)
        {
            TextureFormat format;

            var loadMips = (src.Format & RasterFormat.ExtMipMap) == RasterFormat.ExtMipMap;
            var autoMips = (src.Format & RasterFormat.ExtAutoMipMap) == RasterFormat.ExtAutoMipMap;

            switch (src.Format & RasterFormat.NoExt) {
                case RasterFormat.BGRA8:
                case RasterFormat.BGR8:
                    format = TextureFormat.RGBA32;
                    break;
                case RasterFormat.LUM8:
                    format = TextureFormat.Alpha8;
                    break;
                case RasterFormat.R4G4B4A4:
                    format = TextureFormat.RGBA4444;
                    break;
                case RasterFormat.A1R5G5B5:
                    format = TextureFormat.ARGB32;
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
                    format = TextureFormat.DXT5;
                    break;
                default:
                    throw new NotImplementedException(string.Format("CompressionMode.{0}", src.Compression));
            }

            var tex = new Texture2D(src.Width, src.Height, format, false /*loadMips*/);

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

            var data = src.ImageData;
            
            switch (src.Format & RasterFormat.NoExt) {
                case RasterFormat.BGR8:
                case RasterFormat.BGRA8:
                    data = ConvertBGRToRGB(data);
                    break;
            }

            switch (src.Compression) {
                case CompressionMode.DXT3:
                    data = ConvertDXT3ToDXT5(data);
                    break;
            }

            tex.LoadRawTextureData(data);
			tex.Apply(loadMips || autoMips,
				false /* true */); // makeNoLongerReadable: needed to flip GUI textures. doubles memory used for textures!

            return new LoadedTexture(tex, src.Alpha);
        }

        private static readonly Dictionary<string, string> _sParents = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        private static readonly Dictionary<string, TextureDictionary> _sLoaded = new Dictionary<string, TextureDictionary>(StringComparer.InvariantCultureIgnoreCase);
        
        public static TextureDictionary Load(string name)
        {
            name = name.ToLower();
            if (_sLoaded.ContainsKey(name)) return _sLoaded[name];

            var txd = new TextureDictionary(ArchiveManager.ReadFile<RenderWareStream.TextureDictionary>(name + ".txd"));
            _sLoaded.Add(name, txd);

            return txd;
        }

        public static void AddParent(string dictName, string parentName)
        {
            dictName = dictName.ToLower();
            parentName = parentName.ToLower();

            if (_sParents.ContainsKey(dictName)) return;

            _sParents.Add(dictName, parentName);

            if (_sLoaded.ContainsKey(dictName)) {
                _sLoaded[dictName].ParentName = parentName;
            }
        }

        private class Texture
        {
            private LoadedTexture _converted;
            private bool _attemptedConversion;

            public string DiffuseName { get { return Native.DiffuseName; } }
            public string AlphaName { get { return Native.AlphaName; } }

            public bool IsDiffuse { get { return !string.IsNullOrEmpty(DiffuseName); } }
            public bool IsAlpha { get { return !string.IsNullOrEmpty(AlphaName); } }

            public LoadedTexture Converted
            {
                get
                {
                    if (_attemptedConversion) return _converted;
                    _attemptedConversion = true;
                    _converted = Convert(Native);
                    return _converted;
                }
            }

            public readonly TextureNative Native;

            public Texture(TextureNative native)
            {
                Native = native;
            }
        }

        private readonly Dictionary<string, Texture> _diffuse;
        private readonly Dictionary<string, Texture> _alpha;
        private TextureDictionary _parent;

        public string ParentName { get; set; }

        public TextureDictionary Parent
        {
            get { return _parent ?? (_parent = Load(ParentName)); }
        }

        public IEnumerable<string> DiffuseNames
        {
            get { return _diffuse.Keys; }
        }

        public IEnumerable<string> AlphaNames
        {
            get { return _alpha.Keys; }
        }

        private TextureDictionary(RenderWareStream.TextureDictionary txd)
        {
            _diffuse = new Dictionary<string, Texture>(StringComparer.InvariantCultureIgnoreCase);
            _alpha = new Dictionary<string, Texture>(StringComparer.InvariantCultureIgnoreCase);

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

        public TextureNative GetDiffuseNative(string name)
        {
            if (!_diffuse.ContainsKey(name)) {
                return ParentName != null ? Parent.GetDiffuseNative(name) : null;
            }

            return _diffuse[name].Native;
        }

        public LoadedTexture GetDiffuse(string name)
        {
            if (!_diffuse.ContainsKey(name)) {
                return ParentName != null ? Parent.GetDiffuse(name) : null;
            }

            return _diffuse[name].Converted;
        }

        public LoadedTexture GetAlpha(string name)
        {
            if (!_alpha.ContainsKey(name)) {
                return ParentName != null ? Parent.GetAlpha(name) : null;
            }

            return _alpha[name].Converted;
        }
    }
}
