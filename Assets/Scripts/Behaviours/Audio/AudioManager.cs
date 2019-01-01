using System.Collections.Generic;
using UnityEngine;
using GTAAudioSharp;
using System.IO;
using SanAndreasUnity.Audio;

namespace SanAndreasUnity.Behaviours.Audio
{
	
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

	//	static AudioClip s_sfxGENRLClip;
	//	public static AudioClip SfxGENRLClip { get { return s_sfxGENRLClip; } }

		// describes single sound inside sfx bank
		public struct SfxBankAudioData
		{
			public int startTimeMs;
			public int lengthMs;
			public float lengthSeconds { get { return this.lengthMs / 1000f; } }
			public int bitrateKbs;	// kb/s
			public int offsetInBytes;
			public int bitsPerSecond { get { return this.bitrateKbs * 1000; } }
			public float bytesPerSecond { get { return this.bitsPerSecond / 8f; } }
			public int sizeInBytes { get { return Mathf.RoundToInt( this.lengthSeconds * this.bytesPerSecond ); } }
		}

		static SfxBankAudioData[] s_sfxGENRL137Timings;
		public static SfxBankAudioData[] SfxGENRL137Timings { get { return s_sfxGENRL137Timings; } }



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

		void Update () {
			
		}


		public static void InitFromLoader ()
		{

			s_gtaAudioFiles = GTAAudio.OpenRead (Path.Combine (Utilities.Config.GamePath, "audio"));

			s_sfxGENRL137Timings = LoadSfxBankTimings (Path.Combine (Utilities.Config.DataPath, "SFX_GENRL_137.txt"));

			if (Instance.playStartupSound)
			{
				s_startupAudioSource = PlayStream ("Beats", 1);
				if (s_startupAudioSource != null)
					s_startupAudioSource.time = Instance.startupSoundTimeOffset;
			}

		//	s_sfxGENRLClip = CreateAudioClipFromSfx ("GENRL", 136, 0);

		}

		public static SfxBankAudioData[] LoadSfxBankTimings (string filePath)
		{
		//	int sampleSize = 2;
			System.IFormatProvider formatProvider = System.Globalization.CultureInfo.InvariantCulture;
			var lines = File.ReadAllLines (filePath);
			SfxBankAudioData[] datas = new SfxBankAudioData[(lines.Length - 1) / 2];	// skip last line
		//	int currentTime = 0;
			float currentOffset = 0;
			for (int i = 0, lineIndex = 0; i < datas.Length; i++)
			{
			//	datas [i].startTimeMs = currentTime;
				datas [i].offsetInBytes = Mathf.RoundToInt (currentOffset);

				int clipLengthMs = int.Parse (lines [lineIndex++], formatProvider);
				datas [i].lengthMs = clipLengthMs;

				int bitrate = 288;//int.Parse (lines [lineIndex++], formatProvider);
				lineIndex ++;
				datas [i].bitrateKbs = bitrate;

			//	currentTime += clipLengthMs;
				currentOffset += (clipLengthMs / 1000f) * (bitrate * 1000f / 8f);
			}
			Debug.LogFormat ("sfx timings loaded - current offset {0}", currentOffset);
			return datas;
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

		public static AudioClip CreateAudioClipFromSfx (string sfxName, int bankIndex, int soundIndex, SfxBankAudioData sfxBankAudioData)
		{

			AudioClip ret = null;
			string sfx_file_name = sfxName;
			int sfx_bank_index = bankIndex;
			int sfx_audio_index = soundIndex;
			string key = sfx_file_name + "." + sfx_bank_index + "." + sfx_audio_index;

			const int sampleSize = 2;
			int frequency = Mathf.FloorToInt( sfxBankAudioData.bytesPerSecond / sampleSize );

			long audioStreamSize = -1, originalStreamPosition = -1, audioStreamSampleRate = -1;

			if ((s_gtaAudioFiles != null) && (sfx_bank_index >= 0) && (sfx_audio_index >= 0))
			{
				try
				{
					using (GTAAudioStream audio_stream = s_gtaAudioFiles.OpenSFXAudioStreamByName(sfx_file_name, (uint)sfx_bank_index, (uint)sfx_audio_index))
					{
						if (audio_stream != null)
						{
							// this audio stream holds all sounds from this bank

							audioStreamSize = audio_stream.Length;
							originalStreamPosition = audio_stream.Position;
							audioStreamSampleRate = audio_stream.SampleRate;

							audio_stream.Position = sfxBankAudioData.offsetInBytes;

							byte[] int_pcm = new byte[sfxBankAudioData.sizeInBytes];
							if (audio_stream.Read(int_pcm, 0, int_pcm.Length) == int_pcm.Length)
							{
								float[] float_pcm = new float[int_pcm.Length / sizeof(short)];
								for (int i = 0; i < float_pcm.Length; i++)
								{
									float_pcm[i] = Mathf.Clamp(((short)(int_pcm[i * sizeof(short)] | (int_pcm[(i * sizeof(short)) + 1] << 8)) / 32767.5f), -1.0f, 1.0f);
								}
								ret = AudioClip.Create(key, float_pcm.Length, 1, frequency, false);
								ret.SetData(float_pcm, 0);

								Debug.LogFormat("loaded sfx: {0} {1} {2}, offset {3}, size {4}, length {5}, bitrate Kb/s {6}, stream size {7}, original stream pos {8}, stream sample rate {9}, freq {10}", 
									sfx_file_name, sfx_bank_index, 
									sfx_audio_index, sfxBankAudioData.offsetInBytes, sfxBankAudioData.sizeInBytes, sfxBankAudioData.lengthSeconds,
									sfxBankAudioData.bitrateKbs, audioStreamSize, originalStreamPosition, audioStreamSampleRate, frequency);
							}
						}
					}
				}
				catch (System.Exception e)
				{
					Debug.LogError(e);
				}
			}

			if (null == ret)
			{
				Debug.LogErrorFormat("Failed to load sfx: {0} {1} {2}, offset {3}, size {4}, length {5}, bitrate Kb/s {6}, stream size {7}, original stream pos {8}, stream sample rate {9}", 
					sfx_file_name, sfx_bank_index, 
					sfx_audio_index, sfxBankAudioData.offsetInBytes, sfxBankAudioData.sizeInBytes, sfxBankAudioData.lengthSeconds,
					sfxBankAudioData.bitrateKbs, audioStreamSize, originalStreamPosition, audioStreamSampleRate);
			}

			return ret;
		}

	}

}
