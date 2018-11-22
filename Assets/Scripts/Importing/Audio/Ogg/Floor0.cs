using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TanjentOGG is released under the 3-clause BSD license. Please read license.txt for the full license.
namespace TanjentOGG
{
    public class Floor0
    {
        private static float toBARK(float n)
        {
            return (float)(13.1f * Math.Atan(.00074f * (n)) + 2.24f * Math.Atan((n) * (n) * 1.85e-8f) + 1e-4f * (n));
        }

        public class vorbis_look_floor0 : Codec.vorbis_look_floor
        {
            public int ln;
            public int m;
            public int[][] linearmap;
            public int[] n = new int[2];

            public vorbis_info_floor0 vi;

        }

        public class vorbis_info_floor0 : Codec.vorbis_info_floor
        {
            public int order;
            public long rate;
            public long barkmap;

            public int ampbits;
            public int ampdB;

            public int numbooks; /* <= 16 */
            public int[] books = new int[16];
        }

        public static Codec.vorbis_info_floor floor0_unpack(Codec.vorbis_info vi, Vogg.oggpack_buffer opb)
        {
            Codec.codec_setup_info ci = vi.codec_setup;
            int j;

            vorbis_info_floor0 info = new vorbis_info_floor0();
            info.order = (int)Bitwise.oggpack_read(opb, 8);
            info.rate = Bitwise.oggpack_read(opb, 16);
            info.barkmap = Bitwise.oggpack_read(opb, 16);
            info.ampbits = (int)Bitwise.oggpack_read(opb, 6);
            info.ampdB = (int)Bitwise.oggpack_read(opb, 8);
            info.numbooks = (int)(Bitwise.oggpack_read(opb, 4) + 1);

            if (info.order < 1) return null;
            if (info.rate < 1) return null;
            if (info.barkmap < 1) return null;
            if (info.numbooks < 1) return null;

            for (j = 0; j < info.numbooks; j++)
            {
                info.books[j] = (int)Bitwise.oggpack_read(opb, 8);
                if (info.books[j] < 0 || info.books[j] >= ci.books) return null;
                if (ci.book_param[info.books[j]].maptype == 0) return null;
                if (ci.book_param[info.books[j]].dim < 1) return null;
            }
            return (info);

        }

        /* initialize Bark scale and normalization lookups.  We could do this
           with static tables, but Vorbis allows a number of possible
           combinations, so it's best to do it computationally.

           The below is authoritative in terms of defining scale mapping.
           Note that the scale depends on the sampling rate as well as the
           linear block and mapping sizes */

        static void floor0_map_lazy_init(Codec.vorbis_block vb,
                                         Codec.vorbis_info_floor infoX,
                                         vorbis_look_floor0 look)
        {
            if (look.linearmap[((int)vb.W)] == null)
            {
                Codec.vorbis_dsp_state vd = vb.vd;
                Codec.vorbis_info vi = vd.vi;
                Codec.codec_setup_info ci = vi.codec_setup;
                vorbis_info_floor0 info = (vorbis_info_floor0)infoX;
                int W = (int)vb.W;
                int n = (int)(ci.blocksizes[W] / 2), j;

                /* we choose a scaling constant so that:
                   floor(bark(rate/2-1)*C)=mapped-1
                 floor(bark(rate/2)*C)=mapped */
                float scale = look.ln / toBARK(info.rate / 2f);

                /* the mapping from a linear scale to a smaller bark scale is
                   straightforward.  We do *not* make sure that the linear mapping
                   does not skip bark-scale bins; the decoder simply skips them and
                   the encoder may do what it wishes in filling them.  They're
                   necessary in some mapping combinations to keep the scale spacing
                   accurate */
                look.linearmap[W] = new int[(n + 1)];
                for (j = 0; j < n; j++)
                {
                    int val = (int)Math.Floor(toBARK((info.rate / 2f) / n * j) * scale); /* bark numbers represent band edges */
                    if (val >= look.ln) val = look.ln - 1; /* guard against the approximation */
                    look.linearmap[W][j] = val;
                }
                look.linearmap[W][j] = -1;
                look.n[W] = n;
            }
        }

        public static Codec.vorbis_look_floor floor0_look(Codec.vorbis_dsp_state vd, Codec.vorbis_info_floor i)
        {
            vorbis_info_floor0 info = (vorbis_info_floor0)i;
            vorbis_look_floor0 look = new vorbis_look_floor0();

            look.m = info.order;
            look.ln = (int)info.barkmap;
            look.vi = info;

            look.linearmap = new int[2][];

            return look;
        }

        public static float[] floor0_inverse1(Codec.vorbis_block vb, Codec.vorbis_look_floor i)
        {
            vorbis_look_floor0 look = (vorbis_look_floor0)i;
            vorbis_info_floor0 info = look.vi;
            int j, k;

            int ampraw = (int)Bitwise.oggpack_read(vb.opb, info.ampbits);
            if (ampraw > 0)
            { /* also handles the -1 out of data case */
                long maxval = (1 << info.ampbits) - 1;
                float amp = (float)ampraw / maxval * info.ampdB;
                int booknum = (int)Bitwise.oggpack_read(vb.opb, Sharedbook._ilog(info.numbooks));

                if (booknum != -1 && booknum < info.numbooks)
                { /* be paranoid */
                    Codec.codec_setup_info ci = vb.vd.vi.codec_setup;
                    Codebook.codebook b = ci.fullbooks[info.books[booknum]];
                    float last = 0f;

                    /* the additional b.dim is a guard against any possible stack
                       smash; b.dim is provably more than we can overflow the
                       vector */
                    float[] lsp = new float[(int)(look.m + b.dim + 1)];

                    if (Codebook.vorbis_book_decodev_set(b, lsp, vb.opb, look.m) == -1) return null;
                    for (j = 0; j < look.m; )
                    {
                        for (k = 0; j < look.m && k < b.dim; k++, j++) lsp[j] += last;
                        last = lsp[j - 1];
                    }

                    lsp[look.m] = amp;
                    return (lsp);
                }
            }
            return null;
        }

        public static int floor0_inverse2(Codec.vorbis_block vb, Codec.vorbis_look_floor i, float[] memo, CPtr.FloatPtr pout)
        {
            vorbis_look_floor0 look = (vorbis_look_floor0)i;
            vorbis_info_floor0 info = look.vi;

            floor0_map_lazy_init(vb, info, look);

            if (memo != null)
            {
                float[] lsp = memo;
                float amp = lsp[look.m];

                /* take the coefficients back to a spectral envelope curve */
                Lsp.vorbis_lsp_to_curve(pout,
                        look.linearmap[((int)vb.W)],
                        look.n[((int)vb.W)],
                        look.ln,
                        lsp, look.m, amp, (float)info.ampdB);
                return (1);
            }

            CPtr.FloatPtr.memset(pout, 0, pout.floats.Length - pout.offset);
            return (0);
        }
    }
}
