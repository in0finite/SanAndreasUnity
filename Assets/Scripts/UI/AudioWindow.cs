using System.Collections.Generic;
using UnityEngine;
using UGameCore.Utilities;
using SanAndreasUnity.Behaviours.Audio;
using System.Linq;

namespace SanAndreasUnity.UI {

	public class AudioWindow : PauseMenuWindow {
		
		public float toolbarAreaHeight = 40;
		public float timingAreaHeight = 40;
		public float sideBarWidthPerc = 0.25f;

		GTAAudioSharp.GTAAudioStreamsFile m_selectedStreamsFile;
		GTAAudioSharp.GTAAudioSFXFile m_selectedSfxFile;

		public GTAAudioSharp.AGTAAudioFile SelectedAudioFile => m_selectedStreamsFile != null ? (GTAAudioSharp.AGTAAudioFile) m_selectedStreamsFile : (GTAAudioSharp.AGTAAudioFile) m_selectedSfxFile;

		string m_bankIndexStr = "0";
		string m_audioIndexStr = "0";

		Vector2 m_sideBarScrollPos = Vector2.zero;

		AudioSource m_audioSource;

		bool m_isPaused = false;
	//	GTAAudioSharp.GTAAudioStreamsFile m_playingStreamsFile;
	//	int m_playingBankIndex = -1;

		bool m_playInterval = false;
		string m_playIntervalStartStr = "00:00.000";
		string m_playIntervalEndStr = "00:00.000";



		AudioWindow() {

			// set default parameters

			this.windowName = "Audio";
			this.useScrollView = true;

		}

		void Start () {

			this.RegisterButtonInPauseMenu ();

			// adjust rect
			float minWidth = 600, maxWidth = 1000, desiredWidth = Screen.width * 0.9f ;
			float minHeight = 400, maxHeight = 700, desiredHeight = Screen.height * 0.9f;
			this.windowRect = GUIUtils.GetCenteredRect (new Vector2 (Mathf.Clamp (desiredWidth, minWidth, maxWidth), 
				Mathf.Clamp (desiredHeight, minHeight, maxHeight)));
		}


