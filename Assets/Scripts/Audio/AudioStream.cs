using NVorbis;
using System;
using System.IO;
using UnityEngine;

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
			reader = new VorbisReader(stream, closeStreamOnDispose);
			audioClip = AudioClip.Create(audioClipName, (int)(reader.TotalSamples), reader.Channels, reader.SampleRate, true, (data) =>
				{
					if (data != null)
					{
						if (data.Length > 0)
						{
							reader.ReadSamples(data, 0, Mathf.Min(data.Length, Mathf.Max(0, (int)(reader.TotalSamples - reader.DecodedPosition))));
						}
					}
				}, (newPosition) =>
				{
					reader.DecodedPosition = newPosition;
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
