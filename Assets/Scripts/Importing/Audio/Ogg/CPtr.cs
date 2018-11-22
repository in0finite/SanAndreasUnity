using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TanjentOGG is released under the 3-clause BSD license. Please read license.txt for the full license.
namespace TanjentOGG
{
    public class CPtr
    {
        public class BytePtr
        {
            public int offset;
            public byte[] bytes;

            public BytePtr(byte[] data)
            {
                this.offset = 0;
                this.bytes = data;
            }

            public BytePtr(BytePtr b)
            {
                this.offset = b.offset;
                this.bytes = b.bytes;
            }

            public BytePtr(BytePtr b, int offset)
            {
                this.offset = b.offset + offset;
                this.bytes = b.bytes;
            }

            public static BytePtr malloc(long size)
            {
                return new BytePtr(new byte[(int)size]);
            }

            public static BytePtr realloc(BytePtr b, int newSize)
            {
                if (b == null)
                {
                    return BytePtr.malloc(newSize);
                }

                if (b.bytes == null)
                {
                    return BytePtr.malloc(newSize);
                }

                if (newSize <= 0)
                {
                    return null;
                }

                byte[] newBytes = new byte[newSize];
                Array.Copy(b.bytes, b.offset, newBytes, 0, Math.Min(b.bytes.Length - b.offset, newSize));

                return new BytePtr(newBytes);
            }

            public static int memcmp(BytePtr ptr1, BytePtr ptr2, int num)
            {
                for (int i = 0; i < num; i++)
                {
                    if (ptr1.bytes[ptr1.offset + i] != ptr2.bytes[ptr2.offset + i])
                    {
                        return ptr1.bytes[ptr1.offset + i].CompareTo(ptr2.bytes[ptr2.offset + i]);
                    }
                }
                return 0;
            }

            public static BytePtr memchr(BytePtr ptr, int value, int num)
            {
                BytePtr ret = new BytePtr(ptr);
                for (int i = 0; i < num; i++)
                {
                    if (ptr.bytes[ptr.offset + i] == value)
                    {
                        ret.offset = ptr.offset + i;
                        return ret;
                    }
                }
                return null;
            }

            public static BytePtr memmove(BytePtr destination, BytePtr source, int num)
            {

                // must have temp source buffer to support overlapping move
                byte[] tmpBytes = new byte[num];
                Array.Copy(source.bytes, source.offset, tmpBytes, 0, num);

                // copy from temp to dest
                Array.Copy(tmpBytes, 0, destination.bytes, destination.offset, num);

                return destination;
            }

            public static BytePtr memcpy(BytePtr destination, BytePtr source, int num)
            {
                // copy from temp to dest
                Array.Copy(source.bytes, source.offset, destination.bytes, destination.offset, num);
                return destination;
            }

        }

        public class FloatPtr
        {
            public int offset;
            public float[] floats;

            public FloatPtr(float[] data)
            {
                this.offset = 0;
                this.floats = data;
            }

            public FloatPtr(float[] data, int offset)
            {
                this.offset = offset;
                this.floats = data;
            }

            public FloatPtr(FloatPtr f)
            {
                this.offset = f.offset;
                this.floats = f.floats;
            }

            public FloatPtr(FloatPtr f, int offset)
            {
                this.offset = f.offset + offset;
                this.floats = f.floats;
            }

            public static FloatPtr memset(FloatPtr ptr, int value, long num)
            {
                for (int i = 0; i < num; i++)
                {
                    ptr.floats[ptr.offset + i] = value;
                }
                return ptr;
            }
        }
    }
}