		protected override void OnWindowGUI ()
		{
			
			if (null == AudioManager.AudioFiles)
			{
				GUILayout.Label ("Audio not loaded");
				return;
			}


			// TOOLBAR AREA

			// buttons for: play, pause, stop

			int selectedButtonIndex = GUILayout.Toolbar (-1, new string[]{ "Play", "Pause", "Stop" }, GUILayout.Height(this.toolbarAreaHeight));

			if (0 == selectedButtonIndex)
			{
				// play button

				if (m_selectedStreamsFile != null)
				{
					int index;
					if (int.TryParse (m_bankIndexStr, out index))
					{
						StartPlaying (AudioManager.CreateAudioClipFromStream (m_selectedStreamsFile.Name, index));
					}
				}
				else if (m_selectedSfxFile != null)
				{
					int index;
					int audioIndex;
					if (int.TryParse (m_bankIndexStr, out index) && int.TryParse(m_audioIndexStr, out audioIndex))
					{
						StartPlaying (AudioManager.CreateAudioClipFromSfx (m_selectedSfxFile.Name, index, audioIndex));
					}

				}
			}
			else if (1 == selectedButtonIndex)
			{
				// pause button

				this.TogglePause ();
			}
			else if (2 == selectedButtonIndex)
			{
				// stop button

				this.StopPlaying (false);
			}


			// TIMINGS AREA

			GUILayout.Space (10);
			GUILayout.BeginHorizontal ();

			// elapsed time and total time of currently playing sound

			var timeSpanCurrent = System.TimeSpan.FromSeconds (this.CurrentClipTime);
			var timeSpanLength = System.TimeSpan.FromSeconds (this.CurrentClipLength);
			GUILayout.Label (string.Format ("{0:D2}:{1:D2} / {2:D2}:{3:D2}", timeSpanCurrent.Minutes, timeSpanCurrent.Seconds,
				timeSpanLength.Minutes, timeSpanLength.Seconds), GUILayout.Width(120), GUILayout.Height(this.timingAreaHeight));

			float newTime = GUILayout.HorizontalSlider (this.CurrentClipTimePerc, 0f, 1f, GUILayout.Height (this.timingAreaHeight));

			if (newTime != this.CurrentClipTimePerc)
			{
				this.CurrentClipTimePerc = newTime;
			}

			GUILayout.EndHorizontal ();

			// play interval

			GUILayout.BeginHorizontal ();
			m_playInterval = GUILayout.Toggle (m_playInterval, "Play interval", GUILayout.Height(20));
			GUILayout.FlexibleSpace ();
			if (m_playInterval)
			{
				// 2 text fields for start and end

				GUILayout.Label ("From:");
				m_playIntervalStartStr = GUILayout.TextField (m_playIntervalStartStr, 10, GUILayout.Width(120));
				GUILayout.Space (10);
				GUILayout.Label ("To:");
				m_playIntervalEndStr = GUILayout.TextField (m_playIntervalEndStr, 10, GUILayout.Width(120));

				GUILayout.FlexibleSpace ();
			}
			GUILayout.EndHorizontal ();
			GUILayout.Space (10);


			// the rest of the window will be split in 2 parts - left will be the list of all SFXs and streams, and
			// right will be the list of sounds in currently selected SFX/stream


		//	float startingY = this.toolbarAreaHeight + this.timingAreaHeight + 40f;

			GUILayout.BeginHorizontal ();
			GUILayout.Space(1);

			// SIDEBAR

		//	GUILayout.BeginArea (new Rect( 0, startingY, this.WindowSize.x * this.sideBarWidthPerc, this.WindowSize.y - startingY - 40));
			m_sideBarScrollPos = GUILayout.BeginScrollView (m_sideBarScrollPos, GUILayout.Width(this.WindowSize.x * this.sideBarWidthPerc));

			GUILayout.Label ("<b>Streams</b>");

//			Rect rect = StartHorizontal (400, 25, 2);
//			GUI.Label (rect, "Name");
//			rect = NextElement (rect);
//			GUI.Label (rect, "Num banks");

			foreach (var f in AudioManager.AudioFiles.StreamsAudioFiles)
			{
//				rect = StartHorizontal (400, 25, 2);
//
//				GUI.Label (rect, f.Name);
//
//				rect = NextElement (rect);
//				GUI.Label (rect, f.NumBanks.ToString ());

				GUI.enabled = (null == m_selectedStreamsFile || m_selectedStreamsFile.Name != f.Name);
				if (GUILayout.Button (string.Format ("{0} [{1}]", f.Name, f.NumBanks)))
				{
					// select this file
					SelectStreamFile (f);
				}
				GUI.enabled = true;

			}

			GUILayout.Space (20);

			GUILayout.Label ("<b>SFX</b>");

//			rect = StartHorizontal (400, 25, 3);
//			GUI.Label (rect, "Name");
//			rect = NextElement (rect);
//			GUI.Label (rect, "Num banks");
//			rect = NextElement (rect);
//			GUI.Label (rect, "Num audios");

			foreach (var f in AudioManager.AudioFiles.SFXAudioFiles)
			{
//				rect = StartHorizontal(400, 25, 3);
//
//				GUI.Label (rect, f.Name);
//
//				rect = NextElement (rect);
//				GUI.Label (rect, f.NumBanks.ToString ());
//
//				rect = NextElement (rect);
//				GUI.Label (rect, f.NumAudios.ToString ());

				GUI.enabled = (null == m_selectedSfxFile || m_selectedSfxFile.Name != f.Name);
				if (GUILayout.Button (string.Format ("{0} [{1}]", f.Name, f.NumBanks)))
				{
					// select this file
					SelectSfxFile (f);
				}
				GUI.enabled = true;

			}

			GUILayout.EndScrollView ();
		//	GUILayout.EndArea ();

			// LIST OF SOUNDS IN CURRENTLY SELECTED SFX/STREAM

		//	GUILayout.BeginArea (new Rect( this.WindowSize.x * this.sideBarWidthPerc, startingY, this.WindowSize.x - this.WindowSize.x * this.sideBarWidthPerc, 
		//		this.WindowSize.y - startingY - 40));

			// we can't display a list, because there is no way to enumerate all sounds
			// for now, just display text field for bank index

			GUILayout.Space (15);

			GUILayout.BeginVertical();

			if (null == this.SelectedAudioFile)
			{
				GUILayout.Label("Select stream or SFX file");
			}

			if (this.SelectedAudioFile != null)
			{
				GUILayout.Label ("bank index [0 - " + (this.SelectedAudioFile.NumBanks - 1) + "]:", GUILayout.ExpandWidth(false));
				m_bankIndexStr = GUILayout.TextField (m_bankIndexStr, 6, GUILayout.Width (120));
			}
			
			// if SFX is selected, also display text field for audio index
			if (m_selectedSfxFile != null)
			{
				GUILayout.Space(5);

				uint bankIndex;
				bool isValidBankIndex = uint.TryParse(m_bankIndexStr, out bankIndex);
				bool displayAudioIndex = isValidBankIndex && bankIndex < m_selectedSfxFile.NumBanks;

				GUILayout.Label ("audio index " + (displayAudioIndex ? "[0 - " + (m_selectedSfxFile.GetNumAudioClipsFromBank(bankIndex) - 1).ToString() + "]" : "") + ":", GUILayout.ExpandWidth(false));
				m_audioIndexStr = GUILayout.TextField (m_audioIndexStr, 6, GUILayout.Width (120));
			}

			// display info about current clip
			if (m_audioSource != null && m_audioSource.clip != null)
			{
				GUILayout.Space(15);
				GUILayout.Label("Current clip info:");

				var clip = m_audioSource.clip;
				GUILayout.Label(string.Format("length {0} sec, num samples {1}, frequency {2}, num channels {3}, name {4}", 
					clip.length, clip.samples, clip.frequency, clip.channels, clip.name));
				
			}

		//	GUILayout.EndArea ();

			GUILayout.EndVertical();
			GUILayout.Space(1);

			GUILayout.EndHorizontal ();

		}


