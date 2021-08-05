using NVorbis;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;

namespace SanAndreasUnity.Audio
{
	
	/// <summary>
	/// Audio stream class
	/// </summary>
	public class AudioStream : IDisposable
	{
		/// <summary>
		/// Reader
		/// </summary>
		private VorbisReader reader;

		/// <summary>
		/// Audio clip
		/// </summary>
		private AudioClip audioClip;

		/// <summary>
		/// Audio clip
		/// </summary>
		public AudioClip AudioClip
		{
			get
			{
				return audioClip;
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="stream">Stream</param>
		/// <param name="audioClipName">Audio clip name</param>
		/// <param name="closeStreamOnDispose">Close stream on dispose</param>
		public AudioStream(Stream stream, string audioClipName, bool closeStreamOnDispose)
		{
			Profiler.BeginSample("VorbisReader()");
			reader = new VorbisReader(stream, closeStreamOnDispose);
			Profiler.EndSample();
			audioClip = AudioClip.Create(audioClipName, (int)(reader.TotalSamples), reader.Channels, reader.SampleRate, true, (data) =>
				{
					Profiler.BeginSample("reader.ReadSamples()");
					if (data != null)
					{
						if (data.Length > 0)
						{
							reader.ReadSamples(data, 0, Mathf.Min(data.Length, Mathf.Max(0, (int)(reader.TotalSamples - reader.DecodedPosition))));
						}
					}
					Profiler.EndSample();
				}, (newPosition) =>
				{
					Profiler.BeginSample("set DecodedPosition");
					reader.DecodedPosition = newPosition;
					Profiler.EndSample();
				});
		}

		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			if (reader != null)
			{
				reader.Dispose();
				reader = null;
			}
		}
	}

}
