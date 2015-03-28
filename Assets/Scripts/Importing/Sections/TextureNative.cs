using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Importing.Sections
{
    [SectionType(21)]
    public class TextureNative : SectionData
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

        private static byte[] ConvertB8G8R8ToRGB24(byte[] data)
        {
            for (var i = 0; i < data.Length - 2; i += 3) {
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

        private byte[] _imageData;
        private byte[] _imageLevelData;

        private bool _convertedImageData;
        private bool _convertedImageLevelData;

        public readonly UInt32 PlatformID;
        public readonly Filter FilterFlags;
        public readonly WrapMode WrapV;
        public readonly WrapMode WrapU;
        public readonly string DiffuseName;
        public readonly string AlphaName;
        public readonly RasterFormat Format;
        public readonly bool Alpha;
        public readonly CompressionMode Compression;
        public readonly UInt16 Width;
        public readonly UInt16 Height;
        public readonly byte BPP;
        public readonly byte MipMapCount;
        public readonly byte RasterType;
        public readonly Int32 ImageDataSize;

        public byte[] ImageData
        {
            get
            {
                if (_convertedImageData) return _imageData;

                _convertedImageData = true;
                _convertedImageLevelData = _imageData == _imageLevelData;

                if (Format == RasterFormat.B8G8R8) {
                    _imageData = ConvertB8G8R8ToRGB24(_imageData);
                } else if (Format == RasterFormat.A1R5G5B5) {
                    _imageData = ConvertA1R5G5B5ToARGB32(_imageData);
                } else if (Compression == CompressionMode.DXT3) {
                    _imageData = ConvertDXT3ToDXT5(_imageData);
                }

                return _imageData;
            }
        }

        public byte[] ImageLevelData
        {
            get
            {
                if (_convertedImageLevelData) return _imageLevelData;

                _convertedImageLevelData = true;
                _convertedImageData = _imageData == _imageLevelData;
                
                if (Format == RasterFormat.B8G8R8) {
                    _imageLevelData = ConvertB8G8R8ToRGB24(_imageLevelData);
                } else if (Format == RasterFormat.A1R5G5B5) {
                    _imageLevelData = ConvertA1R5G5B5ToARGB32(_imageLevelData);
                } else if (Compression == CompressionMode.DXT3) {
                    _imageLevelData = ConvertDXT3ToDXT5(_imageLevelData);
                }

                return _imageLevelData;
            }
        }

        public TextureNative(SectionHeader header, Stream stream)
        {
            SectionHeader.Read(stream);
            var reader = new BinaryReader(stream);

            PlatformID = reader.ReadUInt32();
            FilterFlags = (Filter) reader.ReadUInt16();
            WrapV = (WrapMode) reader.ReadByte();
            WrapU = (WrapMode) reader.ReadByte();
            DiffuseName = reader.ReadString(32);
            AlphaName = reader.ReadString(32);
            Format = (RasterFormat) reader.ReadUInt32();

            if (PlatformID == 9) {
                var dxt = reader.ReadString(4);
                switch (dxt) {
                    case "DXT1":
                        Compression = CompressionMode.DXT1; break;
                    case "DXT3":
                        Compression = CompressionMode.DXT3; break;
                    default:
                        Compression = CompressionMode.None; break;
                }
            } else {
                Alpha = reader.ReadUInt32() == 0x1;
            }

            Width = reader.ReadUInt16();
            Height = reader.ReadUInt16();
            BPP = (byte) (reader.ReadByte() >> 3);
            MipMapCount = reader.ReadByte();
            RasterType = reader.ReadByte();

            if (RasterType != 0x4) {
                throw new Exception("Unexpected RasterType, expected 0x04.");
            }

            if (PlatformID == 9) {
                Alpha = (reader.ReadByte() & 0x1) == 0x1;
            } else {
                Compression = (CompressionMode) reader.ReadByte();
            }

            ImageDataSize = reader.ReadInt32();

            _imageData = reader.ReadBytes(ImageDataSize);

            if ((Format & RasterFormat.ExtMipMap) != 0) {
                var tot = ImageDataSize;
                for (var i = 0; i < MipMapCount; ++i) {
                    tot += ImageDataSize >> (2 * i);
                }

                _imageLevelData = reader.ReadBytes(tot);
            } else {
                _imageLevelData = _imageData;
            }
        }
    }
}