		static Rect StartHorizontal(float width, float height, int numElements)
		{
			Rect rect = GUILayoutUtility.GetRect (width, height);
			rect.width /= numElements;
			return rect;
		}

		static Rect NextElement(Rect rect)
		{
			rect.x += rect.width;
			return rect;
		}


		public void StartPlaying(AudioClip clip)
		{
			StopPlaying ();

			if (null == m_audioSource)
			{
				var go = new GameObject ("AudioWindowSound");
				go.hideFlags = HideFlags.HideInHierarchy;
				m_audioSource = go.AddComponent<AudioSource> ();
			}

			m_audioSource.clip = clip;

			if (m_playInterval)
			{
				System.TimeSpan timeSpanStart, timeSpanEnd;
				var fp = System.Globalization.CultureInfo.InvariantCulture;
				if (System.TimeSpan.TryParseExact (m_playIntervalStartStr, "mm\\:ss\\.fff", fp, out timeSpanStart)
					&& System.TimeSpan.TryParseExact (m_playIntervalEndStr, "mm\\:ss\\.fff", fp, out timeSpanEnd)
				    && timeSpanStart.CompareTo (timeSpanEnd) < 0)
				{
					m_audioSource.time = (float) timeSpanStart.TotalSeconds;
					m_audioSource.Play ();
					m_audioSource.SetScheduledEndTime (AudioSettings.dspTime + (timeSpanEnd.TotalSeconds - timeSpanStart.TotalSeconds));
				}

			}
			else
			{
				m_audioSource.time = 0f;
				m_audioSource.Play ();
			}

		}

		public void StopPlaying(bool destroyClip = true)
		{
			if (m_audioSource)
			{
				m_audioSource.Stop ();
				if (destroyClip)
				{
					F.DestroyEvenInEditMode (m_audioSource.clip);
					m_audioSource.clip = null;
				}
			}
		}

		public void TogglePause()
		{
			if (m_audioSource)
			{
				m_isPaused = !m_isPaused;
				if (m_isPaused)
					m_audioSource.Pause ();
				else
					m_audioSource.UnPause ();
			}
		}

		public void SelectSfxFile (GTAAudioSharp.GTAAudioSFXFile f)
		{
			m_selectedSfxFile = f;
			m_selectedStreamsFile = null;
		}

		public void SelectStreamFile (GTAAudioSharp.GTAAudioStreamsFile f)
		{
			m_selectedSfxFile = null;
			m_selectedStreamsFile = f;
		}

		public float CurrentClipTimePerc {
			get {
				if (m_audioSource && m_audioSource.clip)
				{
					return m_audioSource.time / m_audioSource.clip.length;
				}
				return 0f;
			}
			set {
				value = Mathf.Clamp01 (value);
				if (m_audioSource && m_audioSource.clip)
				{
					m_audioSource.time = value * m_audioSource.clip.length;
				}
			}
		}

		public float CurrentClipTime {
			get {
				return this.CurrentClipTimePerc * this.CurrentClipLength;
			}
		}

		public float CurrentClipLength {
			get {
				if (m_audioSource && m_audioSource.clip)
				{
					return m_audioSource.clip.length;
				}
				return 0f;
			}
		}

	}

}
