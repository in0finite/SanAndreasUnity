using System;
using System.Collections.Generic;
using System.IO;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Importing.Sections
{
    [SectionType(21)]
    internal class TextureNative : SectionData
    {
        private static void ConvertDXT3ToDXT5(IList<byte> data)
        {
            for (var i = 0; i < data.Count; i += 16) {
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
        }

        private readonly byte[] _imageData;
        private readonly byte[] _imageLevelData;

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

                if (Compression == CompressionMode.DXT3) {
                    ConvertDXT3ToDXT5(_imageData);
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

                if (Compression == CompressionMode.DXT3) {
                    ConvertDXT3ToDXT5(_imageLevelData);
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
