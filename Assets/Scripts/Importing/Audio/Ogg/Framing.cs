using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TanjentOGG is released under the 3-clause BSD license. Please read license.txt for the full license.
namespace TanjentOGG
{
    public class Framing
    {
        public static int ogg_page_version(Vogg.ogg_page og)
        {
            return ((int)(og.header.bytes[og.header.offset + 4] & 0xFF));
        }

        public static int ogg_page_continued(Vogg.ogg_page og)
        {
            return ((int)(og.header.bytes[og.header.offset + 5] & 0x01));
        }

        public static int ogg_page_bos(Vogg.ogg_page og)
        {
            return ((int)(og.header.bytes[og.header.offset + 5] & 0x02));
        }

        public static int ogg_page_eos(Vogg.ogg_page og)
        {
            return ((int)(og.header.bytes[og.header.offset + 5] & 0x04));
        }

        public static long ogg_page_granulepos(Vogg.ogg_page og)
        {
            CPtr.BytePtr page = og.header;
            long granulepos = page.bytes[page.offset + 13] & (0xff);
            granulepos = (granulepos << 8) | (page.bytes[page.offset + 12] & 0xff);
            granulepos = (granulepos << 8) | (page.bytes[page.offset + 11] & 0xff);
            granulepos = (granulepos << 8) | (page.bytes[page.offset + 10] & 0xff);
            granulepos = (granulepos << 8) | (page.bytes[page.offset + 9] & 0xff);
            granulepos = (granulepos << 8) | (page.bytes[page.offset + 8] & 0xff);
            granulepos = (granulepos << 8) | (page.bytes[page.offset + 7] & 0xff);
            granulepos = (granulepos << 8) | (page.bytes[page.offset + 6] & 0xff);
            return (granulepos);
        }

        public static int ogg_page_serialno(Vogg.ogg_page og)
        {
            return ((og.header.bytes[og.header.offset + 14] & 0xFF) |
                    ((og.header.bytes[og.header.offset + 15] & 0xFF) << 8) |
                    ((og.header.bytes[og.header.offset + 16] & 0xFF) << 16) |
                    ((og.header.bytes[og.header.offset + 17] & 0xFF) << 24));
        }

        public static int ogg_page_pageno(Vogg.ogg_page og)
        {
            return ((og.header.bytes[og.header.offset + 18] & 0xFF) |
                    ((og.header.bytes[og.header.offset + 19] & 0xFF) << 8) |
                    ((og.header.bytes[og.header.offset + 20] & 0xFF) << 16) |
                    ((og.header.bytes[og.header.offset + 21] & 0xFF) << 24));
        }

        /* init the encode/decode logical stream state */

        public static int ogg_stream_init(Vogg.ogg_stream_state os, int serialno)
        {
            if (os != null)
            {
                os.clear();
                os.body_storage = 16 * 1024;
                os.lacing_storage = 1024;

                os.body_data = CPtr.BytePtr.malloc(os.body_storage);
                os.lacing_vals = new int[(int)os.lacing_storage];
                os.granule_vals = new long[(int)os.lacing_storage];

                os.serialno = serialno;

                return (0);
            }
            return (-1);
        }

        /* async/delayed error detection for the ogg_stream_state */
        public static int ogg_stream_check(Vogg.ogg_stream_state os)
        {
            if (os == null || os.body_data == null) return -1;
            return 0;
        }

        /* Helpers for ogg_stream_encode; this keeps the structure and
       what's happening fairly clear */

        public static int _os_body_expand(Vogg.ogg_stream_state os, long needed)
        {
            if (os.body_storage - needed <= os.body_fill)
            {
                long body_storage;
                CPtr.BytePtr ret;
                if (os.body_storage > LONG_MAX - needed)
                {
                    return -1;
                }
                body_storage = os.body_storage + needed;
                if (body_storage < LONG_MAX - 1024) body_storage += 1024;
                ret = CPtr.BytePtr.realloc(os.body_data, (int)body_storage);
                os.body_storage = body_storage;
                os.body_data = ret;
            }
            return 0;
        }

        private static long LONG_MAX = 2147483647L;

