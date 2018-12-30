using System.Collections.Generic;
using UnityEngine;
using GTAAudioSharp;
using System.IO;
using SanAndreasUnity.Audio;

namespace SanAndreasUnity.Behaviours.Audio
{
	
	public class AudioManager : MonoBehaviour
	{

		private static GTAAudioFiles s_gtaAudioFiles;
		public static GTAAudioFiles AudioFiles { get { return s_gtaAudioFiles; } }

	//	private Dictionary<string, AudioClip> sfxAudioClips = new Dictionary<string, AudioClip>();

	//	private Dictionary<string, AudioClip> streamsAudioClips = new Dictionary<string, AudioClip>();

	//	private Dictionary<string, AudioStream> streamsAudioStreams = new Dictionary<string, AudioStream>();




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

		void Update () {
			
		}


		public static void InitFromLoader (string gameDir)
		{

			s_gtaAudioFiles = GTAAudio.OpenRead (Path.Combine (gameDir, "audio"));

		//	PlayStreamSound ("Beats", 1);

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
//				AudioSource audioSource = new GameObject (key).AddComponent<AudioSource> ();
//				audioSource.time = 0.0f;
//				audioSource.clip = audio_stream.AudioClip;
//				audioSource.Play();
//
//				// destroy game object when sound is finished playing
//				Destroy( audioSource.gameObject, audio_stream.AudioClip.length );

				System.TimeSpan time_span = System.DateTime.Now - startTime;
				Debug.Log("\"" + key + "\" took " + time_span.TotalSeconds + " seconds.");

				return audio_stream.AudioClip;
			}

			return null;
		}

	}

}
