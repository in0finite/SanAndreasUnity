using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TanjentOGG is released under the 3-clause BSD license. Please read license.txt for the full license.
namespace TanjentOGG
{
    public class Res012
    {
        static int ilog(int v)
        {
            int ret = 0;
            while (v != 0)
            {
                ret++;
                v >>= 1;
            }
            return (ret);
        }

        static int icount(int v)
        {
            int ret = 0;
            while (v != 0)
            {
                ret += v & 1;
                v >>= 1;
            }
            return (ret);
        }

        public class vorbis_info_residue0 : Codec.vorbis_info_residue
        {
            /* block-partitioned VQ coded straight residue */
           public long begin;
           public long end;

            /* first stage (lossless partitioning) */
           public int grouping;         /* group n vectors per partition */
           public int partitions;       /* possible codebooks for a partition */
           public int partvals;         /* partitions ^ groupbook dim */
           public int groupbook;        /* huffbook for partitioning */
           public int[] secondstages = new int[64]; /* expanded out to pointers in lookup */
           public int[] booklist = new int[512];    /* list of second stage books */

        }

        public class vorbis_look_residue0 : Codec.vorbis_look_residue
        {
            public vorbis_info_residue0 info;

            public int parts;
            public int stages;
            public Codebook.codebook[] fullbooks;
            public Codebook.codebook phrasebook;
            public Codebook.codebook[][] partbooks;

            public int partvals;
            public int[][] decodemap;

        }

        /* vorbis_info is for range checking */
        public static Codec.vorbis_info_residue res0_unpack(Codec.vorbis_info vi, Vogg.oggpack_buffer opb)
        {
            int j, acc = 0;
            vorbis_info_residue0 info = new vorbis_info_residue0();
            Codec.codec_setup_info ci = vi.codec_setup;

            info.begin = Bitwise.oggpack_read(opb, 24);
            info.end = Bitwise.oggpack_read(opb, 24);
            info.grouping = (int)(Bitwise.oggpack_read(opb, 24) + 1);
            info.partitions = (int)(Bitwise.oggpack_read(opb, 6) + 1);
            info.groupbook = (int)Bitwise.oggpack_read(opb, 8);

            /* check for premature EOP */
            if (info.groupbook < 0) return null;

            for (j = 0; j < info.partitions; j++)
            {
                int cascade = (int)Bitwise.oggpack_read(opb, 3);
                int cflag = (int)Bitwise.oggpack_read(opb, 1);
                if (cflag < 0) return null;
                if (cflag != 0)
                {
                    int c = (int)Bitwise.oggpack_read(opb, 5);
                    if (c < 0) return null;
                    cascade |= (c << 3);
                }
                info.secondstages[j] = cascade;

                acc += icount(cascade);
            }
            for (j = 0; j < acc; j++)
            {
                int book = (int)Bitwise.oggpack_read(opb, 8);
                if (book < 0) return null;
                info.booklist[j] = book;
            }

            if (info.groupbook >= ci.books) return null;
            for (j = 0; j < acc; j++)
            {
                if (info.booklist[j] >= ci.books) return null;
                if (ci.book_param[info.booklist[j]].maptype == 0) return null;
            }

            /* verify the phrasebook is not specifying an impossible or
               inconsistent partitioning scheme. */
            /* modify the phrasebook ranging check from r16327; an early beta
               encoder had a bug where it used an oversized phrasebook by
               accident.  These files should continue to be playable, but don't
               allow an exploit */
            {
                int entries = (int)ci.book_param[info.groupbook].entries;
                int dim = (int)ci.book_param[info.groupbook].dim;
                int partvals = 1;
                if (dim < 1) return null;
                while (dim > 0)
                {
                    partvals *= info.partitions;
                    if (partvals > entries) return null;
                    dim--;
                }
                info.partvals = partvals;
            }

            return (info);
        }

        public static Codec.vorbis_look_residue res0_look(Codec.vorbis_dsp_state vd,
                                                          Codec.vorbis_info_residue vr)
        {
            vorbis_info_residue0 info = (vorbis_info_residue0)vr;
            vorbis_look_residue0 look = new vorbis_look_residue0();
            Codec.codec_setup_info ci = vd.vi.codec_setup;

            int j, k, acc = 0;
            int dim;
            int maxstage = 0;
            look.info = info;

            look.parts = info.partitions;
            look.fullbooks = ci.fullbooks;
            look.phrasebook = ci.fullbooks[info.groupbook];
            dim = (int)look.phrasebook.dim;

            look.partbooks = new Codebook.codebook[look.parts][];

            for (j = 0; j < look.parts; j++)
            {
                int stages = ilog(info.secondstages[j]);
                if (stages != 0)
                {
                    if (stages > maxstage) maxstage = stages;
                    look.partbooks[j] = new Codebook.codebook[stages];
                    for (k = 0; k < stages; k++)
                        if ((info.secondstages[j] & (1 << k)) != 0)
                        {
                            look.partbooks[j][k] = ci.fullbooks[info.booklist[acc++]];
                        }
                }
            }

            look.partvals = 1;
            for (j = 0; j < dim; j++)
                look.partvals *= look.parts;

            look.stages = maxstage;
            look.decodemap = new int[look.partvals][];
            for (j = 0; j < look.partvals; j++)
            {
                long val = j;
                long mult = look.partvals / look.parts;
                look.decodemap[j] = new int[dim];
                for (k = 0; k < dim; k++)
                {
                    long deco = val / mult;
                    val -= deco * mult;
                    mult /= look.parts;
                    look.decodemap[j][k] = (int)deco;
                }
            }
            return (look);
        }