        public static int _os_lacing_expand(Vogg.ogg_stream_state os, long needed)
        {
            if (os.lacing_storage - needed <= os.lacing_fill)
            {
                long lacing_storage;
                int[] ret;
                if (os.lacing_storage > LONG_MAX - needed)
                {
                    return -1;
                }
                lacing_storage = os.lacing_storage + needed;
                if (lacing_storage < LONG_MAX - 32) lacing_storage += 32;

                ret = new int[(int)lacing_storage];
                Array.Copy(os.lacing_vals, 0, ret, 0, os.lacing_vals.Length);
                os.lacing_vals = ret;

                long[] retl = new long[(int)lacing_storage];
                Array.Copy(os.granule_vals, 0, retl, 0, os.granule_vals.Length);
                os.granule_vals = retl;
                os.lacing_storage = lacing_storage;
            }
            return 0;
        }

        /* DECODING PRIMITIVES: packet streaming layer **********************/

        /* This has two layers to place more of the multi-serialno and paging
           control in the application's hands.  First, we expose a data buffer
           using ogg_sync_buffer().  The app either copies into the
           buffer, or passes it directly to read(), etc.  We then call
           ogg_sync_wrote() to tell how many bytes we just added.

           Pages are returned (pointers into the buffer in ogg_sync_state)
           by ogg_sync_pageout().  The page is then submitted to
           ogg_stream_pagein() along with the appropriate
           ogg_stream_state* (ie, matching serialno).  We then get raw
           packets out calling ogg_stream_packetout() with a
           ogg_stream_state. */

        /* initialize the struct to a known state */
        public static int ogg_sync_init(Vogg.ogg_sync_state oy)
        {
            if (oy != null)
            {
                oy.storage = -1; /* used as a readiness flag */
                oy.clear();
            }
            return (0);
        }

        private static int ogg_sync_check(Vogg.ogg_sync_state oy)
        {
            if (oy.storage < 0) return -1;
            return 0;
        }

        public static CPtr.BytePtr ogg_sync_buffer(Vogg.ogg_sync_state oy, long size)
        {
            if (ogg_sync_check(oy) != 0) return null;

            /* first, clear out any space that has been previously returned */
            if (oy.returned != 0)
            {
                oy.fill -= oy.returned;
                if (oy.fill > 0)
                    CPtr.BytePtr.memmove(oy.data, new CPtr.BytePtr(oy.data, oy.returned), oy.fill);
                oy.returned = 0;
            }

            if (size > oy.storage - oy.fill)
            {
                /* We need to extend the internal buffer */
                long newsize = size + oy.fill + 4096; /* an extra page to be nice */
                CPtr.BytePtr ret;

                if (oy.data != null)
                    ret = CPtr.BytePtr.realloc(oy.data, (int)newsize);
                else
                    ret = CPtr.BytePtr.malloc(newsize);
                oy.data = ret;
                oy.storage = (int)newsize;
            }

            /* expose a segment at least as large as requested at the fill mark */
            return new CPtr.BytePtr(oy.data, oy.fill);
        }

        public static int ogg_sync_wrote(Vogg.ogg_sync_state oy, long bytes)
        {
            if (ogg_sync_check(oy) != 0) return -1;
            if (oy.fill + bytes > oy.storage) return -1;
            oy.fill += (int)bytes;
            return (0);
        }

        /* sync the stream.  This is meant to be useful for finding page
       boundaries.

       return values for this:
      -n) skipped n bytes
       0) page not ready; more data (no bytes skipped)
       n) page synced at current location; page length n bytes

    */

        private static long ogg_sync_pageseek(Vogg.ogg_sync_state oy, Vogg.ogg_page og)
        {
            CPtr.BytePtr page = new CPtr.BytePtr(oy.data, oy.returned);
            CPtr.BytePtr next;
            long bytes = oy.fill - oy.returned;

            if (ogg_sync_check(oy) != 0) return 0;

            if (oy.headerbytes == 0)
            {
                int headerbytes, i;
                if (bytes < 27) return (0); /* not enough for a header */

                /* verify capture pattern */
                byte[] oggHeader = new byte[] { 0x4f, 0x67, 0x67, 0x53 };
                if (CPtr.BytePtr.memcmp(page, new CPtr.BytePtr(oggHeader), 4) != 0)
                {
                    oy.headerbytes = 0;
                    oy.bodybytes = 0;
                    /* search for possible capture */
                    next = CPtr.BytePtr.memchr(new CPtr.BytePtr(page, 1), 'O', (int)(bytes - 1));
                    if (next == null)
                        next = new CPtr.BytePtr(oy.data, oy.fill);

                    oy.returned = (next.offset - oy.data.offset);
                    return ((long)-(next.offset - page.offset));
                }

                headerbytes = (page.bytes[page.offset + 26] & 0xFF) + 27;
                if (bytes < headerbytes) return (0); /* not enough for header + seg table */

                /* count up body length in the segment table */

                for (i = 0; i < (page.bytes[page.offset + 26] & 0xFF); i++)
                    oy.bodybytes += page.bytes[page.offset + 27 + i] & 0xFF;
                oy.headerbytes = headerbytes;
            }

            if (oy.bodybytes + oy.headerbytes > bytes) return (0);

            /* yes, have a whole page all ready to go */
            page = new CPtr.BytePtr(oy.data, oy.returned);

            if (og != null)
            {
                og.header = page;
                og.header_len = oy.headerbytes;
                og.body = new CPtr.BytePtr(page, oy.headerbytes);
                og.body_len = oy.bodybytes;
            }

            oy.unsynced = 0;
            oy.returned += (int)(bytes = oy.headerbytes + oy.bodybytes);
            oy.headerbytes = 0;
            oy.bodybytes = 0;
            return (bytes);
        }

