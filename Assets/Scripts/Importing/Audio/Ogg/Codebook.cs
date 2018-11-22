using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TanjentOGG is released under the 3-clause BSD license. Please read license.txt for the full license.
namespace TanjentOGG
{
    public class Codebook
    {
        /* This structure encapsulates huffman and VQ style encoding books; it
doesn't do anything specific to either.

valuelist/quantlist are nonNULL (and q_* significant) only if
there's entry->value mapping to be done.

If encode-side mapping must be done (and thus the entry needs to be
hunted), the auxiliary encode pointer will point to a decision
tree.  This is true of both VQ and huffman, but is mostly useful
with VQ.

*/

        public class static_codebook
        {
            public long dim;           /* codebook dimensions (elements per vector) */
            public long entries;       /* codebook entries */
            public byte[] lengthlist;    /* codeword lengths in bits */

            /* mapping ***************************************************************/
            public int maptype;       /* 0=none
                           1=implicitly populated values from map column
                           2=listed arbitrary values */

            /* The below does a linear, single monotonic sequence mapping. */
            public long q_min;       /* packed 32 bit float; quant value 0 maps to minval */
            public long q_delta;     /* packed 32 bit float; val 1 - val 0 == delta */
            public int q_quant;     /* bits: 0 < quant <= 16 */
            public int q_sequencep; /* bitflag */

            public long[] quantlist;  /* map == 1: (int)(entries^(1/dim)) element column map
                           map == 2: list of dim*entries quantized entry vals
                        */
            public int allocedp;
        }

        public  class codebook
        {
            public long dim;           /* codebook dimensions (elements per vector) */
            public long entries;       /* codebook entries */
            public long used_entries;  /* populated codebook entries */
            public static_codebook c;

            /* for encode, the below are entry-ordered, fully populated */
            /* for decode, the below are ordered by bitreversed codeword and only
               used entries are populated */
            public float[] valuelist;  /* list of dim*entries actual entry values */
            public long[] codelist;   /* list of bitstream codewords for each entry */

            public int[] dec_index;  /* only used if sparseness collapsed */
            public byte[] dec_codelengths;
            public long[] dec_firsttable;
            public int dec_firsttablen;
            public int dec_maxlength;

            /* The current encoder uses only centered, integer-only lattice books. */
            public int quantvals;
            public int minval;
            public int delta;

            public void clear()
            {
                dim = 0;
                entries = 0;
                used_entries = 0;
                c = null;

                valuelist = null;
                codelist = null;

                dec_index = null;
                dec_codelengths = null;
                dec_firsttable = null;
                dec_firsttablen = 0;
                dec_maxlength = 0;

                quantvals = 0;
                minval = 0;
                delta = 0;
            }
        }

