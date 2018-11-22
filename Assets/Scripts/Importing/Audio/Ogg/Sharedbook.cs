using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TanjentOGG is released under the 3-clause BSD license. Please read license.txt for the full license.
namespace TanjentOGG
{
    public class Sharedbook
    {
            /**
     * * pack/unpack helpers *****************************************
     */
    public static int _ilog(int v) {
        int ret = 0;
        while (v != 0) {
            ret++;
            v >>= 1;
        }
        return (ret);
    }

    /* 32 bit float (not IEEE; nonnormalized mantissa +
   biased exponent) : neeeeeee eeemmmmm mmmmmmmm mmmmmmmm
   Why not IEEE?  It's just not that important here. */

    static  int VQ_FEXP = 10;
    static  int VQ_FMAN = 21;
    static  int VQ_FEXP_BIAS = 768;

    static double ldexp(double x, int exp) {
        return (x * Math.Pow(2, exp));
    }

    static float _float32_unpack(long val) {
        double mant = val & 0x1fffff;
        int sign = (int) (val & 0x80000000);
        long exp = (val & 0x7fe00000L) >> VQ_FMAN;
        if (sign != 0) mant = -mant;
        return (float) ldexp(mant, (int) (exp - (VQ_FMAN - 1) - VQ_FEXP_BIAS));
    }

    /* given a list of word lengths, generate a list of codewords.  Works
   for length ordered or unordered, always assigns the lowest valued
   codewords first.  Extended to handle unused entries (length 0) */
    public static long[] _make_words(byte[] l, long n, long sparsecount) {
        long i, j, count = 0;
        long[] marker = new long[33];
        int cn = (int) (sparsecount != 0 ? sparsecount : n);
        long[] r = new long[cn];

        for (i = 0; i < n; i++) {
            long length = l[((int) i)];
            if (length > 0) {
                long entry = marker[((int) length)];

      /* when we claim a node for an entry, we also claim the nodes
         below it (pruning off the imagined tree that may have dangled
         from it) as well as blocking the use of any nodes directly
         above for leaves */

      /* update ourself */
                if (length < 32 && (entry >> (int)length) != 0)
                {
        /* error condition; the lengths must specify an overpopulated tree */
                    return null;
                }
                r[((int) count++)] = entry;

      /* Look to see if the next shorter marker points to the node
         above. if so, update it and repeat.  */
                {
                    for (j = length; j > 0; j--) {

                        if ((marker[((int) j)] & 1) != 0) {
            /* have to jump branches */
                            if (j == 1)
                                marker[1]++;
                            else
                                marker[((int) j)] = marker[((int) (j - 1))] << 1;
                            break; /* invariant says next upper marker would already
                      have been moved if it was on the same path */
                        }
                        marker[((int) j)]++;
                    }
                }

      /* prune the tree; the implicit invariant says all the longer
         markers were dangling from our just-taken node.  Dangle them
         from our *new* node. */
                for (j = length + 1; j < 33; j++)
                    if ((marker[((int) j)] >> 1) == entry) {
                        entry = marker[((int) j)];
                        marker[((int) j)] = marker[((int) (j - 1))] << 1;
                    } else
                        break;
            } else if (sparsecount == 0) count++;
        }

  /* sanity check the huffman tree; an underpopulated tree must be
     rejected. The only exception is the one-node pseudo-nil tree,
     which appears to be underpopulated because the tree doesn't
     really exist; there's only one possible 'codeword' or zero bits,
     but the above tree-gen code doesn't mark that. */
        if (sparsecount != 1) {
            for (i = 1; i < 33; i++)
                if ((marker[((int)i)] & (0xffffffffL >> (32 - (int)i))) != 0)
                {
                    return null;
                }
        }

  /* bitreverse the words because our bitwise packer/unpacker is LSb
     endian */
        for (i = 0, count = 0; i < n; i++) {
            int temp = 0;
            for (j = 0; j < l[((int) i)]; j++) {
                temp <<= 1;
                temp |= (int)(r[((int)count)] >> (int)j) & 1;
            }

            if (sparsecount != 0) {
                if (l[((int) i)] != 0)
                    r[((int) count++)] = temp;
            } else
                r[((int) count++)] = temp;
        }

        return (r);
    }

