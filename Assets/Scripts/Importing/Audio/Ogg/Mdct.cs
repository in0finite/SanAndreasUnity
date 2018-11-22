using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TanjentOGG is released under the 3-clause BSD license. Please read license.txt for the full license.
namespace TanjentOGG
{
    public class Mdct
    {
        static int rint(float x)
        {
            return (int)Math.Floor(x + 0.5f);
        }
        /* build lookups for trig functions; also pre-figure scaling and
       some window function algebra. */

        public static void mdct_init(mdct_lookup lookup, int n)
        {
            int[] bitrev = new int[n / 4];
            float[] T = new float[(n + n / 4)];

            int i;
            int n2 = n >> 1;
            int log2n = lookup.log2n = rint((float)(Math.Log((float)n) / Math.Log(2f)));
            lookup.n = n;
            lookup.trig = T;
            lookup.bitrev = bitrev;

            /* trig lookups... */

            for (i = 0; i < n / 4; i++)
            {
                T[i * 2] = (float)Math.Cos((Math.PI / n) * (4 * i));
                T[i * 2 + 1] = (float)-Math.Sin((Math.PI / n) * (4 * i));
                T[n2 + i * 2] = (float)Math.Cos((Math.PI / (2 * n)) * (2 * i + 1));
                T[n2 + i * 2 + 1] = (float)Math.Sin((Math.PI / (2 * n)) * (2 * i + 1));
            }
            for (i = 0; i < n / 8; i++)
            {
                T[n + i * 2] = (float)(Math.Cos((Math.PI / n) * (4 * i + 2)) * .5);
                T[n + i * 2 + 1] = (float)(-Math.Sin((Math.PI / n) * (4 * i + 2)) * .5);
            }

            /* bitreverse lookup... */

            {
                int mask = (1 << (log2n - 1)) - 1, j;
                int msb = 1 << (log2n - 2);
                for (i = 0; i < n / 8; i++)
                {
                    int acc = 0;
                    for (j = 0; (msb >> j) != 0; j++)
                        if (((msb >> j) & i) != 0) acc |= 1 << j;
                    bitrev[i * 2] = ((~acc) & mask) - 1;
                    bitrev[i * 2 + 1] = acc;

                }
            }
            lookup.scale = (4f / n);
        }

        public class mdct_lookup
        {
            public int n;
            public int log2n;

            public float[] trig;
            public int[] bitrev;

            public float scale;
        }

        private static float cPI3_8 = .38268343236508977175F;
        private static float cPI2_8 = .70710678118654752441F;
        private static float cPI1_8 = .92387953251128675613F;

        /* 8 point butterfly (in place, 4 register) */
        public static void mdct_butterfly_8(CPtr.FloatPtr x)
        {
            float r0 = x.floats[x.offset + 6] + x.floats[x.offset + 2];
            float r1 = x.floats[x.offset + 6] - x.floats[x.offset + 2];
            float r2 = x.floats[x.offset + 4] + x.floats[x.offset + 0];
            float r3 = x.floats[x.offset + 4] - x.floats[x.offset + 0];

            x.floats[x.offset + 6] = r0 + r2;
            x.floats[x.offset + 4] = r0 - r2;

            r0 = x.floats[x.offset + 5] - x.floats[x.offset + 1];
            r2 = x.floats[x.offset + 7] - x.floats[x.offset + 3];
            x.floats[x.offset + 0] = r1 + r0;
            x.floats[x.offset + 2] = r1 - r0;

            r0 = x.floats[x.offset + 5] + x.floats[x.offset + 1];
            r1 = x.floats[x.offset + 7] + x.floats[x.offset + 3];
            x.floats[x.offset + 3] = r2 + r3;
            x.floats[x.offset + 1] = r2 - r3;
            x.floats[x.offset + 7] = r1 + r0;
            x.floats[x.offset + 5] = r1 - r0;
        }

