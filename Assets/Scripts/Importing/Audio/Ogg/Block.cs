using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TanjentOGG is released under the 3-clause BSD license. Please read license.txt for the full license.
namespace TanjentOGG
{
    public class Block
    {
        static int ilog2(int v)
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

        public static int vorbis_block_init(Codec.vorbis_dsp_state v, Codec.vorbis_block vb)
        {
            vb.clear();
            vb.vd = v;
            return (0);
        }

        /* Analysis side code, but directly related to blocking.  Thus it's
       here and not in analysis.c (which is for analysis transforms only).
       The init is here because some of it is shared */

        static int _vds_shared_init(Registry r, Codec.vorbis_dsp_state v, Codec.vorbis_info vi, int encp)
        {
            int i;
            Codec.codec_setup_info ci = vi.codec_setup;
            Codec.private_state b;
            int hs;

            if (ci == null) return 1;
            hs = ci.halfrate_flag;
            v.clear();

            b = v.backend_state = new Codec.private_state();

            v.vi = vi;
            b.modebits = ilog2(ci.modes);

            // TODO delete this
            //b.transform[0] = new Mdct.mdct_lookup[Registry.VI_TRANSFORMB];
            //b.transform[1] = new Mdct.mdct_lookup[Registry.VI_TRANSFORMB];

            /* MDCT is tranform 0 */

            b.transform[0,0] = new Mdct.mdct_lookup();
            b.transform[1,0] = new Mdct.mdct_lookup();
            Mdct.mdct_init(b.transform[0,0], (int)(ci.blocksizes[0] >> hs));
            Mdct.mdct_init(b.transform[1,0], (int)(ci.blocksizes[1] >> hs));

            /* Vorbis I uses only window type 0 */
            b.window[0] = ilog2((int)ci.blocksizes[0]) - 6;
            b.window[1] = ilog2((int)ci.blocksizes[1]) - 6;

            /* finish the codebooks */
            if (ci.fullbooks == null)
            {
                ci.fullbooks = new Codebook.codebook[ci.books];
                for (int j = 0; j < ci.books; j++)
                {
                    ci.fullbooks[j] = new Codebook.codebook();
                }
                for (i = 0; i < ci.books; i++)
                {
                    if (ci.book_param[i] == null)
                        return -1;
                    if (Sharedbook.vorbis_book_init_decode(ci.fullbooks[i], ci.book_param[i]) != 0)
                        return -1;
                    /* decode codebooks are now standalone after init */
                    ci.book_param[i] = null;
                }
            }


            /* initialize the storage vectors. blocksize[1] is small for encode,
               but the correct size for decode */
            v.pcm_storage = (int)ci.blocksizes[1];
            v.pcm = new CPtr.FloatPtr[vi.channels];
            v.pcmret = new CPtr.FloatPtr[vi.channels];
            {
                for (i = 0; i < vi.channels; i++)
                    v.pcm[i] = new CPtr.FloatPtr(new float[v.pcm_storage]);
            }

            /* all 1 (large block) or 0 (small block) */
            /* explicitly set for the sake of clarity */
            v.lW = 0; /* previous window size */
            v.W = 0;  /* current window size */

            /* all vector indexes */
            v.centerW = ci.blocksizes[1] / 2;

            v.pcm_current = (int)v.centerW;

            /* initialize all the backend lookups */
            b.flr = new Codec.vorbis_look_floor[ci.floors];
            b.residue = new Codec.vorbis_look_residue[ci.residues];

            for (i = 0; i < ci.floors; i++)
                b.flr[i] = r._floor_P[ci.floor_type[i]].look(v, ci.floor_param[i]);

            for (i = 0; i < ci.residues; i++)
                b.residue[i] = r._residue_P[ci.residue_type[i]].look(v, ci.residue_param[i]);

            return 0;
        }


        static int vorbis_synthesis_restart(Codec.vorbis_dsp_state v)
        {
            Codec.vorbis_info vi = v.vi;
            Codec.codec_setup_info ci;
            int hs;

            if (v.backend_state == null) return -1;
            if (vi == null) return -1;
            ci = vi.codec_setup;
            if (ci == null) return -1;
            hs = ci.halfrate_flag;

            v.centerW = ci.blocksizes[1] >> (hs + 1);
            v.pcm_current = (int)(v.centerW >> hs);

            v.pcm_returned = -1;
            v.granulepos = -1;
            v.sequence = -1;
            v.eofflag = 0;
            v.backend_state.sample_count = -1;

            return (0);
        }


        public static int vorbis_synthesis_init(Registry r, Codec.vorbis_dsp_state v, Codec.vorbis_info vi)
        {
            if (_vds_shared_init(r, v, vi, 0) != 0)
            {
                return 1;
            }
            vorbis_synthesis_restart(v);
            return 0;
        }

        /* Unlike in analysis, the window is only partially applied for each
       block.  The time domain envelope is not yet handled at the point of
       calling (as it relies on the previous block). */

