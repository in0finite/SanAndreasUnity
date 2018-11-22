using UnityEngine;
using System.Collections;

public class RawClip : Object
{
    byte[] bytes;
    public int SampleRate;
    float[] _DecodedFloats = new float[0];
    public float[] DecodedFloats { get { return _DecodedFloats = (_DecodedFloats.Length == 0 )? bytesToFloats() : _DecodedFloats; } }
    // Use this for initialization
    public RawClip(int samplerate,byte[] bytes)
    {
        this.bytes = bytes;
        this.SampleRate = samplerate;
    }

    // Update is called once per frame
    private float[] bytesToFloats()
    {
        // Convert to floating point PCM
        float[] float_pcm = new float[bytes.Length / sizeof(short)];
        for (int i = 0; i < float_pcm.Length; i++)
        {
            float_pcm[i] = Mathf.Clamp((((short)(bytes[i * sizeof(short)] | (bytes[(i * sizeof(short)) + 1] << 8))) / 32767.5f), - 1.0f, 1.0f);
        }
        return float_pcm;
    }
}