        /* 16 point butterfly (in place, 4 register) */
        public static void mdct_butterfly_16(CPtr.FloatPtr x)
        {
            float r0 = x.floats[x.offset + 1] - x.floats[x.offset + 9];
            float r1 = x.floats[x.offset + 0] - x.floats[x.offset + 8];

            x.floats[x.offset + 8] += x.floats[x.offset + 0];
            x.floats[x.offset + 9] += x.floats[x.offset + 1];
            x.floats[x.offset + 0] = ((r0 + r1) * cPI2_8);
            x.floats[x.offset + 1] = ((r0 - r1) * cPI2_8);

            r0 = x.floats[x.offset + 3] - x.floats[x.offset + 11];
            r1 = x.floats[x.offset + 10] - x.floats[x.offset + 2];
            x.floats[x.offset + 10] += x.floats[x.offset + 2];
            x.floats[x.offset + 11] += x.floats[x.offset + 3];
            x.floats[x.offset + 2] = r0;
            x.floats[x.offset + 3] = r1;

            r0 = x.floats[x.offset + 12] - x.floats[x.offset + 4];
            r1 = x.floats[x.offset + 13] - x.floats[x.offset + 5];
            x.floats[x.offset + 12] += x.floats[x.offset + 4];
            x.floats[x.offset + 13] += x.floats[x.offset + 5];
            x.floats[x.offset + 4] = ((r0 - r1) * cPI2_8);
            x.floats[x.offset + 5] = ((r0 + r1) * cPI2_8);

            r0 = x.floats[x.offset + 14] - x.floats[x.offset + 6];
            r1 = x.floats[x.offset + 15] - x.floats[x.offset + 7];
            x.floats[x.offset + 14] += x.floats[x.offset + 6];
            x.floats[x.offset + 15] += x.floats[x.offset + 7];
            x.floats[x.offset + 6] = r0;
            x.floats[x.offset + 7] = r1;

            mdct_butterfly_8(x);
            mdct_butterfly_8(new CPtr.FloatPtr(x, 8));
        }

        /* 32 point butterfly (in place, 4 register) */
        public static void mdct_butterfly_32(CPtr.FloatPtr x)
        {
            float r0 = x.floats[x.offset + 30] - x.floats[x.offset + 14];
            float r1 = x.floats[x.offset + 31] - x.floats[x.offset + 15];

            x.floats[x.offset + 30] += x.floats[x.offset + 14];
            x.floats[x.offset + 31] += x.floats[x.offset + 15];
            x.floats[x.offset + 14] = r0;
            x.floats[x.offset + 15] = r1;

            r0 = x.floats[x.offset + 28] - x.floats[x.offset + 12];
            r1 = x.floats[x.offset + 29] - x.floats[x.offset + 13];
            x.floats[x.offset + 28] += x.floats[x.offset + 12];
            x.floats[x.offset + 29] += x.floats[x.offset + 13];
            x.floats[x.offset + 12] = (r0 * cPI1_8 - r1 * cPI3_8);
            x.floats[x.offset + 13] = (r0 * cPI3_8 + r1 * cPI1_8);

            r0 = x.floats[x.offset + 26] - x.floats[x.offset + 10];
            r1 = x.floats[x.offset + 27] - x.floats[x.offset + 11];
            x.floats[x.offset + 26] += x.floats[x.offset + 10];
            x.floats[x.offset + 27] += x.floats[x.offset + 11];
            x.floats[x.offset + 10] = ((r0 - r1) * cPI2_8);
            x.floats[x.offset + 11] = ((r0 + r1) * cPI2_8);

            r0 = x.floats[x.offset + 24] - x.floats[x.offset + 8];
            r1 = x.floats[x.offset + 25] - x.floats[x.offset + 9];
            x.floats[x.offset + 24] += x.floats[x.offset + 8];
            x.floats[x.offset + 25] += x.floats[x.offset + 9];
            x.floats[x.offset + 8] = (r0 * cPI3_8 - r1 * cPI1_8);
            x.floats[x.offset + 9] = (r1 * cPI3_8 + r0 * cPI1_8);

            r0 = x.floats[x.offset + 22] - x.floats[x.offset + 6];
            r1 = x.floats[x.offset + 7] - x.floats[x.offset + 23];
            x.floats[x.offset + 22] += x.floats[x.offset + 6];
            x.floats[x.offset + 23] += x.floats[x.offset + 7];
            x.floats[x.offset + 6] = r1;
            x.floats[x.offset + 7] = r0;

            r0 = x.floats[x.offset + 4] - x.floats[x.offset + 20];
            r1 = x.floats[x.offset + 5] - x.floats[x.offset + 21];
            x.floats[x.offset + 20] += x.floats[x.offset + 4];
            x.floats[x.offset + 21] += x.floats[x.offset + 5];
            x.floats[x.offset + 4] = (r1 * cPI1_8 + r0 * cPI3_8);
            x.floats[x.offset + 5] = (r1 * cPI3_8 - r0 * cPI1_8);

            r0 = x.floats[x.offset + 2] - x.floats[x.offset + 18];
            r1 = x.floats[x.offset + 3] - x.floats[x.offset + 19];
            x.floats[x.offset + 18] += x.floats[x.offset + 2];
            x.floats[x.offset + 19] += x.floats[x.offset + 3];
            x.floats[x.offset + 2] = ((r1 + r0) * cPI2_8);
            x.floats[x.offset + 3] = ((r1 - r0) * cPI2_8);

            r0 = x.floats[x.offset + 0] - x.floats[x.offset + 16];
            r1 = x.floats[x.offset + 1] - x.floats[x.offset + 17];
            x.floats[x.offset + 16] += x.floats[x.offset + 0];
            x.floats[x.offset + 17] += x.floats[x.offset + 1];
            x.floats[x.offset + 0] = (r1 * cPI3_8 + r0 * cPI1_8);
            x.floats[x.offset + 1] = (r1 * cPI1_8 - r0 * cPI3_8);

            mdct_butterfly_16(x);
            mdct_butterfly_16(new CPtr.FloatPtr(x, 16));

        }