        public static int vorbis_synthesis_blockin(Registry r, Codec.vorbis_dsp_state v, Codec.vorbis_block vb)
        {
            Codec.vorbis_info vi = v.vi;
            Codec.codec_setup_info ci = vi.codec_setup;
            Codec.private_state b = v.backend_state;
            int hs = ci.halfrate_flag;
            int i, j;

            if (vb == null) return (Codec.OV_EINVAL);
            if (v.pcm_current > v.pcm_returned && v.pcm_returned != -1) return (Codec.OV_EINVAL);

            v.lW = v.W;
            v.W = vb.W;
            v.nW = -1;

            if ((v.sequence == -1) ||
                    (v.sequence + 1 != vb.sequence))
            {
                v.granulepos = -1; /* out of sequence; lose count */
                b.sample_count = -1;
            }

            v.sequence = vb.sequence;

            if (vb.pcm != null)
            {  /* no pcm to process if vorbis_synthesis_trackonly
                   was called on block */
                int n = (int)(ci.blocksizes[((int)v.W)] >> (hs + 1));
                int n0 = (int)(ci.blocksizes[0] >> (hs + 1));
                int n1 = (int)(ci.blocksizes[1] >> (hs + 1));

                int thisCenter;
                int prevCenter;

                v.glue_bits += vb.glue_bits;
                v.time_bits += vb.time_bits;
                v.floor_bits += vb.floor_bits;
                v.res_bits += vb.res_bits;

                if (v.centerW != 0)
                {
                    thisCenter = n1;
                    prevCenter = 0;
                }
                else
                {
                    thisCenter = 0;
                    prevCenter = n1;
                }

                /* v.pcm is now used like a two-stage double buffer.  We don't want
                   to have to constantly shift *or* adjust memory usage.  Don't
                   accept a new block until the old is shifted out */

                for (j = 0; j < vi.channels; j++)
                {
                    /* the overlap/add section */
                    if (v.lW != 0)
                    {
                        if (v.W != 0)
                        {
                            /* large/large */
                            float[] w = r.vwin[b.window[1] - hs];
                            CPtr.FloatPtr pcm = new CPtr.FloatPtr(v.pcm[j], prevCenter);
                            CPtr.FloatPtr p = new CPtr.FloatPtr(vb.pcm[j]);
                            for (i = 0; i < n1; i++)
                                pcm.floats[pcm.offset + i] = pcm.floats[pcm.offset + i] * w[n1 - i - 1] + p.floats[p.offset + i] * w[i];
                        }
                        else
                        {
                            /* large/small */
                            float[] w = r.vwin[b.window[0] - hs];
                            CPtr.FloatPtr pcm = new CPtr.FloatPtr(v.pcm[j], prevCenter + n1 / 2 - n0 / 2);
                            CPtr.FloatPtr p = new CPtr.FloatPtr(vb.pcm[j]);
                            for (i = 0; i < n0; i++)
                                pcm.floats[pcm.offset + i] = pcm.floats[pcm.offset + i] * w[n0 - i - 1] + p.floats[p.offset + i] * w[i];
                        }
                    }
                    else
                    {
                        if (v.W != 0)
                        {
                            /* small/large */
                            float[] w = r.vwin[b.window[0] - hs];
                            CPtr.FloatPtr pcm = new CPtr.FloatPtr(v.pcm[j], prevCenter);
                            CPtr.FloatPtr p = new CPtr.FloatPtr(vb.pcm[j], n1 / 2 - n0 / 2);
                            for (i = 0; i < n0; i++)
                                pcm.floats[pcm.offset + i] = pcm.floats[pcm.offset + i] * w[n0 - i - 1] + p.floats[p.offset + i] * w[i];
                            for (; i < n1 / 2 + n0 / 2; i++)
                                pcm.floats[pcm.offset + i] = p.floats[p.offset + i];
                        }
                        else
                        {
                            /* small/small */
                            float[] w = r.vwin[b.window[0] - hs];
                            CPtr.FloatPtr pcm = new CPtr.FloatPtr(v.pcm[j], prevCenter);
                            CPtr.FloatPtr p = new CPtr.FloatPtr(vb.pcm[j]);
                            for (i = 0; i < n0; i++)
                                pcm.floats[pcm.offset + i] = pcm.floats[pcm.offset + i] * w[n0 - i - 1] + p.floats[p.offset + i] * w[i];
                        }
                    }

                    /* the copy section */
                    {

                        CPtr.FloatPtr pcm = new CPtr.FloatPtr(v.pcm[j], thisCenter);
                        CPtr.FloatPtr p = new CPtr.FloatPtr(vb.pcm[j], n);
                        for (i = 0; i < n; i++)
                            pcm.floats[pcm.offset + i] = p.floats[p.offset + i];
                    }
                }

                if (v.centerW != 0)
                    v.centerW = 0;
                else
                    v.centerW = n1;

                /* deal with initial packet state; we do this using the explicit
                   pcm_returned==-1 flag otherwise we're sensitive to first block
                   being short or long */

                if (v.pcm_returned == -1)
                {
                    v.pcm_returned = thisCenter;
                    v.pcm_current = thisCenter;
                }
                else
                {
                    v.pcm_returned = prevCenter;
                    v.pcm_current = (int)(prevCenter +
                            ((ci.blocksizes[((int)v.lW)] / 4 +
                                    ci.blocksizes[((int)v.W)] / 4) >> hs));
                }

            }

            /* track the frame number... This is for convenience, but also
               making sure our last packet doesn't end with added padding.  If
               the last packet is partial, the number of samples we'll have to
               return will be past the vb.granulepos.

               This is not foolproof!  It will be confused if we begin
               decoding at the last page after a seek or hole.  In that case,
               we don't have a starting point to judge where the last frame
               is.  For this reason, vorbisfile will always try to make sure
               it reads the last two marked pages in proper sequence */

            if (b.sample_count == -1)
            {
                b.sample_count = 0;
            }
            else
            {
                b.sample_count += ci.blocksizes[((int)v.lW)] / 4 + ci.blocksizes[((int)v.W)] / 4;
            }

            if (v.granulepos == -1)
            {
                if (vb.granulepos != -1)
                { /* only set if we have a position to set to */

                    v.granulepos = vb.granulepos;

                    /* is this a short page? */
                    if (b.sample_count > v.granulepos)
                    {
                        /* corner case; if this is both the first and last audio page,
                           then spec says the end is cut, not beginning */
                        long extra = b.sample_count - vb.granulepos;

                        /* we use ogg_int64_t for granule positions because a
                           uint64 isn't universally available.  Unfortunately,
                           that means granposes can be 'negative' and result in
                           extra being negative */
                        if (extra < 0)
                            extra = 0;

                        if (vb.eofflag != 0)
                        {
                            /* trim the end */
                            /* no preceding granulepos; assume we started at zero (we'd
                               have to in a short single-page stream) */
                            /* granulepos could be -1 due to a seek, but that would result
                               in a long count, not short count */

                            /* Guard against corrupt/malicious frames that set EOP and
                               a backdated granpos; don't rewind more samples than we
                               actually have */
                            if (extra > (v.pcm_current - v.pcm_returned) << hs)
                                extra = (v.pcm_current - v.pcm_returned) << hs;

                            v.pcm_current -= (int)(extra >> hs);
                        }
                        else
                        {
                            /* trim the beginning */
                            v.pcm_returned += (int)(extra >> hs);
                            if (v.pcm_returned > v.pcm_current)
                                v.pcm_returned = v.pcm_current;
                        }

                    }

                }
            }
            else
            {
                v.granulepos += ci.blocksizes[((int)v.lW)] / 4 + ci.blocksizes[((int)v.W)] / 4;
                if (vb.granulepos != -1 && v.granulepos != vb.granulepos)
                {

                    if (v.granulepos > vb.granulepos)
                    {
                        long extra = v.granulepos - vb.granulepos;

                        if (extra != 0)
                            if (vb.eofflag != 0)
                            {
                                /* partial last frame.  Strip the extra samples off */

                                /* Guard against corrupt/malicious frames that set EOP and
                                   a backdated granpos; don't rewind more samples than we
                                   actually have */
                                if (extra > (v.pcm_current - v.pcm_returned) << hs)
                                    extra = (v.pcm_current - v.pcm_returned) << hs;

                                /* we use ogg_int64_t for granule positions because a
                                   uint64 isn't universally available.  Unfortunately,
                                   that means granposes can be 'negative' and result in
                                   extra being negative */
                                if (extra < 0)
                                    extra = 0;

                                v.pcm_current -= (int)(extra >> hs);
                            } /* else {Shouldn't happen *unless* the bitstream is out of
               spec.  Either way, believe the bitstream } */
                    } /* else {Shouldn't happen *unless* the bitstream is out of
           spec.  Either way, believe the bitstream } */
                    v.granulepos = vb.granulepos;
                }
            }

            /* Update, cleanup */

            if (vb.eofflag != 0) v.eofflag = 1;
            return (0);

        }

        /* pcm==NULL indicates we just want the pending samples, no more */
        public static int vorbis_synthesis_pcmout(Codec.vorbis_dsp_state v, CPtr.FloatPtr[][] pcm)
        {
            Codec.vorbis_info vi = v.vi;

            if (v.pcm_returned > -1 && v.pcm_returned < v.pcm_current)
            {
                if (pcm != null)
                {
                    int i;
                    for (i = 0; i < vi.channels; i++)
                    {
                        v.pcmret[i] = new CPtr.FloatPtr(v.pcm[i], v.pcm_returned);
                    }
                    pcm[0] = v.pcmret;
                }
                return (v.pcm_current - v.pcm_returned);
            }
            return (0);
        }

        public static int vorbis_synthesis_read(Codec.vorbis_dsp_state v, int n)
        {
            if (n != 0 && v.pcm_returned + n > v.pcm_current) return (Codec.OV_EINVAL);
            v.pcm_returned += n;
            return (0);
        }
    }
}
