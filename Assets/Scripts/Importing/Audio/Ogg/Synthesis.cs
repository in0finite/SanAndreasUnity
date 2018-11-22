using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TanjentOGG is released under the 3-clause BSD license. Please read license.txt for the full license.
namespace TanjentOGG
{
    public class Synthesis
    {
        public static int vorbis_synthesis(Registry r, Codec.vorbis_block vb, Vogg.ogg_packet op)
        {
            Codec.vorbis_dsp_state vd = vb != null ? vb.vd : null;
            Codec.private_state b = vd != null ? vd.backend_state : null;
            Codec.vorbis_info vi = vd != null ? vd.vi : null;
            Codec.codec_setup_info ci = vi != null ? vi.codec_setup : null;
            Vogg.oggpack_buffer opb = vb != null ? vb.opb : null;
            int type, mode, i;

            if (vd == null || b == null || vi == null || ci == null || opb == null)
            {
                return Codec.OV_EBADPACKET;
            }

            /* first things first.  Make sure decode is ready */
            Bitwise.oggpack_readinit(opb, op.packet, (int)op.bytes);

            /* Check the packet type */
            if (Bitwise.oggpack_read(opb, 1) != 0)
            {
                /* Oops.  This is not an audio data packet */
                return (Codec.OV_ENOTAUDIO);
            }

            /* read our mode and pre/post windowsize */
            mode = (int)Bitwise.oggpack_read(opb, b.modebits);
            if (mode == -1)
            {
                return (Codec.OV_EBADPACKET);
            }

            vb.mode = mode;
            if (ci.mode_param[mode] == null)
            {
                return (Codec.OV_EBADPACKET);
            }

            vb.W = ci.mode_param[mode].blockflag;
            if (vb.W != 0)
            {

                /* this doesn;t get mapped through mode selection as it's used
                   only for window selection */
                vb.lW = Bitwise.oggpack_read(opb, 1);
                vb.nW = Bitwise.oggpack_read(opb, 1);
                if (vb.nW == -1)
                {
                    return (Codec.OV_EBADPACKET);
                }
            }
            else
            {
                vb.lW = 0;
                vb.nW = 0;
            }

            /* more setup */
            vb.granulepos = op.granulepos;
            vb.sequence = op.packetno;
            vb.eofflag = (int)op.e_o_s;

            /* alloc pcm passback storage */
            vb.pcmend = (int)ci.blocksizes[((int)vb.W)];
            vb.pcm = new CPtr.FloatPtr[vi.channels];
            for (i = 0; i < vi.channels; i++)
                vb.pcm[i] = new CPtr.FloatPtr(new float[vb.pcmend]);

            /* unpack_header enforces range checking */
            type = ci.map_type[ci.mode_param[mode].mapping];

            return (r._mapping_P[type].inverse(r, vb, ci.map_param[ci.mode_param[mode].mapping]));
        }
    }
}
