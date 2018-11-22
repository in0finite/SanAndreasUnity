using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TanjentOGG is released under the 3-clause BSD license. Please read license.txt for the full license.
namespace TanjentOGG
{
    public class Ogg
    {
        // decoded sample rate of the last page
        public int SampleRate;
        // decoded channels of the file
        public int Channels;

        // decoded samples, stored as channel-interleaved floats (-1 ... 1)
        // example for stereo data: [0] = L, [1] = R, [2] = L, [3] = R , ...
        float[] _DecodedFloats = new float[0];
        public float[] DecodedFloats
        {
            get
            {
                if (_DecodedFloats.Length == 0)
                    Decode();
                return _DecodedFloats;
            }
        }
        private int decodedFloatsIndex;
        private byte[] FileBytes;

        private Registry registry;
        private Vogg.ogg_sync_state oy; /* sync and verify incoming physical bitstream */
        private Vogg.ogg_stream_state os; /* take physical pages, weld into a logical stream of packets */
        private Vogg.ogg_page og; /* one Ogg bitstream page. Vorbis packets are inside */
        private Vogg.ogg_packet op; /* one raw packet of data for decode */

        private Codec.vorbis_info vi; /* struct that stores all the static vorbis bitstream settings */
        private Codec.vorbis_comment vc; /* struct that stores all the bitstream user comments */
        private Codec.vorbis_dsp_state vd; /* central working state for the packet->PCM decoder */
        private Codec.vorbis_block vb; /* local working space for packet->PCM decode */

        private CPtr.BytePtr buffer;
        private int bytes;

        private int convsize;
        private CPtr.BytePtr stdin;
        private int eos;

        public Ogg(byte[] fileBytes)
        {
            registry = new Registry();
            oy = new Vogg.ogg_sync_state();

            os = new Vogg.ogg_stream_state(); /* take physical pages, weld into a logical stream of packets */
            og = new Vogg.ogg_page(); /* one Ogg bitstream page. Vorbis packets are inside */
            op = new Vogg.ogg_packet(); /* one raw packet of data for decode */

            vi = new Codec.vorbis_info(); /* struct that stores all the static vorbis bitstream settings */
            vc = new Codec.vorbis_comment(); /* struct that stores all the bitstream user comments */
            vd = new Codec.vorbis_dsp_state(); /* central working state for the packet->PCM decoder */
            vb = new Codec.vorbis_block(); /* local working space for packet->PCM decode */
            FileBytes = fileBytes;
        }

        public void Decode()
        {
            DecodeToFloats(FileBytes);
        }