        /* N point first stage butterfly (in place, 2 register) */
        public static void mdct_butterfly_first(float[] T, int Toffset, CPtr.FloatPtr x, int points)
        {

            float[] x1 = x.floats;
            int x1offset = x.offset + points - 8;
            float[] x2 = x.floats;
            int x2offset = x.offset + (points >> 1) - 8;
            float r0;
            float r1;

            do
            {

                r0 = x1[x1offset + 6] - x2[x2offset + 6];
                r1 = x1[x1offset + 7] - x2[x2offset + 7];
                x1[x1offset + 6] += x2[x2offset + 6];
                x1[x1offset + 7] += x2[x2offset + 7];
                x2[x2offset + 6] = (r1 * T[Toffset + 1] + r0 * T[Toffset + 0]);
                x2[x2offset + 7] = (r1 * T[Toffset + 0] - r0 * T[Toffset + 1]);

                r0 = x1[x1offset + 4] - x2[x2offset + 4];
                r1 = x1[x1offset + 5] - x2[x2offset + 5];
                x1[x1offset + 4] += x2[x2offset + 4];
                x1[x1offset + 5] += x2[x2offset + 5];
                x2[x2offset + 4] = (r1 * T[Toffset + 5] + r0 * T[Toffset + 4]);
                x2[x2offset + 5] = (r1 * T[Toffset + 4] - r0 * T[Toffset + 5]);

                r0 = x1[x1offset + 2] - x2[x2offset + 2];
                r1 = x1[x1offset + 3] - x2[x2offset + 3];
                x1[x1offset + 2] += x2[x2offset + 2];
                x1[x1offset + 3] += x2[x2offset + 3];
                x2[x2offset + 2] = (r1 * T[Toffset + 9] + r0 * T[Toffset + 8]);
                x2[x2offset + 3] = (r1 * T[Toffset + 8] - r0 * T[Toffset + 9]);

                r0 = x1[x1offset + 0] - x2[x2offset + 0];
                r1 = x1[x1offset + 1] - x2[x2offset + 1];
                x1[x1offset + 0] += x2[x2offset + 0];
                x1[x1offset + 1] += x2[x2offset + 1];
                x2[x2offset + 0] = (r1 * T[Toffset + 13] + r0 * T[Toffset + 12]);
                x2[x2offset + 1] = (r1 * T[Toffset + 12] - r0 * T[Toffset + 13]);

                x1offset -= 8;
                x2offset -= 8;
                Toffset += 16;

            } while (x2offset >= x.offset);
        }

