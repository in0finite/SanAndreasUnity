using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TanjentOGG is released under the 3-clause BSD license. Please read license.txt for the full license.
namespace TanjentOGG
{
    public class Info
    {
        public static void _v_readstring(Vogg.oggpack_buffer o, CPtr.BytePtr buf, int bytes)
        {
            CPtr.BytePtr tmpbuf = new CPtr.BytePtr(buf);
            while (bytes-- != 0)
            {
                tmpbuf.bytes[tmpbuf.offset++] = (byte)Bitwise.oggpack_read(o, 8);
            }
        }

        public static void vorbis_comment_init(Codec.vorbis_comment vc)
        {
            vc.clear();
        }

        /* used by synthesis, which has a full, alloced vi */
        public static void vorbis_info_init(Codec.vorbis_info vi)
        {
            vi.clear();
            vi.codec_setup = new Codec.codec_setup_info();
        }

        /* Header packing/unpacking ********************************************/

        public static int _vorbis_unpack_info(Codec.vorbis_info vi, Vogg.oggpack_buffer opb)
        {
            Codec.codec_setup_info ci = vi.codec_setup;
            if (ci == null) return (Codec.OV_EFAULT);

            vi.version = (int)Bitwise.oggpack_read(opb, 32);
            if (vi.version != 0) return (Codec.OV_EVERSION);

            vi.channels = (int)Bitwise.oggpack_read(opb, 8);
            vi.rate = Bitwise.oggpack_read(opb, 32);

            vi.bitrate_upper = Bitwise.oggpack_read(opb, 32);
            vi.bitrate_nominal = Bitwise.oggpack_read(opb, 32);
            vi.bitrate_lower = Bitwise.oggpack_read(opb, 32);

            ci.blocksizes[0] = 1 << (int)Bitwise.oggpack_read(opb, 4);
            ci.blocksizes[1] = 1 << (int)Bitwise.oggpack_read(opb, 4);

            if (vi.rate < 1) return (Codec.OV_EBADHEADER);
            if (vi.channels < 1) return (Codec.OV_EBADHEADER);
            if (ci.blocksizes[0] < 64) return (Codec.OV_EBADHEADER);
            if (ci.blocksizes[1] < ci.blocksizes[0]) return (Codec.OV_EBADHEADER);
            if (ci.blocksizes[1] > 8192) return (Codec.OV_EBADHEADER);

            if (Bitwise.oggpack_read(opb, 1) != 1) return (Codec.OV_EBADHEADER); /* EOP check */

            return (0);
        }

        public static int _vorbis_unpack_comment(Codec.vorbis_comment vc, Vogg.oggpack_buffer opb)
        {
            int i;
            int vendorlen = (int)Bitwise.oggpack_read(opb, 32);
            if (vendorlen < 0) return (Codec.OV_EBADHEADER);
            if (vendorlen > opb.storage - 8) return (Codec.OV_EBADHEADER);
            vc.vendor = new CPtr.BytePtr(new byte[vendorlen + 1]);
            _v_readstring(opb, vc.vendor, vendorlen);
            i = (int)Bitwise.oggpack_read(opb, 32);
            if (i < 0) return (Codec.OV_EBADHEADER);
            if (i > ((opb.storage - Bitwise.oggpack_bytes(opb)) >> 2)) return (Codec.OV_EBADHEADER);
            vc.comments = i;
            vc.user_comments = new CPtr.BytePtr[vc.comments + 1];
            vc.comment_lengths = new int[vc.comments + 1];

            for (i = 0; i < vc.comments; i++)
            {
                int len = (int)Bitwise.oggpack_read(opb, 32);
                if (len < 0) return (Codec.OV_EBADHEADER);
                if (len > opb.storage - Bitwise.oggpack_bytes(opb)) return (Codec.OV_EBADHEADER);
                vc.comment_lengths[i] = len;
                vc.user_comments[i] = new CPtr.BytePtr(new byte[len + 1]);
                _v_readstring(opb, vc.user_comments[i], len);
            }
            if (Bitwise.oggpack_read(opb, 1) != 1) return (Codec.OV_EBADHEADER); /* EOP check */

            return (0);
        }

