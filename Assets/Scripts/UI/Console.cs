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

		static readonly GUIContent clearLabel = new GUIContent("Clear", "Clear the contents of the console.");
		static readonly GUIContent collapseLabel = new GUIContent("Collapse", "Hide repeated messages.");
	//	const int margin = 20;

		static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>
		{
			{ LogType.Assert, Color.white },
			{ LogType.Error, Color.red },
			{ LogType.Exception, Color.red },
			{ LogType.Log, Color.white },
			{ LogType.Warning, Color.yellow },
		};

		bool isCollapsed;
		public bool IsCollapsed { get { return this.isCollapsed; } set { this.isCollapsed = value; } }
	//	bool isVisible;
		readonly List<Log> logs = new List<Log>();
		readonly ConcurrentQueue<Log> queuedLogs = new ConcurrentQueue<Log>();

	//	Vector2 scrollPos;
	//	Rect windowRect = new Rect(margin, margin, Screen.width - (margin * 2), Screen.height - (margin * 2));

		readonly Dictionary<LogType, bool> logTypeFilters = new Dictionary<LogType, bool>
		{
			{ LogType.Assert, true },
			{ LogType.Error, true },
			{ LogType.Exception, true },
			{ LogType.Log, true },
			{ LogType.Warning, true },
		};



		Console ()
		{
			this.windowName = "Console";
			this.useScrollView = false;
		}


		#region MonoBehaviour Messages

		void OnDisable()
		{
			Application.logMessageReceivedThreaded -= HandleLogThreaded;
		}

		void OnEnable()
		{
			Application.logMessageReceivedThreaded += HandleLogThreaded;
		}

		void Start ()
		{

			this.RegisterButtonInPauseMenu ();

			// adjust rect
			this.windowRect = Utilities.GUIUtils.GetCenteredRect( new Vector2(500, 400) );
		}

		void Update()
		{
			UpdateQueuedLogs();

		}

		#endregion

		void DrawCollapsedLog(Log log)
		{
			GUILayout.BeginHorizontal();

			GUILayout.Label(log.GetTruncatedMessage());
			GUILayout.FlexibleSpace();
			GUILayout.Label(log.count.ToString(), GUI.skin.box);

			GUILayout.EndHorizontal();
		}

		void DrawExpandedLog(Log log)
		{
			for (var i = 0; i < log.count; i += 1)
			{
				GUILayout.Label(log.GetTruncatedMessage());
			}
		}

		void DrawLog(Log log)
		{
			GUI.contentColor = logTypeColors[log.type];

			if (isCollapsed)
			{
				DrawCollapsedLog(log);
			}
			else
			{
				DrawExpandedLog(log);
			}
		}

		void DrawLogList()
		{
			scrollPos = GUILayout.BeginScrollView(scrollPos);

			// Used to determine height of accumulated log labels.
			GUILayout.BeginVertical();

			var visibleLogs = logs.Where(IsLogVisible);

			foreach (Log log in visibleLogs)
			{
				DrawLog(log);
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

		void DrawToolbar()
		{
			GUILayout.BeginHorizontal();

			if (GUILayout.Button(clearLabel))
			{
				logs.Clear();
			}

			foreach (LogType logType in Enum.GetValues(typeof(LogType)))
			{
				var currentState = logTypeFilters[logType];
				var label = logType.ToString();
				logTypeFilters[logType] = GUILayout.Toggle(currentState, label, GUILayout.ExpandWidth(false));
				GUILayout.Space(20);
			}

			isCollapsed = GUILayout.Toggle(isCollapsed, collapseLabel, GUILayout.ExpandWidth(false));

			GUILayout.EndHorizontal();
		}

		void DrawWindow()
		{
			DrawLogList();
			DrawToolbar();

		}

		protected override void OnWindowGUI ()
		{
			DrawWindow ();
		}


		Log? GetLastLog()
		{
			if (logs.Count == 0)
			{
				return null;
			}

			return logs.Last();
		}

		void UpdateQueuedLogs()
		{
			Log log;
			while (queuedLogs.TryDequeue(out log))
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
			queuedLogs.Enqueue(log);
		}

		void ProcessLogItem(Log log)
		{
			var lastLog = GetLastLog();
			var isDuplicateOfLastLog = lastLog.HasValue && log.Equals(lastLog.Value);

			if (isDuplicateOfLastLog)
			{
				// Replace previous log with incremented count instead of adding a new one.
				log.count = lastLog.Value.count + 1;
				logs[logs.Count - 1] = log;
			}
			else
			{
				logs.Add(log);
				TrimExcessLogs();
			}
		}

		bool IsLogVisible(Log log)
		{
			return logTypeFilters[log.type];
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
			if (!restrictLogCount)
			{
				return;
			}

			var amountToRemove = logs.Count - maxLogCount;

			if (amountToRemove <= 0)
			{
				return;
			}

			logs.RemoveRange(0, amountToRemove);
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

	/// <summary>
	/// Alternative to System.Collections.Concurrent.ConcurrentQueue
	/// (It's only available in .NET 4.0 and greater)
	/// </summary>
	/// <remarks>
	/// It's a bit slow (as it uses locks), and only provides a small subset of the interface
	/// Overall, the implementation is intended to be simple & robust
	/// </remarks>
	public class ConcurrentQueue<T>
	{
		private readonly System.Object queueLock = new System.Object();
		private readonly Queue<T> queue = new Queue<T>();

		public void Enqueue(T item)
		{
			lock (queueLock)
			{
				queue.Enqueue(item);
			}
		}

		public bool TryDequeue(out T result)
		{
			lock (queueLock)
			{
				if (queue.Count == 0)
				{
					result = default(T);
					return false;
				}

				result = queue.Dequeue();
				return true;
			}
		}
	}
}