        /* N/stage point generic N stage butterfly (in place, 2 register) */
        public static void mdct_butterfly_generic(float[] T, int Toffset, CPtr.FloatPtr x, int points, int trigint)
        {

            float[] x1 = x.floats;
            int x1offset = x.offset + points - 8;
            float[] x2 = x.floats;
            int x2offset = x.offset + (points >> 1) - 8;
            float r0;
            float r1;

            do
            {

                r0 = x1[x1offset + 6] - x2[x2offset + 6];
                r1 = x1[x1offset + 7] - x2[x2offset + 7];
                x1[x1offset + 6] += x2[x2offset + 6];
                x1[x1offset + 7] += x2[x2offset + 7];
                x2[x2offset + 6] = (r1 * T[Toffset + 1] + r0 * T[Toffset + 0]);
                x2[x2offset + 7] = (r1 * T[Toffset + 0] - r0 * T[Toffset + 1]);

                Toffset += trigint;

                r0 = x1[x1offset + 4] - x2[x2offset + 4];
                r1 = x1[x1offset + 5] - x2[x2offset + 5];
                x1[x1offset + 4] += x2[x2offset + 4];
                x1[x1offset + 5] += x2[x2offset + 5];
                x2[x2offset + 4] = (r1 * T[Toffset + 1] + r0 * T[Toffset + 0]);
                x2[x2offset + 5] = (r1 * T[Toffset + 0] - r0 * T[Toffset + 1]);

                Toffset += trigint;

                r0 = x1[x1offset + 2] - x2[x2offset + 2];
                r1 = x1[x1offset + 3] - x2[x2offset + 3];
                x1[x1offset + 2] += x2[x2offset + 2];
                x1[x1offset + 3] += x2[x2offset + 3];
                x2[x2offset + 2] = (r1 * T[Toffset + 1] + r0 * T[Toffset + 0]);
                x2[x2offset + 3] = (r1 * T[Toffset + 0] - r0 * T[Toffset + 1]);

                Toffset += trigint;

                r0 = x1[x1offset + 0] - x2[x2offset + 0];
                r1 = x1[x1offset + 1] - x2[x2offset + 1];
                x1[x1offset + 0] += x2[x2offset + 0];
                x1[x1offset + 1] += x2[x2offset + 1];
                x2[x2offset + 0] = (r1 * T[Toffset + 1] + r0 * T[Toffset + 0]);
                x2[x2offset + 1] = (r1 * T[Toffset + 0] - r0 * T[Toffset + 1]);

                Toffset += trigint;
                x1offset -= 8;
                x2offset -= 8;

            } while (x2offset >= x.offset);
        }

        public static void mdct_butterflies(mdct_lookup init, CPtr.FloatPtr x, int points)
        {

            float[] T = init.trig;
            int stages = init.log2n - 5;
            int i, j;

            if (--stages > 0)
            {
                mdct_butterfly_first(T, 0, x, points);
            }

            for (i = 1; --stages > 0; i++)
            {
                for (j = 0; j < (1 << i); j++)
                    mdct_butterfly_generic(T, 0, new CPtr.FloatPtr(x, (points >> i) * j), points >> i, 4 << i);
            }

            for (j = 0; j < points; j += 32)
                mdct_butterfly_32(new CPtr.FloatPtr(x, j));

        }