    /* there might be a straightforward one-line way to do the below
   that's portable and totally safe against roundoff, but I haven't
   thought of it.  Therefore, we opt on the side of caution */
    public static long _book_maptype1_quantvals(Codebook.static_codebook b) {
        long vals = (long) Math.Floor(Math.Pow((float) b.entries, 1f / b.dim));

  /* the above *should* be reliable, but we'll not assume that FP is
     ever reliable when bitstream sync is at stake; verify via integer
     means that vals really is the greatest value of dim for which
     vals^b->bim <= b->entries */
  /* treat the above as an initial guess */
        while (true) {
            long acc = 1;
            long acc1 = 1;
            int i;
            for (i = 0; i < b.dim; i++) {
                acc *= vals;
                acc1 *= vals + 1;
            }
            if (acc <= b.entries && acc1 > b.entries) {
                return (vals);
            } else {
                if (acc > b.entries) {
                    vals--;
                } else {
                    vals++;
                }
            }
        }
    }

    /* unpack the quantized list of values for encode/decode ***********/
/* we need to deal with two map types: in map type 1, the values are
   generated algorithmically (each column of the vector counts through
   the values in the quant vector). in map type 2, all the values came
   in in an explicit list.  Both value lists must be unpacked */
    public static float[] _book_unquantize(Codebook.static_codebook b, int n, int[] sparsemap) {
        long j, k, count = 0;
        if (b.maptype == 1 || b.maptype == 2) {
            int quantvals;
            float mindel = _float32_unpack(b.q_min);
            float delta = _float32_unpack(b.q_delta);
            float[] r = new float[(int) (n * b.dim)];

    /* maptype 1 and 2 both use a quantized value vector, but
       different sizes */
            switch (b.maptype) {
                case 1:
      /* most of the time, entries%dimensions == 0, but we need to be
         well defined.  We define that the possible vales at each
         scalar is values == entries/dim.  If entries%dim != 0, we'll
         have 'too few' values (values*dim<entries), which means that
         we'll have 'left over' entries; left over entries use zeroed
         values (and are wasted).  So don't generate codebooks like
         that */
                    quantvals = (int) _book_maptype1_quantvals(b);
                    for (j = 0; j < b.entries; j++) {
                        if (((sparsemap != null) && (b.lengthlist[((int) j)] != 0)) || (sparsemap == null)) {
                            float last = 0f;
                            int indexdiv = 1;
                            for (k = 0; k < b.dim; k++) {
                                int index = (int) ((j / indexdiv) % quantvals);
                                float val = b.quantlist[index];
                                val = Math.Abs(val) * delta + mindel + last;
                                if (b.q_sequencep != 0) last = val;
                                if (sparsemap != null)
                                    r[((int) (sparsemap[((int) count)] * b.dim + k))] = val;
                                else
                                    r[((int) (count * b.dim + k))] = val;
                                indexdiv *= quantvals;
                            }
                            count++;
                        }

                    }
                    break;
                case 2:
                    for (j = 0; j < b.entries; j++) {
                        if (((sparsemap != null) && (b.lengthlist[((int) j)] != 0)) || (sparsemap == null)) {
                            float last = 0f;

                            for (k = 0; k < b.dim; k++) {
                                float val = b.quantlist[((int) (j * b.dim + k))];
                                val = Math.Abs(val) * delta + mindel + last;
                                if (b.q_sequencep != 0) last = val;
                                if (sparsemap != null)
                                    r[((int) (sparsemap[((int) count)] * b.dim + k))] = val;
                                else
                                    r[((int) (count * b.dim + k))] = val;
                            }
                            count++;
                        }
                    }
                    break;
            }

            return (r);
        }
        return null;
    }

    static long bitreverse(long x) {
        x = ((x >> 16) & 0x0000ffffL) | ((x << 16) & 0xffff0000L);
        x = ((x >> 8) & 0x00ff00ffL) | ((x << 8) & 0xff00ff00L);
        x = ((x >> 4) & 0x0f0f0f0fL) | ((x << 4) & 0xf0f0f0f0L);
        x = ((x >> 2) & 0x33333333L) | ((x << 2) & 0xccccccccL);
        return ((x >> 1) & 0x55555555L) | ((x << 1) & 0xaaaaaaaaL);
    }

    class codepComparator : IComparer<int> {
        public long[] codes;