        private void DecodeToFloats(byte[] fileBytes)
        {

            this._DecodedFloats = new float[0];
            int totalSamples = 0;
            List<float[]> allFloats = new List<float[]>();

            Framing.ogg_sync_init(oy); /* Now we can read pages */
            stdin = new CPtr.BytePtr(fileBytes);

            convsize = 4096;

            while (true)
            {
                eos = 0;
                int i;

                /* grab some data at the head of the stream. We want the first page
            (which is guaranteed to be small and only contain the Vorbis
            stream initial header) We need the first page to get the stream
            serialno. */

                /* submit a 4k block to libvorbis' Ogg layer */
                buffer = Framing.ogg_sync_buffer(oy, 4096);
                bytes = fread(buffer, 1, 4096, stdin);
                Framing.ogg_sync_wrote(oy, bytes);

                /* Get the first page. */
                if (Framing.ogg_sync_pageout(oy, og) != 1)
                {
                    /* have we simply run out of data?  If so, we're done. */
                    if (bytes < 4096) break;

                    /* error case.  Must not be Vorbis data */
                    return;
                }

                /* Get the serial number and set up the rest of decode. */
                /* serialno first; use it to set up a logical stream */
                Framing.ogg_stream_init(os, Framing.ogg_page_serialno(og));

                /* extract the initial header from the first page and verify that the
            Ogg bitstream is in fact Vorbis data */

                /* I handle the initial header first instead of just having the code
                read all three Vorbis headers at once because reading the initial
                header is an easy way to identify a Vorbis bitstream and it's
                useful to see that functionality seperated out. */
                Info.vorbis_info_init(vi);
                Info.vorbis_comment_init(vc);
                if (Framing.ogg_stream_pagein(os, og) < 0)
                {
                    return;
                }

                if (Framing.ogg_stream_packetout(os, op) != 1)
                {
                    /* no page? must not be vorbis */
                    return;
                }

                if (Info.vorbis_synthesis_headerin(registry, vi, vc, op) < 0)
                {
                    /* error case; not a vorbis header */
                    return;
                }

                /* At this point, we're sure we're Vorbis. We've set up the logical
            (Ogg) bitstream decoder. Get the comment and codebook headers and
            set up the Vorbis decoder */

                /* The next two packets in order are the comment and codebook headers.
                They're likely large and may span multiple pages. Thus we read
                and submit data until we get our two packets, watching that no
                pages are missing. If a page is missing, error out; losing a
                header page is the only place where missing data is fatal. */

                i = 0;
                while (i < 2)
                { // page loop
                    while (i < 2)
                    { // packet loop
                        int result = Framing.ogg_sync_pageout(oy, og);
                        if (result == 0) break; /* Need more data */
                        /* Don't complain about missing or corrupt data yet. We'll
                        catch it at the packet output phase */
                        if (result == 1)
                        {
                            Framing.ogg_stream_pagein(os, og); /* we can ignore any errors here
                                                 as they'll also become apparent
												 at packetout */
                            while (i < 2)
                            {
                                result = Framing.ogg_stream_packetout(os, op);
                                if (result == 0) break;
                                if (result < 0)
                                {
                                    /* Uh oh; data at some point was corrupted or missing!
                                    We can't tolerate that in a header.  Die. */
                                    return;
                                }
                                result = Info.vorbis_synthesis_headerin(registry, vi, vc, op);
                                if (result < 0)
                                {
                                    // corrupt secondary header
                                    return;
                                }
                                i++;
                            }
                        }
                    }
                    /* no harm in not checking before adding more */
                    buffer = Framing.ogg_sync_buffer(oy, 4096);
                    bytes = fread(buffer, 1, 4096, stdin);
                    if (bytes == 0 && i < 2)
                    {
                        // eof before finding all vorbis headers
                        return;
                    }
                    Framing.ogg_sync_wrote(oy, bytes);
                }

                convsize = 4096 / vi.channels;
                this.Channels = vi.channels;

                /* OK, got and parsed all three headers. Initialize the Vorbis packet->PCM decoder. */
                if (Block.vorbis_synthesis_init(registry, vd, vi) == 0)
                {
                    /* central decode state */

                    Block.vorbis_block_init(vd, vb);          /* local state for most of the decode
                                              so multiple block decodes can
											  proceed in parallel. We could init
											  multiple vorbis_block structures
											  for vd here */

                    while (eos == 0)
                    {
                        while (eos == 0)
                        {
                            int result = Framing.ogg_sync_pageout(oy, og);
                            if (result == 0) break; /* need more data */
                            if (result < 0)
                            { /* missing or corrupt data at this page position */
                            }
                            else
                            {
                                Framing.ogg_stream_pagein(os, og); /* can safely ignore errors at this point */
                                while (true)
                                {
                                    result = Framing.ogg_stream_packetout(os, op);
                                    if (result == 0) break; /* need more data */
                                    if (result < 0)
                                    { /* missing or corrupt data at this page position */
                                        /* no reason to complain; already complained above */
                                    }
                                    else
                                    {
                                        /* we have a packet.  Decode it */
                                        CPtr.FloatPtr[] pcm = null;
                                        int samples;
                                        if (Synthesis.vorbis_synthesis(registry, vb, op) == 0)
                                        {
                                            /* test for success! */
                                            Block.vorbis_synthesis_blockin(registry, vd, vb);
                                        }
                                        /* pcm is a multichannel float vector.
                                        In stereo, for example, pcm[0] is left, and pcm[1] is right.
                                        samples is the size of each channel.
                                        Convert the float values (-1.<=range<=1.) to whatever PCM format and write it out */
                                        CPtr.FloatPtr[][] tpcm = new CPtr.FloatPtr[1][];
                                        tpcm[0] = pcm;
                                        while ((samples = Block.vorbis_synthesis_pcmout(vd, tpcm)) > 0)
                                        {
                                            pcm = tpcm[0];
                                            int j;
                                            int bout = (samples < convsize ? samples : convsize);
                                            float[] newFloats = new float[bout * vi.channels];
                                            for (i = 0; i < vi.channels; i++)
                                            {
                                                int p = i;
                                                for (j = pcm[i].offset; j < pcm[i].offset + bout; j++, p += vi.channels)
                                                {
                                                    newFloats[p] = pcm[i].floats[j];
                                                }
                                            }
                                            totalSamples += newFloats.Length;
                                            allFloats.Add(newFloats);

                                            Block.vorbis_synthesis_read(vd, bout); /* tell libvorbis how many samples we actually consumed */
                                        }
                                        this.SampleRate = (int)vi.rate;
                                    }
                                }
                                if (Framing.ogg_page_eos(og) != 0) eos = 1;
                            }
                        }
                        if (eos == 0)
                        {
                            buffer = Framing.ogg_sync_buffer(oy, 4096);
                            bytes = fread(buffer, 1, 4096, stdin);
                            Framing.ogg_sync_wrote(oy, bytes);
                            if (bytes == 0) eos = 1;
                        }
                    }
                }
                else
                {
                    // corrupt header during playback
                }
            }

            // assemble to short array
            this._DecodedFloats = new float[totalSamples];
            int lastSize = 0;
            for (int i = 0; i < allFloats.Count; i++)
            {
                Array.Copy(allFloats[i], 0, this.DecodedFloats, lastSize, allFloats[i].Length);
                lastSize += allFloats[i].Length;
            }

        }