        public static void mdct_bitreverse(mdct_lookup init, CPtr.FloatPtr x)
        {
            int n = init.n;
            int[] bit = init.bitrev;
            int bitoffset = 0;
            float[] w0 = x.floats;
            int w0offset = x.offset;
            float[] w1 = x.floats;
            int xoffset = w0offset + (n >> 1);
            int w1offset = xoffset;
            float[] T = init.trig;
            int Toffset = n;

            do
            {
                float[] x0 = x.floats;
                int x0offset = xoffset + bit[bitoffset + 0];
                float[] x1 = x.floats;
                int x1offset = xoffset + bit[bitoffset + 1];

                float r0 = x0[x0offset + 1] - x1[x1offset + 1];
                float r1 = x0[x0offset + 0] + x1[x1offset + 0];
                float r2 = (r1 * T[Toffset + 0] + r0 * T[Toffset + 1]);
                float r3 = (r1 * T[Toffset + 1] - r0 * T[Toffset + 0]);

                w1offset -= 4;

                r0 = 0.5f * (x0[x0offset + 1] + x1[x1offset + 1]);
                r1 = 0.5f * (x0[x0offset + 0] - x1[x1offset + 0]);

                w0[w0offset + 0] = r0 + r2;
                w1[w1offset + 2] = r0 - r2;
                w0[w0offset + 1] = r1 + r3;
                w1[w1offset + 3] = r3 - r1;

                x0 = x.floats;
                x0offset = xoffset + bit[bitoffset + 2];
                x1 = x.floats;
                x1offset = xoffset + bit[bitoffset + 3];

                r0 = x0[x0offset + 1] - x1[x1offset + 1];
                r1 = x0[x0offset + 0] + x1[x1offset + 0];
                r2 = (r1 * T[Toffset + 2] + r0 * T[Toffset + 3]);
                r3 = (r1 * T[Toffset + 3] - r0 * T[Toffset + 2]);

                r0 = 0.5f * (x0[x0offset + 1] + x1[x1offset + 1]);
                r1 = 0.5f * (x0[x0offset + 0] - x1[x1offset + 0]);

                w0[w0offset + 2] = r0 + r2;
                w1[w1offset + 0] = r0 - r2;
                w0[w0offset + 3] = r1 + r3;
                w1[w1offset + 1] = r3 - r1;

                Toffset += 4;
                bitoffset += 4;
                w0offset += 4;

            } while (w0offset < w1offset);
        }