        /* unpacks a codebook from the packet buffer into the codebook struct,
       readies the codebook auxiliary structures for decode *************/
        public static static_codebook vorbis_staticbook_unpack(Vogg.oggpack_buffer opb)
        {
            long i, j;
            static_codebook s = new static_codebook();
            s.allocedp = 1;

            /* make sure alignment is correct */
            if (Bitwise.oggpack_read(opb, 24) != 0x564342) return null;

            /* first the basic parameters */
            s.dim = Bitwise.oggpack_read(opb, 16);
            s.entries = Bitwise.oggpack_read(opb, 24);
            if (s.entries == -1) return null;

            if (Sharedbook._ilog((int)s.dim) + Sharedbook._ilog((int)s.entries) > 24) return null;

            /* codeword ordering.... length ordered or unordered? */
            switch ((int)Bitwise.oggpack_read(opb, 1))
            {
                case 0:
                    {
                        long unused;
                        /* allocated but unused entries? */
                        unused = Bitwise.oggpack_read(opb, 1);
                        if ((s.entries * (unused != 0 ? 1 : 5) + 7) >> 3 > opb.storage - Bitwise.oggpack_bytes(opb))
                            return null;
                        /* unordered */
                        s.lengthlist = new byte[(int)s.entries];

                        /* allocated but unused entries? */
                        if (unused != 0)
                        {
                            /* yes, unused entries */

                            for (i = 0; i < s.entries; i++)
                            {
                                if (Bitwise.oggpack_read(opb, 1) != 0)
                                {
                                    long num = Bitwise.oggpack_read(opb, 5);
                                    if (num == -1) return null;
                                    s.lengthlist[((int)i)] = (byte)(num + 1);
                                }
                                else
                                    s.lengthlist[((int)i)] = 0;
                            }
                        }
                        else
                        {
                            /* all entries used; no tagging */
                            for (i = 0; i < s.entries; i++)
                            {
                                long num = Bitwise.oggpack_read(opb, 5);
                                if (num == -1) return null;
                                s.lengthlist[((int)i)] = (byte)(num + 1);
                            }
                        }

                        break;
                    }
                case 1:
                    /* ordered */
                    {
                        long length = Bitwise.oggpack_read(opb, 5) + 1;
                        if (length == 0) return null;
                        s.lengthlist = new byte[(int)s.entries];

                        for (i = 0; i < s.entries; )
                        {
                            long num = Bitwise.oggpack_read(opb, Sharedbook._ilog((int)(s.entries - i)));
                            if (num == -1) return null;
                            if (length > 32 || num > s.entries - i ||
                                    (num > 0 && (num - 1) >> (int)(length - 1) > 1))
                            {
                                return null;
                            }
                            if (length > 32) return null;
                            for (j = 0; j < num; j++, i++)
                                s.lengthlist[((int)i)] = (byte)length;
                            length++;
                        }
                    }
                    break;
                default:
                    /* EOF */
                    return null;
            }

            /* Do we have a mapping to unpack? */
            switch ((s.maptype = (int)Bitwise.oggpack_read(opb, 4)))
            {
                case 0:
                    /* no mapping */
                    break;
                case 1:
                case 2:
                    /* implicitly populated value mapping */
                    /* explicitly populated value mapping */

                    s.q_min = Bitwise.oggpack_read(opb, 32);
                    s.q_delta = Bitwise.oggpack_read(opb, 32);
                    s.q_quant = (int)(Bitwise.oggpack_read(opb, 4) + 1);
                    s.q_sequencep = (int)Bitwise.oggpack_read(opb, 1);
                    if (s.q_sequencep == -1) return null;
                    {
                        int quantvals = 0;
                        switch (s.maptype)
                        {
                            case 1:
                                quantvals = s.dim == 0 ? 0 : (int)Sharedbook._book_maptype1_quantvals(s);
                                break;
                            case 2:
                                quantvals = (int)(s.entries * s.dim);
                                break;
                        }

                        /* quantized values */
                        if (((quantvals * s.q_quant + 7) >> 3) > opb.storage - Bitwise.oggpack_bytes(opb))
                            return null;
                        s.quantlist = new long[quantvals];
                        for (i = 0; i < quantvals; i++)
                            s.quantlist[((int)i)] = Bitwise.oggpack_read(opb, s.q_quant);

                        if (quantvals != 0 && s.quantlist[quantvals - 1] == -1) return null;
                    }
                    break;
                default:
                    return null;
            }

            /* all set */
            return (s);
        }

        /* the 'eliminate the decode tree' optimization actually requires the
       codewords to be MSb first, not LSb.  This is an annoying inelegancy
       (and one of the first places where carefully thought out design
       turned out to be wrong; Vorbis II and future Ogg codecs should go
       to an MSb bitpacker), but not actually the huge hit it appears to
       be.  The first-stage decode table catches most words so that
       bitreverse is not in the main execution path. */

        static long bitreverse(long x)
        {
            x = ((x >> 16) & 0x0000ffffL) | ((x << 16) & 0xffff0000L);
            x = ((x >> 8) & 0x00ff00ffL) | ((x << 8) & 0xff00ff00L);
            x = ((x >> 4) & 0x0f0f0f0fL) | ((x << 4) & 0xf0f0f0f0L);
            x = ((x >> 2) & 0x33333333L) | ((x << 2) & 0xccccccccL);
            return ((x >> 1) & 0x55555555L) | ((x << 1) & 0xaaaaaaaaL);
        }

        static long decode_packed_entry_number(codebook book, Vogg.oggpack_buffer b)
        {
            int read = book.dec_maxlength;
            long lo, hi;
            long lok = Bitwise.oggpack_look(b, book.dec_firsttablen);

            if (lok >= 0)
            {
                long entry = (int)book.dec_firsttable[((int)lok)]; // signed 32 bit int in C
                if ((entry & 0x80000000L) != 0)
                {
                    lo = (entry >> 15) & 0x7fff;
                    hi = book.used_entries - (entry & 0x7fff);
                }
                else
                {
                    Bitwise.oggpack_adv(b, book.dec_codelengths[((int)(entry - 1))]);
                    return (entry - 1);
                }
            }
            else
            {
                lo = 0;
                hi = book.used_entries;
            }

            lok = Bitwise.oggpack_look(b, read);

            while (lok < 0 && read > 1)
                lok = Bitwise.oggpack_look(b, --read);
            if (lok < 0) return -1;

            /* bisect search for the codeword in the ordered list */
            {
                long testword = bitreverse(lok);

                while (hi - lo > 1)
                {
                    long p = (hi - lo) >> 1;
                    long test = (book.codelist[((int)(lo + p))] > testword ? 1 : 0);
                    lo += p & (test - 1);
                    hi -= p & (-test);
                }

                if (book.dec_codelengths[((int)lo)] <= read)
                {
                    Bitwise.oggpack_adv(b, book.dec_codelengths[((int)lo)]);
                    return (lo);
                }
            }

            Bitwise.oggpack_adv(b, read);

            return (-1);
        }