        /* sync the stream and get a page.  Keep trying until we find a page.
       Suppress 'sync errors' after reporting the first.

       return values:
       -1) recapture (hole in data)
        0) need more data
        1) page returned

       Returns pointers into buffered data; invalidated by next call to
       _stream, _clear, _init, or _buffer */

        public static int ogg_sync_pageout(Vogg.ogg_sync_state oy, Vogg.ogg_page og)
        {

            if (ogg_sync_check(oy) != 0) return 0;

            /* all we need to do is verify a page at the head of the stream
               buffer.  If it doesn't verify, we look for the next potential
               frame */

            for (; ; )
            {
                long ret = ogg_sync_pageseek(oy, og);
                if (ret > 0)
                {
                    /* have a page */
                    return (1);
                }
                if (ret == 0)
                {
                    /* need more data */
                    return (0);
                }

                /* head did not start a synced page... skipped some bytes */
                if (oy.unsynced == 0)
                {
                    oy.unsynced = 1;
                    return (-1);
                }

                /* loop. keep looking */
            }
        }

        /* add the incoming page to the stream state; we decompose the page
       into packet segments here as well. */

        public static int ogg_stream_pagein(Vogg.ogg_stream_state os, Vogg.ogg_page og)
        {
            CPtr.BytePtr header = og.header;
            CPtr.BytePtr body = og.body;
            long bodysize = og.body_len;
            int segptr = 0;

            int version = ogg_page_version(og);
            int continued = ogg_page_continued(og);
            int bos = ogg_page_bos(og);
            int eos = ogg_page_eos(og);
            long granulepos = ogg_page_granulepos(og);
            int serialno = ogg_page_serialno(og);
            long pageno = ogg_page_pageno(og);
            int segments = header.bytes[header.offset + 26] & 0xFF;

            if (ogg_stream_check(os) != 0) return -1;

            /* clean up 'returned data' */
            {
                long lr = os.lacing_returned;
                long br = os.body_returned;

                /* body data */
                if (br != 0)
                {
                    os.body_fill -= br;
                    if (os.body_fill != 0)
                        CPtr.BytePtr.memmove(os.body_data, new CPtr.BytePtr(os.body_data, (int)br), (int)os.body_fill);
                    os.body_returned = 0;
                }

                if (lr != 0)
                {
                    /* segment table */
                    if ((os.lacing_fill - lr) != 0)
                    {
                        int[] newoslacingvals = new int[os.lacing_vals.Length];
                        long[] newosgranulevals = new long[os.granule_vals.Length];
                        for (int i = 0; i < (int)(os.lacing_fill - lr); i++)
                        {
                            newoslacingvals[i] = os.lacing_vals[((int)(i + lr))];
                            newosgranulevals[i] = os.granule_vals[((int)(i + lr))];
                        }
                        os.lacing_vals = newoslacingvals;
                        os.granule_vals = newosgranulevals;
                    }
                    os.lacing_fill -= lr;
                    os.lacing_packet -= lr;
                    os.lacing_returned = 0;
                }
            }

            /* check the serial number */
            if (serialno != os.serialno) return (-1);
            if (version > 0) return (-1);

            if (_os_lacing_expand(os, segments + 1) != 0) return -1;

            /* are we in sequence? */
            if (pageno != os.pageno)
            {
                int i;

                /* unroll previous partial packet (if any) */
                for (i = (int)os.lacing_packet; i < os.lacing_fill; i++)
                    os.body_fill -= os.lacing_vals[i] & 0xff;
                os.lacing_fill = os.lacing_packet;

                /* make a note of dropped data in segment table */
                if (os.pageno != -1)
                {
                    os.lacing_vals[((int)os.lacing_fill++)] = 0x400;
                    os.lacing_packet++;
                }
            }

            /* are we a 'continued packet' page?  If so, we may need to skip
               some segments */
            if (continued != 0)
            {
                if (os.lacing_fill < 1 ||
                        os.lacing_vals[((int)(os.lacing_fill - 1))] == 0x400)
                {
                    bos = 0;
                    for (; segptr < segments; segptr++)
                    {
                        int val = header.bytes[header.offset + 27 + segptr] & 0xFF;
                        body.offset += val;
                        bodysize -= val;
                        if (val < 255)
                        {
                            segptr++;
                            break;
                        }
                    }
                }
            }

            if (bodysize != 0)
            {
                if (_os_body_expand(os, bodysize) != 0) return -1;
                CPtr.BytePtr.memcpy(new CPtr.BytePtr(os.body_data, (int)os.body_fill), body, (int)bodysize);
                os.body_fill += bodysize;
            }

            {
                int saved = -1;
                while (segptr < segments)
                {
                    int val = header.bytes[header.offset + 27 + segptr] & 0xFF;
                    os.lacing_vals[((int)os.lacing_fill)] = val;
                    os.granule_vals[((int)os.lacing_fill)] = -1;

                    if (bos != 0)
                    {
                        os.lacing_vals[((int)os.lacing_fill)] |= 0x100;
                        bos = 0;
                    }

                    if (val < 255) saved = (int)os.lacing_fill;

                    os.lacing_fill++;
                    segptr++;

                    if (val < 255) os.lacing_packet = os.lacing_fill;
                }

                /* set the granulepos on the last granuleval of the last full packet */
                if (saved != -1)
                {
                    os.granule_vals[saved] = granulepos;
                }

            }

            if (eos != 0)
            {
                os.e_o_s = 1;
                if (os.lacing_fill > 0)
                    os.lacing_vals[((int)(os.lacing_fill - 1))] |= 0x200;
            }

            os.pageno = pageno + 1;

            return (0);
        }

