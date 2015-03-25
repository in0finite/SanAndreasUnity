using UnityEngine;
using System.Linq;
using Steamworks;

namespace Facepunch.Steam
{
    public class SteamVoiceRecorder : MonoBehaviour
    {
        internal static byte[] _voiceBufferCompressed = new byte[1024 * 64];
        internal static byte[] _voiceBufferUncompressed = new byte[1024 * 64];

        public bool Recording { get; private set; }

        public VoiceSpeaker Speaker;

        void Awake()
        {
            Recording = false;
        }

        public bool GetAvailableVoice(out byte[] bufferCompressed, out byte[] bufferUncompressed)
        {
            bufferCompressed = null;
            bufferUncompressed = null;

            if (!SteamService.IsInitialized) return false;

            if (!Recording)
            {
                return false;
            }

            uint bytesAvailableCompressed = 0;
            uint bytesAvailableUncompressed = 0;

            // find out if there's any recorded data
            EVoiceResult result = SteamUser.GetAvailableVoice(out bytesAvailableCompressed, out bytesAvailableUncompressed, 0);

            if (result == EVoiceResult.k_EVoiceResultNotRecording)
            {
                Recording = false;

                return false;
            }

            // if there's recorded data
            if (result == EVoiceResult.k_EVoiceResultOK && bytesAvailableCompressed > 0)
            {
                // retrieve recorded data
                uint bytesWrittenCompressed = 0;
                uint bytesWrittenUncompressed = 0;

                // we only care about the compressed data because that's what we'll network across
                result = SteamUser.GetVoice(true, _voiceBufferCompressed, (uint)_voiceBufferCompressed.Length, out bytesWrittenCompressed, false, null, 0, out bytesWrittenUncompressed, 0);

                // did we get it successfully?
                if (result == EVoiceResult.k_EVoiceResultOK && bytesWrittenCompressed > 0)
                {
                    result = SteamUser.DecompressVoice(_voiceBufferCompressed, bytesWrittenCompressed, _voiceBufferUncompressed, (uint)_voiceBufferUncompressed.Length, out bytesWrittenUncompressed, (uint)11025);

                    // was the decompress OK?
                    if (result == EVoiceResult.k_EVoiceResultOK && bytesWrittenUncompressed > 0)
                    {
                        bufferCompressed = _voiceBufferCompressed.Take((int)bytesWrittenCompressed).ToArray();
                        bufferUncompressed = _voiceBufferUncompressed.Take((int)bytesWrittenUncompressed).ToArray();

                        return true;
                    }
                }
            }

            return false;
        }

        public bool DecompressVoice(byte[] bufferCompressed, out byte[] bufferUncompressed)
        {
            bufferUncompressed = null;

            if (!SteamService.IsInitialized) return false;

            uint bytesWritten = 0;

            // try to decompress it
            Steamworks.EVoiceResult result = Steamworks.SteamUser.DecompressVoice(bufferCompressed, (uint)bufferCompressed.Length, _voiceBufferUncompressed, (uint)_voiceBufferUncompressed.Length, out bytesWritten, (uint)11025);

            // was the decompress OK?
            if (result == Steamworks.EVoiceResult.k_EVoiceResultOK && bytesWritten > 0)
            {
                bufferUncompressed = _voiceBufferUncompressed.Take((int)bytesWritten).ToArray();

                return true;
            }

            return false;
        }

        public void StartRecording()
        {
            if (!SteamService.IsInitialized) return;

            Steamworks.SteamUser.StartVoiceRecording();

            Recording = true;
        }

        public void StopRecording()
        {
            if (!SteamService.IsInitialized) return;

            Steamworks.SteamUser.StopVoiceRecording();
        }

        public void ToggleRecording()
        {
            if (Recording)
            {
                StopRecording();
            }
            else
            {
                StartRecording();
            }
        }
    }
}