        public int Compare(int x, int y)
        {
            long a = codes[x];
            long b = codes[y];
            int agb = (a > b) ? 1 : 0;
            int alb = (a < b) ? 1 : 0;
            return (agb - alb);
        }
    }

    /* decode codebook arrangement is more heavily optimized than encode */
    public static int vorbis_book_init_decode(Codebook.codebook c, Codebook.static_codebook s) {
        int i, j, n = 0, tabn;
        int[] sortindex;
        c.clear();

  /* count actually used entries */
        for (i = 0; i < s.entries; i++)
            if (s.lengthlist[i] > 0)
                n++;

        c.entries = s.entries;
        c.used_entries = n;
        c.dim = s.dim;

        if (n > 0) {

    /* two different remappings go on here.

    First, we collapse the likely sparse codebook down only to
    actually represented values/words.  This collapsing needs to be
    indexed as map-valueless books are used to encode original entry
    positions as integers.

    Second, we reorder all vectors, including the entry index above,
    by sorted bitreversed codeword to allow treeless decode. */

    /* perform sort */
            long[] codes =_make_words(s.lengthlist, s.entries, c.used_entries);
            if (codes == null) return -1;

            int[] codep = new int[n];
            for (i = 0; i < n; i++) {
                codes[i] = bitreverse(codes[i]);
                codep[i] = i;
            }
            codepComparator comparator = new codepComparator();
            comparator.codes = codes;
            Array.Sort(codep, 0, n, comparator);

            sortindex = new int[n];
            c.codelist = new long[n];
    /* the index is a reverse index */
            for (i = 0; i < n; i++) {
                int position = codep[i];
                sortindex[position] = i;
            }
            for (i = 0; i < n; i++)
                c.codelist[sortindex[i]] = codes[i];


            c.valuelist = _book_unquantize(s, n, sortindex);
            c.dec_index = new int[n];

            for (n = 0, i = 0; i < s.entries; i++)
                if (s.lengthlist[i] > 0)
                    c.dec_index[sortindex[n++]] = i;

            c.dec_codelengths = new byte[n];
            for (n = 0, i = 0; i < s.entries; i++)
                if (s.lengthlist[i] > 0)
                    c.dec_codelengths[sortindex[n++]] = s.lengthlist[i];

            c.dec_firsttablen = _ilog((int) c.used_entries) - 4; /* this is magic */
            if (c.dec_firsttablen < 5) c.dec_firsttablen = 5;
            if (c.dec_firsttablen > 8) c.dec_firsttablen = 8;

            tabn = 1 << c.dec_firsttablen;
            c.dec_firsttable = new long[tabn];
            c.dec_maxlength = 0;

            for (i = 0; i < n; i++) {
                if (c.dec_maxlength < c.dec_codelengths[i])
                    c.dec_maxlength = c.dec_codelengths[i];
                if (c.dec_codelengths[i] <= c.dec_firsttablen) {
                    long orig = bitreverse(c.codelist[i]);
                    for (j = 0; j < (1 << (c.dec_firsttablen - c.dec_codelengths[i])); j++)
                        c.dec_firsttable[((int) (orig | (j << c.dec_codelengths[i])))] = i + 1;
                }
            }

    /* now fill in 'unused' entries in the firsttable with hi/lo search
       hints for the non-direct-hits */
            {
                long mask = (0xfffffffeL << (31 - c.dec_firsttablen));
                mask &= 0xFFFFFFFFL;
                long lo = 0, hi = 0;

                for (i = 0; i < tabn; i++) {
                    long word = (long)i << (32 - c.dec_firsttablen);
                    if (c.dec_firsttable[((int) bitreverse(word))] == 0) {
                        while ((lo + 1) < n && c.codelist[((int) (lo + 1))] <= word) lo++;
                        while (hi < n && word >= (c.codelist[((int) hi)] & mask)) hi++;

          /* we only actually have 15 bits per hint to play with here.
             In order to overflow gracefully (nothing breaks, efficiency
             just drops), encode as the difference from the extremes. */
                        {
                            long loval = lo;
                            long hival = n - hi;

                            if (loval > 0x7fff) loval = 0x7fff;
                            if (hival > 0x7fff) hival = 0x7fff;
                            c.dec_firsttable[((int) bitreverse(word))] = (0x80000000L | (loval << 15) | hival);
                        }
                    }
                }
            }
        }

        return (0);
    }
    }
}