        static int _packetout(Vogg.ogg_stream_state os, Vogg.ogg_packet op, int adv)
        {

            /* The last part of decode. We have the stream broken into packet
               segments.  Now we need to group them into packets (or return the
               out of sync markers) */

            int ptr = (int)os.lacing_returned;

            if (os.lacing_packet <= ptr) return (0);

            if ((os.lacing_vals[ptr] & 0x400) != 0)
            {
                /* we need to tell the codec there's a gap; it might need to
                   handle previous packet dependencies. */
                os.lacing_returned++;
                os.packetno++;
                return (-1);
            }

            if (op == null && adv == 0) return (1); /* just using peek as an inexpensive way
                               to ask if there's a whole packet
                               waiting */

            /* Gather the whole packet. We'll have no holes or a partial packet */
            {
                int size = os.lacing_vals[ptr] & 0xff;
                long bytes = size;
                int eos = os.lacing_vals[ptr] & 0x200; /* last packet of the stream? */
                int bos = os.lacing_vals[ptr] & 0x100; /* first packet of the stream? */

                while (size == 255)
                {
                    int val = os.lacing_vals[++ptr];
                    size = val & 0xff;
                    if ((val & 0x200) != 0) eos = 0x200;
                    bytes += size;
                }

                if (op != null)
                {
                    op.e_o_s = eos;
                    op.b_o_s = bos;
                    op.packet = new CPtr.BytePtr(os.body_data, (int)os.body_returned);
                    op.packetno = os.packetno;
                    op.granulepos = os.granule_vals[ptr];
                    op.bytes = bytes;
                }

                if (adv != 0)
                {
                    os.body_returned += bytes;
                    os.lacing_returned = ptr + 1;
                    os.packetno++;
                }
            }
            return (1);
        }

        public static int ogg_stream_packetout(Vogg.ogg_stream_state os, Vogg.ogg_packet op)
        {
            if (ogg_stream_check(os) != 0) return 0;
            return _packetout(os, op, 1);
        }
    }
}