        /* a truncated packet here just means 'stop working'; it's not an error */
        static int _01inverse(Codec.vorbis_block vb, Codec.vorbis_look_residue vl,
                              CPtr.FloatPtr[] pin, int ch,
                              bool use_decodevs)
        {

            long i, j, k, l, s;
            vorbis_look_residue0 look = (vorbis_look_residue0)vl;
            vorbis_info_residue0 info = look.info;

            /* move all this setup out later */
            int samples_per_partition = info.grouping;
            int partitions_per_word = (int)look.phrasebook.dim;
            int max = vb.pcmend >> 1;
            int end = info.end < max ? (int)info.end : max;
            int n = (int)(end - info.begin);

            if (n > 0)
            {
                int partvals = n / samples_per_partition;
                int partwords = (partvals + partitions_per_word - 1) / partitions_per_word;
                int[][][] partword = new int[ch][][];

                for (j = 0; j < ch; j++)
                    partword[((int)j)] = new int[partwords][];

                for (s = 0; s < look.stages; s++)
                {

                    /* each loop decodes on partition codeword containing
                       partitions_per_word partitions */
                    for (i = 0, l = 0; i < partvals; l++)
                    {
                        if (s == 0)
                        {
                            /* fetch the partition word for each channel */
                            for (j = 0; j < ch; j++)
                            {
                                int temp = (int)Codebook.vorbis_book_decode(look.phrasebook, vb.opb);

                                if (temp == -1 || temp >= info.partvals) return 0;
                                partword[((int)j)][((int)l)] = look.decodemap[temp];
                                if (partword[((int)j)][((int)l)] == null) return 0;
                            }
                        }

                        /* now we decode residual values for the partitions */
                        for (k = 0; k < partitions_per_word && i < partvals; k++, i++)
                            for (j = 0; j < ch; j++)
                            {
                                long offset = info.begin + i * samples_per_partition;
                                if ((info.secondstages[partword[((int)j)][((int)l)][((int)k)]] & (1 << (int)s)) != 0)
                                {
                                    Codebook.codebook stagebook = look.partbooks[partword[((int)j)][((int)l)][((int)k)]][((int)s)];
                                    if (stagebook != null)
                                    {
                                        if (use_decodevs == true)
                                        {
                                            if (Codebook.vorbis_book_decodevs_add(stagebook, new CPtr.FloatPtr(pin[((int)j)], (int)offset), vb.opb, samples_per_partition) == -1)
                                                return 0;
                                        }
                                        else
                                        {
                                            if (Codebook.vorbis_book_decodev_add(stagebook, new CPtr.FloatPtr(pin[((int)j)], (int)offset), vb.opb, samples_per_partition) == -1)
                                                return 0;
                                        }
                                    }
                                }
                            }
                    }
                }
            }
            return (0);
        }

        public static int res0_inverse(Codec.vorbis_block vb, Codec.vorbis_look_residue vl,
                                       CPtr.FloatPtr[] pin, int[] nonzero, int ch)
        {
            int i, used = 0;
            for (i = 0; i < ch; i++)
                if (nonzero[i] != 0)
                    pin[used++] = pin[i];
            if (used != 0)
                return (_01inverse(vb, vl, pin, used, true));
            else
                return (0);
        }

        public static int res1_inverse(Codec.vorbis_block vb, Codec.vorbis_look_residue vl,
                                       CPtr.FloatPtr[] pin, int[] nonzero, int ch)
        {
            int i, used = 0;
            for (i = 0; i < ch; i++)
                if (nonzero[i] != 0)
                    pin[used++] = pin[i];
            if (used != 0)
                return (_01inverse(vb, vl, pin, used, false));
            else
                return (0);
        }

        /* duplicate code here as speed is somewhat more important */
        public static int res2_inverse(Codec.vorbis_block vb, Codec.vorbis_look_residue vl,
                                       CPtr.FloatPtr[] pin, int[] nonzero, int ch)
        {
            long i, k, l, s;
            vorbis_look_residue0 look = (vorbis_look_residue0)vl;
            vorbis_info_residue0 info = look.info;

            /* move all this setup out later */
            int samples_per_partition = info.grouping;
            int partitions_per_word = (int)look.phrasebook.dim;
            int max = (vb.pcmend * ch) >> 1;
            int end = info.end < max ? (int)info.end : max;
            int n = (int)(end - info.begin);

            if (n > 0)
            {
                int partvals = n / samples_per_partition;
                int partwords = (partvals + partitions_per_word - 1) / partitions_per_word;
                int[][] partword = new int[partwords][];

                for (i = 0; i < ch; i++) if (nonzero[((int)i)] != 0) break;
                if (i == ch) return (0); /* no nonzero vectors */

                for (s = 0; s < look.stages; s++)
                {
                    for (i = 0, l = 0; i < partvals; l++)
                    {

                        if (s == 0)
                        {
                            /* fetch the partition word */
                            int temp = (int)Codebook.vorbis_book_decode(look.phrasebook, vb.opb);
                            if (temp == -1 || temp >= info.partvals) return 0;
                            partword[((int)l)] = look.decodemap[temp];
                            if (partword[((int)l)] == null) return 0;
                        }

                        /* now we decode residual values for the partitions */
                        for (k = 0; k < partitions_per_word && i < partvals; k++, i++)
                            if ((info.secondstages[partword[((int)l)][((int)k)]] & (1 << (int)s)) != 0)
                            {
                                Codebook.codebook stagebook = look.partbooks[partword[((int)l)][((int)k)]][((int)s)];

                                if (stagebook != null)
                                {
                                    if (Codebook.vorbis_book_decodevv_add(stagebook, pin, i * samples_per_partition + info.begin, ch, vb.opb, samples_per_partition) == -1)
                                        return 0;
                                }
                            }
                    }
                }
            }
            return (0);
        }
    }
}
