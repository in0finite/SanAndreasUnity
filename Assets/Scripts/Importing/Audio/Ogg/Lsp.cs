using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TanjentOGG is released under the 3-clause BSD license. Please read license.txt for the full license.
namespace TanjentOGG
{
    public class Lsp
    {
        private static float fromdB(float x)
        {
            return (float)Math.Exp((x) * .11512925f);
        }

        /* old, nonoptimized but simple version for any poor sap who needs to
       figure out what the hell this code does, or wants the other
       fraction of a dB precision */

        /* side effect: changes *lsp to cosines of lsp */
        public static void vorbis_lsp_to_curve(CPtr.FloatPtr curve, int[] map, int n, int ln, float[] lsp, int m, float amp, float ampoffset)
        {
            int i;
            float wdel = (float)(Math.PI / ln);
            for (i = 0; i < m; i++) lsp[i] = (float)(2f * Math.Cos(lsp[i]));

            i = 0;
            while (i < n)
            {
                int j, k = map[i];
                float p = .5f;
                float q = .5f;
                float w = (float)(2f * Math.Cos(wdel * k));
                for (j = 1; j < m; j += 2)
                {
                    q *= w - lsp[j - 1];
                    p *= w - lsp[j];
                }
                if (j == m)
                {
                    /* odd order filter; slightly assymetric */
                    /* the last coefficient */
                    q *= w - lsp[j - 1];
                    p *= p * (4f - w * w);
                    q *= q;
                }
                else
                {
                    /* even order filter; still symmetric */
                    p *= p * (2f - w);
                    q *= q * (2f + w);
                }

                q = fromdB((float)(amp / Math.Sqrt(p + q) - ampoffset));

                curve.floats[curve.offset + i] *= q;
                while (map[++i] == k) curve.floats[curve.offset + i] *= q;
            }
        }

    }
}