        public static void mdct_backward(mdct_lookup init, CPtr.FloatPtr pin, CPtr.FloatPtr pout)
        {
            int n = init.n;
            int n2 = n >> 1;
            int n4 = n >> 2;

            /* rotate */

            float[] iX = pin.floats;
            int iXoffset = pin.offset + n2 - 7;
            float[] oX = pout.floats;
            int oXoffset = pout.offset + n2 + n4;
            float[] T = init.trig;
            int Toffset = n4;

            do
            {
                oXoffset -= 4;
                oX[oXoffset + 0] = (-iX[iXoffset + 2] * T[Toffset + 3] - iX[iXoffset + 0] * T[Toffset + 2]);
                oX[oXoffset + 1] = (iX[iXoffset + 0] * T[Toffset + 3] - iX[iXoffset + 2] * T[Toffset + 2]);
                oX[oXoffset + 2] = (-iX[iXoffset + 6] * T[Toffset + 1] - iX[iXoffset + 4] * T[Toffset + 0]);
                oX[oXoffset + 3] = (iX[iXoffset + 4] * T[Toffset + 1] - iX[iXoffset + 6] * T[Toffset + 0]);
                iXoffset -= 8;
                Toffset += 4;
            } while (iXoffset >= pin.offset);

            iX = pin.floats;
            iXoffset = pin.offset + n2 - 8;
            oX = pout.floats;
            oXoffset = pout.offset + n2 + n4;
            T = init.trig;
            Toffset = n4;

            do
            {
                Toffset -= 4;
                oX[oXoffset + 0] = (iX[iXoffset + 4] * T[Toffset + 3] + iX[iXoffset + 6] * T[Toffset + 2]);
                oX[oXoffset + 1] = (iX[iXoffset + 4] * T[Toffset + 2] - iX[iXoffset + 6] * T[Toffset + 3]);
                oX[oXoffset + 2] = (iX[iXoffset + 0] * T[Toffset + 1] + iX[iXoffset + 2] * T[Toffset + 0]);
                oX[oXoffset + 3] = (iX[iXoffset + 0] * T[Toffset + 0] - iX[iXoffset + 2] * T[Toffset + 1]);
                iXoffset -= 8;
                oXoffset += 4;
            } while (iXoffset >= pin.offset);

            mdct_butterflies(init, new CPtr.FloatPtr(pout, n2), n2);
            mdct_bitreverse(init, pout);

            /* roatate + window */

            {
                float[] oX1 = pout.floats;
                int oX1offset = pout.offset + n2 + n4;
                float[] oX2 = pout.floats;
                int oX2offset = pout.offset + n2 + n4;
                iX = pout.floats;
                iXoffset = pout.offset;
                T = init.trig;
                Toffset = n2;

                do
                {
                    oX1offset -= 4;

                    oX1[oX1offset + 3] = (iX[iXoffset + 0] * T[Toffset + 1] - iX[iXoffset + 1] * T[Toffset + 0]);
                    oX2[oX2offset + 0] = -(iX[iXoffset + 0] * T[Toffset + 0] + iX[iXoffset + 1] * T[Toffset + 1]);

                    oX1[oX1offset + 2] = (iX[iXoffset + 2] * T[Toffset + 3] - iX[iXoffset + 3] * T[Toffset + 2]);
                    oX2[oX2offset + 1] = -(iX[iXoffset + 2] * T[Toffset + 2] + iX[iXoffset + 3] * T[Toffset + 3]);

                    oX1[oX1offset + 1] = (iX[iXoffset + 4] * T[Toffset + 5] - iX[iXoffset + 5] * T[Toffset + 4]);
                    oX2[oX2offset + 2] = -(iX[iXoffset + 4] * T[Toffset + 4] + iX[iXoffset + 5] * T[Toffset + 5]);

                    oX1[oX1offset + 0] = (iX[iXoffset + 6] * T[Toffset + 7] - iX[iXoffset + 7] * T[Toffset + 6]);
                    oX2[oX2offset + 3] = -(iX[iXoffset + 6] * T[Toffset + 6] + iX[iXoffset + 7] * T[Toffset + 7]);

                    oX2offset += 4;
                    iXoffset += 8;
                    Toffset += 8;
                } while (iXoffset < oX1offset);

                iX = pout.floats;
                iXoffset = pout.offset + n2 + n4;
                oX1 = pout.floats;
                oX1offset = pout.offset + n4;
                oX2 = oX1;
                oX2offset = oX1offset;

                do
                {
                    oX1offset -= 4;
                    iXoffset -= 4;

                    oX2[oX2offset + 0] = -(oX1[oX1offset + 3] = iX[iXoffset + 3]);
                    oX2[oX2offset + 1] = -(oX1[oX1offset + 2] = iX[iXoffset + 2]);
                    oX2[oX2offset + 2] = -(oX1[oX1offset + 1] = iX[iXoffset + 1]);
                    oX2[oX2offset + 3] = -(oX1[oX1offset + 0] = iX[iXoffset + 0]);

                    oX2offset += 4;
                } while (oX2offset < iXoffset);

                iX = pout.floats;
                iXoffset = pout.offset + n2 + n4;
                oX1 = pout.floats;
                oX1offset = pout.offset + n2 + n4;
                //oX2 = out.floats;
                oX2offset = pout.offset + n2;
                do
                {
                    oX1offset -= 4;
                    oX1[oX1offset + 0] = iX[iXoffset + 3];
                    oX1[oX1offset + 1] = iX[iXoffset + 2];
                    oX1[oX1offset + 2] = iX[iXoffset + 1];
                    oX1[oX1offset + 3] = iX[iXoffset + 0];
                    iXoffset += 4;
                } while (oX1offset > oX2offset);
            }
        }
    }
}
