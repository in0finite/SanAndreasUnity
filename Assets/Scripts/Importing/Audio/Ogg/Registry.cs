using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TanjentOGG is released under the 3-clause BSD license. Please read license.txt for the full license.
namespace TanjentOGG
{
    public class Registry
    {
            public static  int VI_TRANSFORMB = 1;
    public static  int VI_WINDOWB = 1;
    public static  int VI_TIMEB = 1;
    public static  int VI_FLOORB = 2;
    public static  int VI_RESB = 3;
    public static  int VI_MAPB = 1;


    // Floor backend generic
    public class vorbis_func_floor {
        public virtual Codec.vorbis_look_floor look(Codec.vorbis_dsp_state vd, Codec.vorbis_info_floor i)
        {
            return Floor0.floor0_look(vd, i);
        }

        public virtual float[] inverse1(Codec.vorbis_block vb, Codec.vorbis_look_floor i)
        {
            return Floor0.floor0_inverse1(vb, i);
        }

        public virtual int inverse2(Codec.vorbis_block vb, Codec.vorbis_look_floor i, float[] memo, CPtr.FloatPtr pout)
        {
            return Floor0.floor0_inverse2(vb, i, memo, pout);
        }

        public virtual Codec.vorbis_info_floor unpack(Codec.vorbis_info vi, Vogg.oggpack_buffer opb)
        {
            return Floor0.floor0_unpack(vi, opb);
        }
    }

    public class vorbis_func_floor1 : vorbis_func_floor {
        override
        public Codec.vorbis_look_floor look(Codec.vorbis_dsp_state vd, Codec.vorbis_info_floor i)
        {
            return Floor1.floor1_look(vd, i);
        }

        override
        public float[] inverse1(Codec.vorbis_block vb, Codec.vorbis_look_floor i)
        {
            return Floor1.floor1_inverse1(vb, i);
        }

        override
        public int inverse2(Codec.vorbis_block vb, Codec.vorbis_look_floor i, float[] memo, CPtr.FloatPtr pout)
        {
            return Floor1.floor1_inverse2(vb, i, memo, pout);
        }

        override
        public Codec.vorbis_info_floor unpack(Codec.vorbis_info vi, Vogg.oggpack_buffer opb)
        {
            return Floor1.floor1_unpack(vi, opb);
        }
    }

    // Residue backend generic
    public class vorbis_func_residue {
        public virtual Codec.vorbis_look_residue look(Codec.vorbis_dsp_state vd, Codec.vorbis_info_residue vr) {
            return Res012.res0_look(vd, vr);
        }

        public virtual int inverse(Codec.vorbis_block vb, Codec.vorbis_look_residue vl, CPtr.FloatPtr[] pin, int[] nonzero, int ch)
        {
            return Res012.res0_inverse(vb, vl, pin, nonzero, ch);
        }

        public virtual Codec.vorbis_info_residue unpack(Codec.vorbis_info vi, Vogg.oggpack_buffer opb)
        {
            return Res012.res0_unpack(vi, opb);
        }
    }

    public class vorbis_func_residue1 : vorbis_func_residue {
        override
        public int inverse(Codec.vorbis_block vb, Codec.vorbis_look_residue vl, CPtr.FloatPtr[] pin, int[] nonzero, int ch) {
            return Res012.res1_inverse(vb, vl, pin, nonzero, ch);
        }
    }

    public class vorbis_func_residue2 : vorbis_func_residue {
        override
        public int inverse(Codec.vorbis_block vb, Codec.vorbis_look_residue vl, CPtr.FloatPtr[] pin, int[] nonzero, int ch)
        {
            return Res012.res2_inverse(vb, vl, pin, nonzero, ch);
        }
    }

    // Mapping backend generic
    public class vorbis_func_mapping {
        public Codec.vorbis_info_mapping unpack(Codec.vorbis_info vi, Vogg.oggpack_buffer opb)
        {
            return Mapping0.mapping0_unpack(vi, opb);
        }

        public int inverse(Registry r, Codec.vorbis_block vb, Codec.vorbis_info_mapping l)
        {
            return Mapping0.mapping0_inverse(r, vb, l);
        }
    }

    public class vorbis_info_mapping0 : Codec.vorbis_info_mapping {
        public int submaps;  // <= 16
        public int[] chmuxlist = new int[256];   // up to 256 channels in a Vorbis stream

        public int[] floorsubmap = new int[16];   // [mux] submap to floors
        public int[] residuesubmap = new int[16]; // [mux] submap to residue

        public int[] psy = new int[2]; // by blocktype; impulse/padding for short, transition/normal for long

        public int coupling_steps;
        public int[] coupling_mag = new int[256];
        public int[] coupling_ang = new int[256];

        public void clear()
        {
            submaps = 0;
            psy[0] = 0;
            psy[1] = 0;
            coupling_steps = 0;

            for (int i = 0; i < 16; i++) {
                floorsubmap[i] = 0;
                residuesubmap[i] = 0;
            }
            for (int i = 0; i < 256; i++) {
                chmuxlist[i] = 0;
                coupling_ang[i] = 0;
                coupling_mag[i] = 0;
            }
        }
    }

    public vorbis_func_floor floor0_exportbundle;
    public vorbis_func_floor1 floor1_exportbundle;

    public vorbis_func_residue residue0_exportbundle;
    public vorbis_func_residue1 residue1_exportbundle;
    public vorbis_func_residue2 residue2_exportbundle;

    public vorbis_func_mapping mapping0_exportbundle;

    public vorbis_func_floor[] _floor_P;
    public vorbis_func_residue[] _residue_P;
    public vorbis_func_mapping[] _mapping_P;

    public float[][] vwin;

    public Registry()
    {
        floor0_exportbundle = new vorbis_func_floor();
        floor1_exportbundle = new vorbis_func_floor1();
        _floor_P = new vorbis_func_floor[2];
        _floor_P[0] = floor0_exportbundle;
        _floor_P[1] = floor1_exportbundle;

        residue0_exportbundle = new vorbis_func_residue();
        residue1_exportbundle = new vorbis_func_residue1();
        residue2_exportbundle = new vorbis_func_residue2();
        _residue_P = new vorbis_func_residue[3];
        _residue_P[0] = residue0_exportbundle;
        _residue_P[1] = residue1_exportbundle;
        _residue_P[2] = residue2_exportbundle;

        mapping0_exportbundle = new vorbis_func_mapping();
        _mapping_P = new vorbis_func_mapping[1];
        _mapping_P[0] = mapping0_exportbundle;

        vwin = new float[8][];
        vwin[0] = Window.vwin64;
        vwin[1] = Window.vwin128;
        vwin[2] = Window.vwin256;
        vwin[3] = Window.vwin512;
        vwin[4] = Window.vwin1024;
        vwin[5] = Window.vwin2048;
        vwin[6] = Window.vwin4096;
        vwin[7] = Window.vwin8192;
    }
    }
}