        /* all of the real encoding details are here.  The modes, books,
           everything */
        static int _vorbis_unpack_books(Registry r, Codec.vorbis_info vi, Vogg.oggpack_buffer opb)
        {
            Codec.codec_setup_info ci = vi.codec_setup;
            int i;
            if (ci == null) return (Codec.OV_EFAULT);

            /* codebooks */
            ci.books = (int)(Bitwise.oggpack_read(opb, 8) + 1);
            if (ci.books <= 0) return (Codec.OV_EBADHEADER);
            for (i = 0; i < ci.books; i++)
            {
                ci.book_param[i] = Codebook.vorbis_staticbook_unpack(opb);
                if (ci.book_param[i] == null) return (Codec.OV_EBADHEADER);
            }

            /* time backend settings; hooks are unused */
            {
                int times = (int)(Bitwise.oggpack_read(opb, 6) + 1);
                if (times <= 0) return (Codec.OV_EBADHEADER);
                for (i = 0; i < times; i++)
                {
                    int test = (int)Bitwise.oggpack_read(opb, 16);
                    if (test < 0 || test >= Registry.VI_TIMEB) return (Codec.OV_EBADHEADER);
                }
            }

            /* floor backend settings */
            ci.floors = (int)(Bitwise.oggpack_read(opb, 6) + 1);
            if (ci.floors <= 0) return (Codec.OV_EBADHEADER);
            for (i = 0; i < ci.floors; i++)
            {
                ci.floor_type[i] = (int)Bitwise.oggpack_read(opb, 16);
                if (ci.floor_type[i] < 0 || ci.floor_type[i] >= Registry.VI_FLOORB) return (Codec.OV_EBADHEADER);
                ci.floor_param[i] = r._floor_P[ci.floor_type[i]].unpack(vi, opb);
                if (ci.floor_param[i] == null) return (Codec.OV_EBADHEADER);
            }

            /* residue backend settings */
            ci.residues = (int)(Bitwise.oggpack_read(opb, 6) + 1);
            if (ci.residues <= 0) return (Codec.OV_EBADHEADER);
            for (i = 0; i < ci.residues; i++)
            {
                ci.residue_type[i] = (int)Bitwise.oggpack_read(opb, 16);
                if (ci.residue_type[i] < 0 || ci.residue_type[i] >= Registry.VI_RESB)
                    return (Codec.OV_EBADHEADER);
                ci.residue_param[i] = r._residue_P[ci.residue_type[i]].unpack(vi, opb);
                if (ci.residue_param[i] == null) return (Codec.OV_EBADHEADER);
            }

            /* map backend settings */
            ci.maps = (int)(Bitwise.oggpack_read(opb, 6) + 1);
            if (ci.maps <= 0) return (Codec.OV_EBADHEADER);
            for (i = 0; i < ci.maps; i++)
            {
                ci.map_type[i] = (int)Bitwise.oggpack_read(opb, 16);
                if (ci.map_type[i] < 0 || ci.map_type[i] >= Registry.VI_MAPB) return (Codec.OV_EBADHEADER);
                ci.map_param[i] = r._mapping_P[ci.map_type[i]].unpack(vi, opb);
                if (ci.map_param[i] == null) return (Codec.OV_EBADHEADER);
            }

            /* mode settings */
            ci.modes = (int)(Bitwise.oggpack_read(opb, 6) + 1);
            if (ci.modes <= 0) return (Codec.OV_EBADHEADER);
            for (i = 0; i < ci.modes; i++)
            {
                ci.mode_param[i] = new Codec.vorbis_info_mode();
                ci.mode_param[i].blockflag = (int)Bitwise.oggpack_read(opb, 1);
                ci.mode_param[i].windowtype = (int)Bitwise.oggpack_read(opb, 16);
                ci.mode_param[i].transformtype = (int)Bitwise.oggpack_read(opb, 16);
                ci.mode_param[i].mapping = (int)Bitwise.oggpack_read(opb, 8);

                if (ci.mode_param[i].windowtype >= Registry.VI_WINDOWB) return (Codec.OV_EBADHEADER);
                if (ci.mode_param[i].transformtype >= Registry.VI_WINDOWB) return (Codec.OV_EBADHEADER);
                if (ci.mode_param[i].mapping >= ci.maps) return (Codec.OV_EBADHEADER);
                if (ci.mode_param[i].mapping < 0) return (Codec.OV_EBADHEADER);
            }

            if (Bitwise.oggpack_read(opb, 1) != 1) return (Codec.OV_EBADHEADER); /* top level EOP check */

            return (0);
        }

        /* The Vorbis header is in three packets; the initial small packet in
       the first page that identifies basic parameters, a second packet
       with bitstream comments and a third packet that holds the
       codebook. */

        public static int vorbis_synthesis_headerin(Registry r, Codec.vorbis_info vi, Codec.vorbis_comment vc, Vogg.ogg_packet op)
        {
            Vogg.oggpack_buffer opb = new Vogg.oggpack_buffer();

            if (op != null)
            {
                Bitwise.oggpack_readinit(opb, op.packet, (int)op.bytes);

                /* Which of the three types of header is this? */
                /* Also verify header-ness, vorbis */
                {
                    CPtr.BytePtr buffer = new CPtr.BytePtr(new byte[6]);
                    int packtype = (int)Bitwise.oggpack_read(opb, 8);
                    _v_readstring(opb, buffer, 6);
                    byte[] tmpvorbis = new byte[] { 0x76, 0x6f, 0x72, 0x62, 0x69, 0x73 };
                    if (CPtr.BytePtr.memcmp(buffer, new CPtr.BytePtr(tmpvorbis), 6) != 0)
                    {
                        /* not a vorbis header */
                        return (Codec.OV_ENOTVORBIS);
                    }
                    switch (packtype)
                    {
                        case 0x01: /* least significant *bit* is read first */
                            if (op.b_o_s == 0)
                            {
                                /* Not the initial packet */
                                return (Codec.OV_EBADHEADER);
                            }
                            if (vi.rate != 0)
                            {
                                /* previously initialized info header */
                                return (Codec.OV_EBADHEADER);
                            }

                            return (_vorbis_unpack_info(vi, opb));

                        case 0x03: /* least significant *bit* is read first */
                            if (vi.rate == 0)
                            {
                                /* um... we didn't get the initial header */
                                return (Codec.OV_EBADHEADER);
                            }

                            return (_vorbis_unpack_comment(vc, opb));

                        case 0x05: /* least significant *bit* is read first */
                            if (vi.rate == 0 || vc.vendor == null)
                            {
                                /* um... we didn;t get the initial header or comments yet */
                                return (Codec.OV_EBADHEADER);
                            }

                            return (_vorbis_unpack_books(r, vi, opb));

                        default:
                            /* Not a valid vorbis header type */
                            return (Codec.OV_EBADHEADER);
                    }
                }
            }
            return (Codec.OV_EBADHEADER);
        }
    }
}
