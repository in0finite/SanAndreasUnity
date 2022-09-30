using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SanAndreasUnity.UI
{

	public class Console : PauseMenuWindow
	{
		
		#region Inspector Settings

		/// <summary>
		/// Whether to only keep a certain number of logs, useful if memory usage is a concern.
		/// </summary>
		public bool restrictLogCount = true;

		/// <summary>
		/// Number of logs to keep before removing old ones.
		/// </summary>
		public int maxLogCount = 1000;

		#endregion

		static readonly GUIContent s_clearLabel = new GUIContent("Clear", "Clear the contents of the console.");
		static readonly GUIContent s_collapseLabel = new GUIContent("Collapse", "Hide repeated messages.");

		private static readonly LogType[] s_allLogTypes = new LogType[] {
			LogType.Log, LogType.Warning, LogType.Error, LogType.Exception, LogType.Assert
		};

		static readonly Dictionary<LogType, Color> s_logTypeColors = new Dictionary<LogType, Color>
		{
			{ LogType.Assert, Color.red },
			{ LogType.Error, Color.red },
			{ LogType.Exception, Color.red },
			{ LogType.Log, Color.white },
			{ LogType.Warning, Color.yellow },
		};

		bool m_isCollapsed;
		public bool IsCollapsed { get { return this.m_isCollapsed; } set { this.m_isCollapsed = value; } }

		bool ShowDetails { get; set; }
		Vector2 m_detailsAreaScrollViewPos = Vector2.zero;

		int m_selectedLogIndex = -1;

		readonly List<Log> m_logs = new List<Log>();
		readonly UGameCore.Utilities.ConcurrentQueue<Log> m_queuedLogs = new UGameCore.Utilities.ConcurrentQueue<Log>();

		readonly Dictionary<LogType, bool> m_logTypeFilters = new Dictionary<LogType, bool>
		{
			{ LogType.Assert, true },
			{ LogType.Error, true },
			{ LogType.Exception, true },
			{ LogType.Log, true },
			{ LogType.Warning, true },
		};

		private Dictionary<LogType, int> m_numMessagesPerType = new Dictionary<LogType, int> ();



		Console ()
		{
			this.windowName = "Console";
			this.useScrollView = false;
		}

		internal static void OnEventSubscriberAwake()
		{
			var console = FindObjectOfType<Console>();

			if (null == console)
			{
				Debug.LogError("Failed to find console object");
				return;
			}

			console.OnEventSubscriberAwakeNonStatic();
		}

		void OnEventSubscriberAwakeNonStatic()
		{
			Application.logMessageReceivedThreaded += HandleLogThreaded;
		}


		#region MonoBehaviour Messages

		protected override void OnDisable()
		{
			Application.logMessageReceivedThreaded -= HandleLogThreaded;
			base.OnDisable();
		}

		void Start ()
		{

			this.RegisterButtonInPauseMenu ();

			// adjust rect
			this.windowRect = UGameCore.Utilities.GUIUtils.GetCenteredRectPerc( new Vector2(0.9f, 0.8f) );
		}

		void Update()
		{
			UpdateQueuedLogs();

		}

		#endregion

		static void RestoreContentColor ()
		{
			GUI.contentColor = Color.white;
		}

		void DrawCollapsedLog(Log log, int index)
		{
			bool isSelected = index == m_selectedLogIndex;

			if (isSelected)
				GUI.contentColor = Color.green;

			GUILayout.BeginHorizontal();

			if (GUILayout.Button(log.GetTruncatedMessage(), GUI.skin.label))
			{
				if(isSelected)
					m_selectedLogIndex = -1;
				else
					m_selectedLogIndex = index;
			}
			GUILayout.FlexibleSpace();
			GUILayout.Label(log.count.ToString(), GUI.skin.box);

			GUILayout.EndHorizontal();

			if (isSelected)
				RestoreContentColor();
		}

		void DrawExpandedLog(Log log, int index)
		{
			bool isSelected = index == m_selectedLogIndex;

			if (isSelected)
				GUI.contentColor = Color.green;

			for (var i = 0; i < log.count; i += 1)
			{
				if (GUILayout.Button(log.GetTruncatedMessage(), GUI.skin.label))
				{
					if(isSelected)
						m_selectedLogIndex = -1;
					else
						m_selectedLogIndex = index;
				}
			}

			if (isSelected)
				RestoreContentColor();
		}

		void DrawLog(Log log, int index)
		{
			GUI.contentColor = s_logTypeColors[log.type];

			if (m_isCollapsed)
			{
				DrawCollapsedLog(log, index);
			}
			else
			{
				DrawExpandedLog(log, index);
			}
		}

		void DrawLogList()
		{
			scrollPos = GUILayout.BeginScrollView(scrollPos);

			// Used to determine height of accumulated log labels.
			GUILayout.BeginVertical();

			foreach (LogType logType in s_allLogTypes)
				m_numMessagesPerType [logType] = 0;

			for (int i=0; i < m_logs.Count; i++)
			{
				Log log = m_logs[i];

				if (IsLogVisible (log))
					DrawLog (log, i);

				m_numMessagesPerType [log.type] += (m_isCollapsed ? 1 : log.count);
			}

			GUILayout.EndVertical();
			var innerScrollRect = GUILayoutUtility.GetLastRect();
			GUILayout.EndScrollView();
			var outerScrollRect = GUILayoutUtility.GetLastRect();

			// If we're scrolled to bottom now, guarantee that it continues to be in next cycle.
			if (Event.current.type == EventType.Repaint && IsScrolledToBottom(innerScrollRect, outerScrollRect))
			{
				ScrollToBottom();
			}

			// Ensure GUI colour is reset before drawing other components.
			GUI.contentColor = Color.white;
		}

		void DrawDetails()
		{

			float height = Mathf.Max( this.windowRect.height / 4, 100 );
			m_detailsAreaScrollViewPos = GUILayout.BeginScrollView(m_detailsAreaScrollViewPos, GUILayout.Height(height));

			if (m_selectedLogIndex >= 0 && m_selectedLogIndex < this.m_logs.Count)
			{
				Log log = this.m_logs[m_selectedLogIndex];
				GUILayout.Label(log.message);
				GUILayout.Space(5);
				GUILayout.Label(log.stackTrace);
			}

			GUILayout.EndScrollView();

		}

		void DrawToolbar()
		{
			GUILayout.BeginHorizontal();

			foreach (LogType logType in s_allLogTypes)
			{
				bool currentState = m_logTypeFilters[logType];
				int count = m_numMessagesPerType [logType];
				string label = logType.ToString() + ( (count > 0) ? (" [" + count + "]") : "" );

				GUI.contentColor = s_logTypeColors [logType];
				m_logTypeFilters[logType] = GUILayout.Toggle (currentState, label, GUILayout.ExpandWidth(false));
				GUILayout.Space(15);
			}

			GUILayout.EndHorizontal();

			RestoreContentColor ();

			GUILayout.BeginHorizontal ();

			if (UGameCore.Utilities.GUIUtils.ButtonWithCalculatedSize (s_clearLabel))
			{
				m_logs.Clear();
				m_selectedLogIndex = -1;
			}

			GUILayout.Space (10);

			m_isCollapsed = GUILayout.Toggle (m_isCollapsed, s_collapseLabel, GUILayout.ExpandWidth(false));

			ShowDetails = GUILayout.Toggle (ShowDetails, "Show details", GUILayout.ExpandWidth(false));

			GUILayout.Space (10);

			GUILayout.Label("Cap: " + m_logs.Capacity);
			GUILayout.Label("MT queue count: " + m_queuedLogs.Count);

			GUILayout.EndHorizontal ();
		}

		void DrawWindow()
		{
			DrawLogList();
			if (this.ShowDetails)
			{
				UGameCore.Utilities.GUIUtils.DrawHorizontalLine(2f, 3f, Color.black);
				this.DrawDetails();
			}
			UGameCore.Utilities.GUIUtils.DrawHorizontalLine(2f, 3f, Color.black);
			DrawToolbar();
		}

		protected override void OnWindowGUI ()
		{
			DrawWindow ();
		}


		Log? GetLastLog()
		{
			if (m_logs.Count == 0)
			{
				return null;
			}

			return m_logs[m_logs.Count - 1];
		}

		void UpdateQueuedLogs()
		{
			foreach (Log log in m_queuedLogs.DequeueAll())
			{
				ProcessLogItem(log);
			}
		}

		void HandleLogThreaded(string message, string stackTrace, LogType type)
		{
			var log = new Log
			{
				count = 1,
				message = message,
				stackTrace = stackTrace,
				type = type,
			};

			// Queue the log into a ConcurrentQueue to be processed later in the Unity main thread,
			// so that we don't get GUI-related errors for logs coming from other threads
			m_queuedLogs.Enqueue(log);
		}

		void ProcessLogItem(Log log)
		{
			var lastLog = GetLastLog();
			var isDuplicateOfLastLog = lastLog.HasValue && log.Equals(lastLog.Value);

			if (isDuplicateOfLastLog)
			{
				// Replace previous log with incremented count instead of adding a new one.
				log.count = lastLog.Value.count + 1;
				m_logs[m_logs.Count - 1] = log;
			}
			else
			{
				m_logs.Add(log);
				TrimExcessLogs();
			}
		}

		bool IsLogVisible(Log log)
		{
			return m_logTypeFilters[log.type];
		}

		bool IsScrolledToBottom(Rect innerScrollRect, Rect outerScrollRect)
		{
			var innerScrollHeight = innerScrollRect.height;

			// Take into account extra padding added to the scroll container.
			var outerScrollHeight = outerScrollRect.height - GUI.skin.box.padding.vertical;

			// If contents of scroll view haven't exceeded outer container, treat it as scrolled to bottom.
			if (outerScrollHeight > innerScrollHeight)
			{
				return true;
			}

			// Scrolled to bottom (with error margin for float math)
			return Mathf.Approximately(innerScrollHeight, scrollPos.y + outerScrollHeight);
		}

		void ScrollToBottom()
		{
			scrollPos = new Vector2(0, Int32.MaxValue);
		}

		void TrimExcessLogs()
		{
			if (!this.restrictLogCount)
			{
				return;
			}

			int amountToRemove = m_logs.Count - this.maxLogCount;

			if (amountToRemove <= 0)
			{
				return;
			}

			m_logs.RemoveRange(0, amountToRemove);
		}
	}

	/// <summary>
	/// A basic container for log details.
	/// </summary>
	struct Log
	{
		public int count;
		public string message;
		public string stackTrace;
		public LogType type;

		/// <summary>
		/// The max string length supported by UnityEngine.GUILayout.Label without triggering this error:
		/// "String too long for TextMeshGenerator. Cutting off characters."
		/// </summary>
		private const int MaxMessageLength = 16382;

		public bool Equals(Log log)
		{
			return message == log.message && stackTrace == log.stackTrace && type == log.type;
		}

		/// <summary>
		/// Return a truncated message if it exceeds the max message length
		/// </summary>
		public string GetTruncatedMessage()
		{
			if (string.IsNullOrEmpty(message)) return message;
			return message.Length <= MaxMessageLength ? message : message.Substring(0, MaxMessageLength);
		}
	}


}