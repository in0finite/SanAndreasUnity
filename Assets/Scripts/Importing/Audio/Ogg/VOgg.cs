using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TanjentOGG is released under the 3-clause BSD license. Please read license.txt for the full license.
namespace TanjentOGG
{
    public class Vogg
    {
        public class oggpack_buffer
        {
            public long endbyte;
            public int endbit;

            public CPtr.BytePtr buffer;
            public CPtr.BytePtr ptr;
            public long storage;

            public void clear()
            {
                endbyte = 0;
                endbit = 0;
                buffer = null;
                ptr = null;
                storage = 0;
            }
        }


        /* ogg_page is used to encapsulate the data in one Ogg bitstream page *****/

        public class ogg_page
        {
            public CPtr.BytePtr header;
            public long header_len;
            public CPtr.BytePtr body;
            public long body_len;
        }

        public class ogg_stream_state
        {
            public CPtr.BytePtr body_data;    /* bytes from packet bodies */
            public long body_storage;          /* storage elements allocated */
            public long body_fill;             /* elements stored; fill mark */
            public long body_returned;         /* elements of fill returned */


            public int[] lacing_vals;      /* The values that will go to the segment table */
            public long[] granule_vals; /* granulepos values for headers. Not compact
                                this way, but it is simple coupled to the
                                lacing fifo */
            public long lacing_storage;
            public long lacing_fill;
            public long lacing_packet;
            public long lacing_returned;

            public byte[] header = new byte[282];      /* working space for header encode */
            public int header_fill;

            public int e_o_s;          /* set when we have buffered the last packet in the
                             logical bitstream */
            public int b_o_s;          /* set after we've written the initial page
                             of a logical bitstream */
            public long serialno;
            public long pageno;
            public long packetno;  /* sequence number for decode; the framing
                             knows where there's a hole in the data,
                             but we need coupling so that the codec
                             (which is in a separate abstraction
                             layer) also knows about the gap */
            public long granulepos;

            public void clear()
            {
                body_data = null;
                body_storage = 0;
                body_fill = 0;
                body_returned = 0;
                lacing_vals = null;
                granule_vals = null;
                lacing_storage = 0;
                lacing_fill = 0;
                lacing_packet = 0;
                lacing_returned = 0;
                for (int i = 0; i < header.Length; i++)
                {
                    header[i] = 0;
                }
                header_fill = 0;
                e_o_s = 0;
                b_o_s = 0;
                serialno = 0;
                pageno = 0;
                packetno = 0;
                granulepos = 0;
            }
        }

        /* ogg_packet is used to encapsulate the data and metadata belonging
       to a single raw Ogg/Vorbis packet *************************************/

        public class ogg_packet
        {
            public CPtr.BytePtr packet;
            public long bytes;
            public long b_o_s;
            public long e_o_s;

            public long granulepos;

            public long packetno;     /* sequence number for decode; the framing
                                knows where there's a hole in the data,
                                but we need coupling so that the codec
                                (which is in a separate abstraction
                                layer) also knows about the gap */
        }

        public class ogg_sync_state
        {
            public CPtr.BytePtr data;
            public int storage;
            public int fill;
            public int returned;

            public int unsynced;
            public int headerbytes;
            public int bodybytes;

            public void clear()
            {
                data = null;
                storage = 0;
                fill = 0;
                returned = 0;
                unsynced = 0;
                headerbytes = 0;
                bodybytes = 0;
            }
        }
    }
}