        /* Decode side is specced and easier, because we don't need to find
       matches using different criteria; we simply read and map.  There are
       two things we need to do 'depending':

       We may need to support interleave.  We don't really, but it's
       convenient to do it here rather than rebuild the vector later.

       Cascades may be additive or multiplicitive; this is not inherent in
       the codebook, but set in the code using the codebook.  Like
       interleaving, it's easiest to do it here.
       addmul==0 -> declarative (set the value)
       addmul==1 -> additive
       addmul==2 -> multiplicitive */

        /* returns the [original, not compacted] entry number or -1 on eof *********/
        public static long vorbis_book_decode(codebook book, Vogg.oggpack_buffer b)
        {
            if (book.used_entries > 0)
            {
                long packed_entry = decode_packed_entry_number(book, b);
                if (packed_entry >= 0)
                    return (book.dec_index[((int)packed_entry)]);
            }

            /* if there's no dec_index, the codebook unpacking isn't collapsed */
            return (-1);
        }

        /* returns 0 on OK or -1 on eof *************************************/
        /* decode vector / dim granularity gaurding is done in the upper layer */
        public static long vorbis_book_decodevs_add(codebook book, CPtr.FloatPtr a, Vogg.oggpack_buffer b, int n)
        {
            if (book.used_entries > 0)
            {
                int step = (int)(n / book.dim);
                long[] entry = new long[step];
                CPtr.FloatPtr[] t = new CPtr.FloatPtr[step];
                int i, j, o;

                for (i = 0; i < step; i++)
                {
                    entry[i] = decode_packed_entry_number(book, b);
                    if (entry[i] == -1) return (-1);
                    t[i] = new CPtr.FloatPtr(book.valuelist, (int)(entry[i] * book.dim));
                }
                for (i = 0, o = 0; i < book.dim; i++, o += step)
                    for (j = 0; j < step; j++)
                        a.floats[a.offset + o + j] += t[j].floats[t[j].offset + i];
            }
            return (0);
        }

        /* decode vector / dim granularity gaurding is done in the upper layer */
        public static long vorbis_book_decodev_add(codebook book, CPtr.FloatPtr a, Vogg.oggpack_buffer b, int n)
        {
            if (book.used_entries > 0)
            {
                int i, j, entry;
                CPtr.FloatPtr t;

                if (book.dim > 8)
                {
                    for (i = 0; i < n; )
                    {
                        entry = (int)decode_packed_entry_number(book, b);
                        if (entry == -1) return (-1);
                        t = new CPtr.FloatPtr(book.valuelist, (int)(entry * book.dim));
                        for (j = 0; j < book.dim; )
                            a.floats[a.offset + i++] += t.floats[t.offset + j++];
                    }
                }
                else
                {
                    for (i = 0; i < n; )
                    {
                        entry = (int)decode_packed_entry_number(book, b);
                        if (entry == -1) return (-1);
                        t = new CPtr.FloatPtr(book.valuelist, (int)(entry * book.dim));
                        for (j = 0; j < book.dim; )
                        {
                            a.floats[a.offset + i++] += t.floats[t.offset + j++];
                        }
                    }
                }
            }
            return (0);
        }

        /* unlike the others, we guard against n not being an integer number
       of <dim> internally rather than in the upper layer (called only by
       floor0) */
        public static long vorbis_book_decodev_set(codebook book, float[] a, Vogg.oggpack_buffer b, int n)
        {
            if (book.used_entries > 0)
            {
                int i, j, entry;

                for (i = 0; i < n; )
                {
                    entry = (int)decode_packed_entry_number(book, b);
                    if (entry == -1) return (-1);
                    for (j = 0; i < n && j < book.dim; )
                    {
                        a[i++] = book.valuelist[((int)((entry * book.dim) + j++))];
                    }
                }
            }
            else
            {
                int i;

                for (i = 0; i < n; )
                {
                    a[i++] = 0f;
                }
            }
            return (0);
        }

        public static long vorbis_book_decodevv_add(codebook book, CPtr.FloatPtr[] a, long offset, int ch, Vogg.oggpack_buffer b, int n)
        {

            long i, j, entry;
            int chptr = 0;
            if (book.used_entries > 0)
            {
                for (i = offset / ch; i < (offset + n) / ch; )
                {
                    entry = decode_packed_entry_number(book, b);
                    if (entry == -1) return (-1);
                    {
                        CPtr.FloatPtr t = new CPtr.FloatPtr(book.valuelist, (int)(entry * book.dim));
                        for (j = 0; j < book.dim; j++)
                        {
                            a[chptr].floats[((int)(a[chptr].offset + i))] += t.floats[((int)(t.offset + j))];
                            chptr++;
                            if (chptr == ch)
                            {
                                chptr = 0;
                                i++;
                            }
                        }
                    }
                }
            }
            return (0);
        }
    }
}
