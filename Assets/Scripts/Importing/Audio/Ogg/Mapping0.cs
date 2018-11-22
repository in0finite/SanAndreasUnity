using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TanjentOGG is released under the 3-clause BSD license. Please read license.txt for the full license.
namespace TanjentOGG
{
    public class Mapping0
    {
        static int ilog(int v)
        {
            int ret = 0;
            if (v != 0) --v;
            while (v != 0)
            {
                ret++;
                v >>= 1;
            }
            return (ret);
        }

        /* also responsible for range checking */
        public static Codec.vorbis_info_mapping mapping0_unpack(Codec.vorbis_info vi, Vogg.oggpack_buffer opb)
        {
            int i, b;
            Registry.vorbis_info_mapping0 info = new Registry.vorbis_info_mapping0();
            Codec.codec_setup_info ci = vi.codec_setup;
            info.clear();

            b = (int)Bitwise.oggpack_read(opb, 1);
            if (b < 0) return null;
            if (b != 0)
            {
                info.submaps = (int)(Bitwise.oggpack_read(opb, 4) + 1);
                if (info.submaps <= 0) return null;
            }
            else
                info.submaps = 1;

            b = (int)Bitwise.oggpack_read(opb, 1);
            if (b < 0) return null;
            if (b != 0)
            {
                info.coupling_steps = (int)(Bitwise.oggpack_read(opb, 8) + 1);
                if (info.coupling_steps <= 0) return null;
                for (i = 0; i < info.coupling_steps; i++)
                {
                    int testM = info.coupling_mag[i] = (int)Bitwise.oggpack_read(opb, ilog(vi.channels));
                    int testA = info.coupling_ang[i] = (int)Bitwise.oggpack_read(opb, ilog(vi.channels));

                    if (testM < 0 ||
                            testA < 0 ||
                            testM == testA ||
                            testM >= vi.channels ||
                            testA >= vi.channels) return null;
                }

            }

            if (Bitwise.oggpack_read(opb, 2) != 0) return null; /* 2,3:reserved */

            if (info.submaps > 1)
            {
                for (i = 0; i < vi.channels; i++)
                {
                    info.chmuxlist[i] = (int)Bitwise.oggpack_read(opb, 4);
                    if (info.chmuxlist[i] >= info.submaps || info.chmuxlist[i] < 0) return null;
                }
            }
            for (i = 0; i < info.submaps; i++)
            {
                Bitwise.oggpack_read(opb, 8); /* time submap unused */
                info.floorsubmap[i] = (int)Bitwise.oggpack_read(opb, 8);
                if (info.floorsubmap[i] >= ci.floors || info.floorsubmap[i] < 0) return null;
                info.residuesubmap[i] = (int)Bitwise.oggpack_read(opb, 8);
                if (info.residuesubmap[i] >= ci.residues || info.residuesubmap[i] < 0) return null;
            }

            return info;

        }

        public static int mapping0_inverse(Registry r, Codec.vorbis_block vb, Codec.vorbis_info_mapping l)
        {
            Codec.vorbis_dsp_state vd = vb.vd;
            Codec.vorbis_info vi = vd.vi;
            Codec.codec_setup_info ci = vi.codec_setup;
            Codec.private_state b = vd.backend_state;
            Registry.vorbis_info_mapping0 info = (Registry.vorbis_info_mapping0)l;

            int i, j;
            long n = vb.pcmend = (int)ci.blocksizes[((int)vb.W)];

            CPtr.FloatPtr[] pcmbundle = new CPtr.FloatPtr[vi.channels];
            int[] zerobundle = new int[vi.channels];

            int[] nonzero = new int[vi.channels];
            float[][] floormemo = new float[vi.channels][];

            /* recover the spectral envelope; store it in the PCM vector for now */
            for (i = 0; i < vi.channels; i++)
            {
                int submap = info.chmuxlist[i];
                floormemo[i] = r._floor_P[ci.floor_type[info.floorsubmap[submap]]].inverse1(vb, b.flr[info.floorsubmap[submap]]);
                if (floormemo[i] != null)
                    nonzero[i] = 1;
                else
                    nonzero[i] = 0;
                CPtr.FloatPtr.memset(vb.pcm[i], 0, n / 2);
            }

            /* channel coupling can 'dirty' the nonzero listing */
            for (i = 0; i < info.coupling_steps; i++)
            {
                if (nonzero[info.coupling_mag[i]] != 0 || nonzero[info.coupling_ang[i]] != 0)
                {
                    nonzero[info.coupling_mag[i]] = 1;
                    nonzero[info.coupling_ang[i]] = 1;
                }
            }

            /* recover the residue into our working vectors */
            for (i = 0; i < info.submaps; i++)
            {
                int ch_in_bundle = 0;
                for (j = 0; j < vi.channels; j++)
                {
                    if (info.chmuxlist[j] == i)
                    {
                        if (nonzero[j] != 0)
                            zerobundle[ch_in_bundle] = 1;
                        else
                            zerobundle[ch_in_bundle] = 0;
                        pcmbundle[ch_in_bundle++] = new CPtr.FloatPtr(vb.pcm[j]);
                    }
                }

                r._residue_P[ci.residue_type[info.residuesubmap[i]]].inverse(vb, b.residue[info.residuesubmap[i]], pcmbundle, zerobundle, ch_in_bundle);
            }

            /* channel coupling */
            for (i = info.coupling_steps - 1; i >= 0; i--)
            {
                CPtr.FloatPtr pcmM = vb.pcm[info.coupling_mag[i]];
                CPtr.FloatPtr pcmA = vb.pcm[info.coupling_ang[i]];

                for (j = 0; j < n / 2; j++)
                {
                    float mag = pcmM.floats[pcmM.offset + j];
                    float ang = pcmA.floats[pcmA.offset + j];

                    if (mag > 0)
                        if (ang > 0)
                        {
                            pcmM.floats[pcmM.offset + j] = mag;
                            pcmA.floats[pcmA.offset + j] = mag - ang;
                        }
                        else
                        {
                            pcmA.floats[pcmA.offset + j] = mag;
                            pcmM.floats[pcmM.offset + j] = mag + ang;
                        }
                    else if (ang > 0)
                    {
                        pcmM.floats[pcmM.offset + j] = mag;
                        pcmA.floats[pcmA.offset + j] = mag + ang;
                    }
                    else
                    {
                        pcmA.floats[pcmA.offset + j] = mag;
                        pcmM.floats[pcmM.offset + j] = mag - ang;
                    }
                }
            }

            /* compute and apply spectral envelope */
            for (i = 0; i < vi.channels; i++)
            {
                CPtr.FloatPtr pcm = new CPtr.FloatPtr(vb.pcm[i]);
                int submap = info.chmuxlist[i];
                r._floor_P[ci.floor_type[info.floorsubmap[submap]]].inverse2(vb, b.flr[info.floorsubmap[submap]], floormemo[i], pcm);
            }

            /* transform the PCM data; takes PCM vector, vb; modifies PCM vector */
            /* only MDCT right now.... */
            for (i = 0; i < vi.channels; i++)
            {
                CPtr.FloatPtr pcm = new CPtr.FloatPtr(vb.pcm[i]);
                Mdct.mdct_backward(b.transform[((int)vb.W),0], pcm, pcm);
            }

            /* all done! */
            return (0);
        }
    }
}
