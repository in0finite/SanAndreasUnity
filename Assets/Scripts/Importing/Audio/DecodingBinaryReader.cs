using System.IO;

/// <summary>
/// GTA audio sharp namespace
/// </summary>
namespace GTAAudioSharp
{
    /// <summary>
    /// Decoding binary reader class
    /// </summary>
    public class DecodingBinaryReader : BinaryReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="input">Input</param>
        public DecodingBinaryReader(Stream input) : base(input)
        {
            // ...
        }

        /// <summary>
        /// Read and decode byte
        /// </summary>
        /// <returns>Decoded byte</returns>
        public byte ReadDecodeByte()
        {
            byte ret = 0;
            byte[] decoded_bytes = ReadDecodeBytes(sizeof(byte));
            for (int i = 0; i < decoded_bytes.Length; i++)
            {
                ret |= (byte)(decoded_bytes[i] << (i * 8));
            }
            return ret;
        }

        /// <summary>
        /// Read and decode bytes
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>Decoded bytes</returns>
        public byte[] ReadDecodeBytes(int count)
        {
            byte[] ret = ReadBytes(count);
            if (ret != null)
            {
                long base_position = BaseStream.Position - ret.Length;
                for (int i = 0; i < ret.Length; i++)
                {
                    ret[i] ^= GTAAudio.streamsEncodingSecret[(base_position + i) % GTAAudio.streamsEncodingSecret.LongLength];
                }
            }
            return ret;
        }

        /// <summary>
        /// Read and decode unsigned int16
        /// </summary>
        /// <returns>Decoded unsigned int16</returns>
        /*public ushort ReadDecodeUInt16()
        {
            ushort ret = 0;
            byte[] decoded_bytes = ReadDecodeBytes(sizeof(ushort));
            for (int i = 0; i < decoded_bytes.Length; i++)
            {
                ret |= (ushort)(decoded_bytes[i] << (i * 8));
            }
            return ret;
        }*/

        /// <summary>
        /// Read and decode unsigned int32
        /// </summary>
        /// <returns>Decoded unsigned int32</returns>
        public uint ReadDecodeUInt32()
        {
            uint ret = 0U;
            byte[] decoded_bytes = ReadDecodeBytes(sizeof(uint));
            for (int i = 0; i < decoded_bytes.Length; i++)
            {
                ret |= (uint)(decoded_bytes[i] << (i * 8));
            }
            return ret;
        }
    }
}
