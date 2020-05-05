using SanAndreasUnity.Behaviours.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public class RadioStation
    {
        private static readonly string[] streams = { "CH", "CO", "CR", "DS", "HC", "MH", "MR", "NJ", "RE", "RG", "TK" };
        public static readonly string[] StationNames = { "PlayBack FM", "KRose", "K-DST", "Bounce FM", "SFUR", "Radio Los Santos", "Radio X", "CSR", "K-Jah", "Master Sounds", "WCTR" };
        private static readonly int startIndex = 3;
        private static readonly int endIndex = 114;

        private static RadioStation[] _stations;
        public static RadioStation[] stations
        {
            get
            {
                if (_stations == null)
                {
                    _stations = new RadioStation[streams.Length];
                    int i = 0;
                    foreach (var stream in streams)
                    {
                        _stations[i++] = new RadioStation(stream);
                    }
                }
                return _stations;
            }
        }

        private readonly string streamName;
        private int clipIndex;

        public AudioClip LoadCurrentClip() { return AudioManager.CreateAudioClipFromStream(streamName, clipIndex); }
        public float currentTime;

        private RadioStation(string streamName)
        {
            this.streamName = streamName;
            clipIndex = Random.Range(startIndex, endIndex);
            currentTime = 0.0f;
        }

        public void NextClip()
        {
            clipIndex++;
            if (clipIndex > endIndex)
                clipIndex = startIndex;
            currentTime = 0.0f;
        }

    }
}
