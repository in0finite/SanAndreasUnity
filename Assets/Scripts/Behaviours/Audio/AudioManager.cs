using System.Collections.Generic;
using UnityEngine;
using GTAAudioSharp;
using System.IO;
using SanAndreasUnity.Audio;

namespace SanAndreasUnity.Behaviours.Audio
{
	public struct SoundId
    {
		public bool isStream;
		public string fileName;
		public int bankIndex;
		public int audioIndex;
	}


	public class AudioManager : MonoBehaviour
	{

		public static AudioManager Instance { get; private set; }

		private static GTAAudioFiles s_gtaAudioFiles;
		public static GTAAudioFiles AudioFiles { get { return s_gtaAudioFiles; } }

	//	private Dictionary<string, AudioClip> sfxAudioClips = new Dictionary<string, AudioClip>();

	//	private Dictionary<string, AudioClip> streamsAudioClips = new Dictionary<string, AudioClip>();

	//	private Dictionary<string, AudioStream> streamsAudioStreams = new Dictionary<string, AudioStream>();

		public bool playStartupSound = true;
		public float startupSoundTimeOffset = 0f;
		static AudioSource s_startupAudioSource;

		public const int kSfxSampleSize = 2;

		private static Dictionary<SoundId, AudioClip> _cachedMouthAudioClips = new Dictionary<SoundId, AudioClip>();



		void Awake ()
		{
			Instance = this;
		}

		void OnDisable ()
		{
			if (s_gtaAudioFiles != null)
			{
				s_gtaAudioFiles.Dispose ();
				s_gtaAudioFiles = null;
			}
		}

		void Start () {
			
		}

		void OnLoaderFinished ()
		{
			if (s_startupAudioSource != null)
			{
				Destroy (s_startupAudioSource.gameObject);
			}
		}


		public static void InitFromLoader ()
		{

			s_gtaAudioFiles = GTAAudio.OpenRead (Path.Combine (Utilities.Config.GamePath, "audio"));

			if (Instance.playStartupSound)
			{
				s_startupAudioSource = PlayStream ("Beats", 1);
				if (s_startupAudioSource != null)
					s_startupAudioSource.time = Instance.startupSoundTimeOffset;
			}

		}


		public static AudioClip CreateAudioClipFromStream (string streamFileName, int bankIndex)
		{
			AudioStream audio_stream = null;
			System.DateTime startTime = System.DateTime.Now;

			int streams_bank_index = bankIndex;
			string streams_file_name = streamFileName;
			string key = streams_file_name + "." + streams_bank_index;


			if ((s_gtaAudioFiles != null) && (streams_bank_index >= 0))
			{
				try
				{
					Stream stream = s_gtaAudioFiles.OpenStreamsAudioStreamByName(streams_file_name, (uint)streams_bank_index);
					if (stream != null)
					{
						audio_stream = new AudioStream(stream, key, true);
					}
				}
				catch (System.Exception e)
				{
					Debug.LogError(e);
				}
			}


			if (audio_stream != null)
			{
				System.TimeSpan time_span = System.DateTime.Now - startTime;
				Debug.Log("\"" + key + "\" took " + time_span.TotalSeconds + " seconds.");

				return audio_stream.AudioClip;
			}

			return null;
		}

		public static AudioSource PlayStream (string streamFileName, int bankIndex)
		{
			var clip = CreateAudioClipFromStream (streamFileName, bankIndex);
			if (clip != null)
			{
				AudioSource audioSource = new GameObject (streamFileName + "." + bankIndex).AddComponent<AudioSource> ();
				audioSource.time = 0.0f;
				audioSource.clip = clip;
				audioSource.Play();

				// destroy game object when sound is finished playing
				Destroy( audioSource.gameObject, clip.length );

				return audioSource;
			}

			return null;
		}

		public static AudioClip CreateAudioClipFromSfx (string sfxFileName, int bankIndex, int audioIndex)
		{

			if (null == s_gtaAudioFiles || bankIndex < 0 || audioIndex < 0)
				return null;
			
			AudioClip ret = null;
			string clipName = sfxFileName + "." + bankIndex + "." + audioIndex;

			try
			{
				using (GTAAudioStream audio_stream = s_gtaAudioFiles.OpenSFXAudioStreamByName(sfxFileName, (uint)bankIndex, (uint)audioIndex))
				{
					if (audio_stream != null)
					{
						ret = CreateAudioClipFromSfx (clipName, audio_stream);
					}
				}
			}
			catch (System.Exception e)
			{
				Debug.LogError(e);
			}

			return ret;
		}

		static AudioClip CreateAudioClipFromSfx (string clipName, GTAAudioStream audio_stream)
		{
			AudioClip ret = null;

			byte[] int_pcm = new byte[audio_stream.Length];

			if (audio_stream.Read(int_pcm, 0, int_pcm.Length) == int_pcm.Length)
			{
				float[] float_pcm = new float[int_pcm.Length / sizeof(short)];
				for (int i = 0; i < float_pcm.Length; i++)
				{
					float_pcm[i] = Mathf.Clamp(((short)(int_pcm[i * sizeof(short)] | (int_pcm[(i * sizeof(short)) + 1] << 8)) / 32767.5f), -1.0f, 1.0f);
				}
				ret = AudioClip.Create(clipName, float_pcm.Length, 1, audio_stream.SampleRate, false);
				ret.SetData(float_pcm, 0);

				// Debug.LogFormat("loaded sfx: name {0}, offset {1}, size {2}, length {3}, bitrate Kb/s {4}, stream size {5}, freq {6}", 
				// 	clipName, 0, 0, ret.length,
				// 	FreqToKbs (audio_stream.SampleRate), audio_stream.Length, audio_stream.SampleRate);
			}

			return ret;
		}

		public static AudioClip CreateAudioClip(SoundId soundId)
        {
			return soundId.isStream
				? CreateAudioClipFromStream(soundId.fileName, soundId.bankIndex)
				: CreateAudioClipFromSfx(soundId.fileName, soundId.bankIndex, soundId.audioIndex);
        }

		public static AudioClip GetAudioClipCached(SoundId soundId)
        {
			if (!_cachedMouthAudioClips.TryGetValue(soundId, out AudioClip audioClip))
			{
				audioClip = AudioManager.CreateAudioClip(soundId);
				_cachedMouthAudioClips.Add(soundId, audioClip);
			}

			return audioClip;
		}

		static float FreqToKbs(int freq)
		{
			return freq * kSfxSampleSize * 8f / 1000f;
		}

		static float KbsToFreq(float kbs)
		{
			return kbs / 8f * 1000f / kSfxSampleSize;
		}

	}

}