        public int DecodeToFloatSamples(byte[] fileBytes, float[] floatSamples)
        {

            int i;
            int samplesReturned = 0;

            if (floatSamples == null) return samplesReturned;
            int samplesToDecode = floatSamples.Length;
            if (samplesToDecode <= 0)
            {
                return samplesReturned;
            }

            if (stdin == null)
            {
                eos = 0;
                this._DecodedFloats = new float[0];

                Framing.ogg_sync_init(oy); /* Now we can read pages */
                stdin = new CPtr.BytePtr(fileBytes);

                /* grab some data at the head of the stream. We want the first page
            (which is guaranteed to be small and only contain the Vorbis
            stream initial header) We need the first page to get the stream
            serialno. */

                /* submit a 4k block to libvorbis' Ogg layer */
                buffer = Framing.ogg_sync_buffer(oy, 4096);
                bytes = fread(buffer, 1, 4096, stdin);
                Framing.ogg_sync_wrote(oy, bytes);

                /* Get the first page. */
                if (Framing.ogg_sync_pageout(oy, og) != 1)
                {
                    /* have we simply run out of data?  If so, we're done. */
                    if (bytes < 4096)
                    {
                        eos = 1;
                        return samplesReturned;
                    }

                    /* error case.  Must not be Vorbis data */
                    eos = 1;
                    return -1;
                }

                /* Get the serial number and set up the rest of decode. */
                /* serialno first; use it to set up a logical stream */
                Framing.ogg_stream_init(os, Framing.ogg_page_serialno(og));

                /* extract the initial header from the first page and verify that the
            Ogg bitstream is in fact Vorbis data */

                /* I handle the initial header first instead of just having the code
                read all three Vorbis headers at once because reading the initial
                header is an easy way to identify a Vorbis bitstream and it's
                useful to see that functionality seperated out. */
                Info.vorbis_info_init(vi);
                Info.vorbis_comment_init(vc);
                if (Framing.ogg_stream_pagein(os, og) < 0)
                {
                    eos = 1;
                    return -1;
                }

                if (Framing.ogg_stream_packetout(os, op) != 1)
                {
                    /* no page? must not be vorbis */
                    eos = 1;
                    return -1;
                }

                if (Info.vorbis_synthesis_headerin(registry, vi, vc, op) < 0)
                {
                    /* error case; not a vorbis header */
                    eos = 1;
                    return -1;
                }

                /* At this point, we're sure we're Vorbis. We've set up the logical
            (Ogg) bitstream decoder. Get the comment and codebook headers and
            set up the Vorbis decoder */

                /* The next two packets in order are the comment and codebook headers.
                They're likely large and may span multiple pages. Thus we read
                and submit data until we get our two packets, watching that no
                pages are missing. If a page is missing, error out; losing a
                header page is the only place where missing data is fatal. */

                i = 0;
                while (i < 2)
                { // page loop
                    while (i < 2)
                    { // packet loop
                        int result = Framing.ogg_sync_pageout(oy, og);
                        if (result == 0) break; /* Need more data */
                        /* Don't complain about missing or corrupt data yet. We'll
                        catch it at the packet output phase */
                        if (result == 1)
                        {
                            Framing.ogg_stream_pagein(os, og); /* we can ignore any errors here
                                                 as they'll also become apparent
												 at packetout */
                            while (i < 2)
                            {
                                result = Framing.ogg_stream_packetout(os, op);
                                if (result == 0) break;
                                if (result < 0)
                                {
                                    /* Uh oh; data at some point was corrupted or missing!
                                    We can't tolerate that in a header.  Die. */
                                    eos = 1;
                                    return -1;
                                }
                                result = Info.vorbis_synthesis_headerin(registry, vi, vc, op);
                                if (result < 0)
                                {
                                    eos = 1;
                                    return -1;
                                }
                                i++;
                            }
                        }
                    }
                    /* no harm in not checking before adding more */
                    buffer = Framing.ogg_sync_buffer(oy, 4096);
                    bytes = fread(buffer, 1, 4096, stdin);
                    if (bytes == 0 && i < 2)
                    {
                        eos = 1;
                        return -1;
                    }
                    Framing.ogg_sync_wrote(oy, bytes);
                }

                convsize = 4096 / vi.channels;
                this.Channels = vi.channels;

                /* OK, got and parsed all three headers. Initialize the Vorbis packet->PCM decoder. */
                if (Block.vorbis_synthesis_init(registry, vd, vi) != 0)
                {
                    eos = 1;
                    return -1;
                }
                /* central decode state */
                Block.vorbis_block_init(vd, vb);          /* local state for most of the decode
                                              so multiple block decodes can
											  proceed in parallel. We could init
											  multiple vorbis_block structures
											  for vd here */
            }

            while (eos == 0)
            {
                while (eos == 0)
                {

                    if (this.decodedFloatsIndex < this.DecodedFloats.Length)
                    {
                        int minSamplesToProcess = Math.Min(samplesToDecode, this.DecodedFloats.Length - this.decodedFloatsIndex);
                        minSamplesToProcess = Math.Min(minSamplesToProcess, samplesToDecode - samplesReturned);
                        Array.Copy(this.DecodedFloats, this.decodedFloatsIndex, floatSamples, samplesReturned, minSamplesToProcess);
                        this.decodedFloatsIndex += minSamplesToProcess;
                        samplesReturned += minSamplesToProcess;
                        if (samplesReturned >= samplesToDecode)
                        {
                            return samplesReturned;
                        }
                    }

                    int result = Framing.ogg_sync_pageout(oy, og);
                    if (result == 0) break; /* need more data */
                    if (result < 0)
                    { /* missing or corrupt data at this page position */
                    }
                    else
                    {
                        Framing.ogg_stream_pagein(os, og); /* can safely ignore errors at this point */
                        while (true)
                        {
                            result = Framing.ogg_stream_packetout(os, op);
                            if (result == 0) break; /* need more data */
                            if (result < 0)
                            { /* missing or corrupt data at this page position */
                                /* no reason to complain; already complained above */
                            }
                            else
                            {
                                /* we have a packet.  Decode it */
                                CPtr.FloatPtr[] pcm = null;
                                int samples;
                                if (Synthesis.vorbis_synthesis(registry, vb, op) == 0)
                                {
                                    /* test for success! */
                                    Block.vorbis_synthesis_blockin(registry, vd, vb);
                                }


                                /*

                    **pcm is a multichannel float vector.  In stereo, for
                    example, pcm[0] is left, and pcm[1] is right.  samples is
                    the size of each channel.  Convert the float values
                    (-1.<=range<=1.) to whatever PCM format and write it out */

                                CPtr.FloatPtr[][] tpcm = new CPtr.FloatPtr[1][];
                                tpcm[0] = pcm;
                                while ((samples = Block.vorbis_synthesis_pcmout(vd, tpcm)) > 0)
                                {
                                    pcm = tpcm[0];
                                    int j;
                                    int bout = (samples < convsize ? samples : convsize);
                                    int extraSamples = 0;
                                    if (this.decodedFloatsIndex < this.DecodedFloats.Length)
                                    {
                                        extraSamples = this.DecodedFloats.Length - this.decodedFloatsIndex;
                                    }
                                    float[] newFloats = new float[bout * vi.channels + extraSamples];
                                    Array.Copy(this.DecodedFloats, this.decodedFloatsIndex, newFloats, 0, extraSamples);
                                    for (i = 0; i < vi.channels; i++)
                                    {
                                        int p = extraSamples + i;
                                        for (j = pcm[i].offset; j < pcm[i].offset + bout; j++, p += vi.channels)
                                        {
                                            newFloats[p] = pcm[i].floats[j];
                                        }
                                    }
                                    this.decodedFloatsIndex = 0;
                                    this._DecodedFloats = newFloats;
                                    Block.vorbis_synthesis_read(vd, bout); /* tell libvorbis how many samples we actually consumed */
                                }
                                this.SampleRate = (int)vi.rate;
                            }
                        }
                        if (Framing.ogg_page_eos(og) != 0) eos = 1;
                    }
                }
                if (eos == 0)
                {
                    buffer = Framing.ogg_sync_buffer(oy, 4096);
                    bytes = fread(buffer, 1, 4096, stdin);
                    Framing.ogg_sync_wrote(oy, bytes);
                    if (bytes == 0) eos = 1;
                }
            }

            if (eos == 1)
            {
                if (this.decodedFloatsIndex < this.DecodedFloats.Length)
                {
                    int minSamplesToProcess = Math.Min(samplesToDecode, this.DecodedFloats.Length - this.decodedFloatsIndex);
                    minSamplesToProcess = Math.Min(minSamplesToProcess, samplesToDecode - samplesReturned);
                    Array.Copy(this.DecodedFloats, this.decodedFloatsIndex, floatSamples, samplesReturned, minSamplesToProcess);
                    this.decodedFloatsIndex += minSamplesToProcess;
                    samplesReturned += minSamplesToProcess;
                    if (samplesReturned >= samplesToDecode)
                    {
                        return samplesReturned;
                    }
                }
            }

            return samplesReturned;
        }


        private int fread(CPtr.BytePtr ptr, int size, int count, CPtr.BytePtr stream)
        {
            for (int i = 0; i < (size * count); i++)
            {
                if (stream.offset >= stream.bytes.Length)
                {
                    return i;
                }
                ptr.bytes[ptr.offset + i] = stream.bytes[stream.offset];
                stream.offset++;
            }
            return size * count;
        }
    }
}
