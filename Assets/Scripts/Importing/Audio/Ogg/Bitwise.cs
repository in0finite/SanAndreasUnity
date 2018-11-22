using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TanjentOGG is released under the 3-clause BSD license. Please read license.txt for the full license.
namespace TanjentOGG
{
    public class Bitwise
    {
        private static long[] mask = new long[]
            {0x00000000, 0x00000001, 0x00000003, 0x00000007, 0x0000000f,
                    0x0000001f, 0x0000003f, 0x0000007f, 0x000000ff, 0x000001ff,
                    0x000003ff, 0x000007ff, 0x00000fff, 0x00001fff, 0x00003fff,
                    0x00007fff, 0x0000ffff, 0x0001ffff, 0x0003ffff, 0x0007ffff,
                    0x000fffff, 0x001fffff, 0x003fffff, 0x007fffff, 0x00ffffff,
                    0x01ffffff, 0x03ffffff, 0x07ffffff, 0x0fffffff, 0x1fffffff,
                    0x3fffffff, 0x7fffffff, 0xffffffff};

        public static void oggpack_readinit(Vogg.oggpack_buffer b, CPtr.BytePtr buf, int bytes)
        {
            b.clear();
            b.buffer = b.ptr = buf;
            b.storage = bytes;
        }

        /* Read in bits without advancing the bitptr; bits <= 32 */
        public static long oggpack_look(Vogg.oggpack_buffer b, int bits)
        {
            long ret;
            long m;

            if (bits < 0 || bits > 32) return -1;
            m = mask[bits];
            bits += b.endbit;

            if (b.endbyte >= b.storage - 4)
            {
                /* not the main path */
                if (b.endbyte > b.storage - ((bits + 7) >> 3)) return -1;
                /* special case to avoid reading b.ptr[0], which might be past the end of
                    the buffer; also skips some useless accounting */
                else if (bits == 0) return (0L);
            }

            ret = (b.ptr.bytes[b.ptr.offset + 0] & 0xFF) >> b.endbit;
            if (bits > 8)
            {
                ret |= (b.ptr.bytes[b.ptr.offset + 1] & 0xFF) << (8 - b.endbit);
                if (bits > 16)
                {
                    ret |= (b.ptr.bytes[b.ptr.offset + 2] & 0xFF) << (16 - b.endbit);
                    if (bits > 24)
                    {
                        ret |= (b.ptr.bytes[b.ptr.offset + 3] & 0xFF) << (24 - b.endbit);
                        if (bits > 32 && (b.endbit != 0))
                            ret |= (b.ptr.bytes[b.ptr.offset + 4] & 0xFF) << (32 - b.endbit);
                    }
                }
            }
            return (m & ret);
        }
		
        public static void oggpack_adv(Vogg.oggpack_buffer b, int bits)
        {
            bits += b.endbit;

            if (b.endbyte > b.storage - ((bits + 7) >> 3))
            {
                b.ptr = null;
                b.endbyte = b.storage;
                b.endbit = 1;
            }

            b.ptr.offset += bits / 8;
            b.endbyte += bits / 8;
            b.endbit = bits & 7;
        }

        /* bits <= 32 */
        public static long oggpack_read(Vogg.oggpack_buffer b, int bits)
        {
            long ret;
            long m;

            if (bits < 0 || bits > 32)
            {
                b.ptr = null;
                b.endbyte = b.storage;
                b.endbit = 1;
                return -1L;
            }
            m = mask[bits];
            bits += b.endbit;

            if (b.endbyte >= b.storage - 4)
            {
                /* not the main path */
                if (b.endbyte > b.storage - ((bits + 7) >> 3))
                {
                    b.ptr = null;
                    b.endbyte = b.storage;
                    b.endbit = 1;
                    return -1L;
                }
                /* special case to avoid reading b.ptr[0], which might be past the end of
                    the buffer; also skips some useless accounting */
                else if (bits == 0) return (0L);
            }

            ret = (b.ptr.bytes[b.ptr.offset + 0] & 0xFF) >> b.endbit;
            if (bits > 8)
            {
                ret |= (b.ptr.bytes[b.ptr.offset + 1] & 0xFF) << (8 - b.endbit);
                if (bits > 16)
                {
                    ret |= (b.ptr.bytes[b.ptr.offset + 2] & 0xFF) << (16 - b.endbit);
                    if (bits > 24)
                    {
                        ret |= (b.ptr.bytes[b.ptr.offset + 3] & 0xFF) << (24 - b.endbit);
                        if (bits > 32 && b.endbit != 0)
                        {
                            ret |= (b.ptr.bytes[b.ptr.offset + 4] & 0xFF) << (32 - b.endbit);
                        }
                    }
                }
            }
            ret &= m;
            b.ptr.offset += bits / 8;
            b.endbyte += bits / 8;
            b.endbit = bits & 7;
            return ret;
        }

        public static long oggpack_bytes(Vogg.oggpack_buffer b)
        {
            return (b.endbyte + (b.endbit + 7) / 8);
        }
    }
}
