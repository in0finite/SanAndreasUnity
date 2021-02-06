using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

/*
	File browser for selecting files or folders at runtime.
 */

public enum FileBrowserType {
	File,
	Directory
}

public class FileBrowser {

	// Called when the user clicks cancel or select
	public delegate void FinishedCallback(string path);
	// Defaults to working directory
	public string CurrentDirectory {
		get {
			return m_currentDirectory;
		}
		set {
			SetNewDirectory(value);
			SwitchDirectoryNow();
		}
	}
	protected string m_currentDirectory;
	// Optional pattern for filtering selectable files/folders. See:
	// http://msdn.microsoft.com/en-us/library/wz42302f(v=VS.90).aspx
	// and
	// http://msdn.microsoft.com/en-us/library/6ff71z1w(v=VS.90).aspx
	public string SelectionPattern {
		get {
			return m_filePattern;
		}
		set {
			m_filePattern = value;
			ReadDirectoryContents();
		}
	}
	protected string m_filePattern;

	protected List<(string, string)> m_topPanelEntries = new List<(string, string)>();

	// Optional image for directories
	public Texture2D DirectoryImage {
		get {
			return m_directoryImage;
		}
		set {
			m_directoryImage = value;
			BuildContent();
		}
	}
	protected Texture2D m_directoryImage;

	// Optional image for files
	public Texture2D FileImage {
		get {
			return m_fileImage;
		}
		set {
			m_fileImage = value;
			BuildContent();
		}
	}
	protected Texture2D m_fileImage;

	// Browser type. Defaults to File, but can be set to Folder
	public FileBrowserType BrowserType {
		get {
			return m_browserType;
		}
		set {
			m_browserType = value;
			ReadDirectoryContents();
		}
	}
	protected FileBrowserType m_browserType;
	protected string m_newDirectory;
	protected string[] m_currentDirectoryParts;

	protected string[] m_files;
	protected GUIContent[] m_filesWithImages;
	protected int m_selectedFile;

	protected string[] m_nonMatchingFiles;
	protected GUIContent[] m_nonMatchingFilesWithImages;
	protected int m_selectedNonMatchingDirectory;

	protected string[] m_directories;
	protected GUIContent[] m_directoriesWithImages;
	protected int m_selectedDirectory;

	protected string[] m_nonMatchingDirectories;
	protected GUIContent[] m_nonMatchingDirectoriesWithImages;

	protected bool m_currentDirectoryMatches;

	protected GUIStyle CentredText {
		get {
			if (m_centredText == null) {
				m_centredText = new GUIStyle(GUI.skin.label);
				m_centredText.alignment = TextAnchor.MiddleLeft;
				m_centredText.fixedHeight = this.ButtonStyle.fixedHeight;
			}
			return m_centredText;
		}
	}
	protected GUIStyle m_centredText;

	protected GUIStyle m_buttonStyle;
	protected GUIStyle ButtonStyle {
		get {
			if (null == m_buttonStyle) {
				// m_buttonStyle = new GUIStyle(GUI.skin.button);
				// m_buttonStyle.fixedHeight = 25;
				m_buttonStyle = GUI.skin.button;
			}
			return m_buttonStyle;
		}
	}

	protected string m_name;
	protected Rect m_screenRect;
	protected GUIStyle m_areaStyle;

	protected Vector2 m_scrollPosition;

	protected FinishedCallback m_callback;



	public FileBrowser(Rect screenRect, string name, GUIStyle areaStyle, FinishedCallback callback) {
		m_name = name;
		m_screenRect = screenRect;
		m_areaStyle = areaStyle;
		m_browserType = FileBrowserType.File;
		m_callback = callback;
		SetNewDirectory(Directory.GetCurrentDirectory());
		SwitchDirectoryNow();
	}

	protected void SetNewDirectory(string directory) {
		m_newDirectory = directory;
	}

	protected void SwitchDirectoryNow() {
		if (m_newDirectory == null || m_currentDirectory == m_newDirectory) {
			return;
		}
		m_currentDirectory = m_newDirectory;
		m_scrollPosition = Vector2.zero;
		m_selectedDirectory = m_selectedNonMatchingDirectory = m_selectedFile = -1;
		ReadDirectoryContents();
	}

	protected void ReadDirectoryContents() {

		// refresh top panel
		try {
			m_topPanelEntries.Clear ();
			m_topPanelEntries.AddRange( GetDirectoriesForTopPanel() );
		} catch {
		
		}

		if (m_currentDirectory == "/") {
			m_currentDirectoryParts = new string[] {""};
			m_currentDirectoryMatches = false;
		} else {
			m_currentDirectoryParts = m_currentDirectory.Split(Path.DirectorySeparatorChar);
			if (SelectionPattern != null) {
				string[] generation = GetDirectories(
					Path.GetDirectoryName(m_currentDirectory),
					SelectionPattern
				);
				m_currentDirectoryMatches = Array.IndexOf(generation, m_currentDirectory) >= 0;
			} else {
				m_currentDirectoryMatches = false;
			}
		}

		if (BrowserType == FileBrowserType.File || SelectionPattern == null) {
			m_directories = GetDirectories(m_currentDirectory);
			m_nonMatchingDirectories = new string[0];
		} else {
			m_directories = GetDirectories(m_currentDirectory, SelectionPattern);
			var nonMatchingDirectories = new List<string>();
			foreach (string directoryPath in GetDirectories(m_currentDirectory)) {
				if (Array.IndexOf(m_directories, directoryPath) < 0) {
					nonMatchingDirectories.Add(directoryPath);
				}
			}
			m_nonMatchingDirectories = nonMatchingDirectories.ToArray();
			for (int i = 0; i < m_nonMatchingDirectories.Length; ++i) {
				int lastSeparator = m_nonMatchingDirectories[i].LastIndexOf(Path.DirectorySeparatorChar);
				m_nonMatchingDirectories[i] = m_nonMatchingDirectories[i].Substring(lastSeparator + 1);
			}
			Array.Sort(m_nonMatchingDirectories);
		}

		for (int i = 0; i < m_directories.Length; ++i) {
			m_directories[i] = m_directories[i].Substring(m_directories[i].LastIndexOf(Path.DirectorySeparatorChar) + 1);
		}

		if (BrowserType == FileBrowserType.Directory || SelectionPattern == null) {
			m_files = GetFiles(m_currentDirectory);
			m_nonMatchingFiles = new string[0];
		} else {
			m_files = GetFiles(m_currentDirectory, SelectionPattern);
			var nonMatchingFiles = new List<string>();
			foreach (string filePath in GetFiles(m_currentDirectory)) {
				if (Array.IndexOf(m_files, filePath) < 0) {
					nonMatchingFiles.Add(filePath);
				}
			}
			m_nonMatchingFiles = nonMatchingFiles.ToArray();
			for (int i = 0; i < m_nonMatchingFiles.Length; ++i) {
				m_nonMatchingFiles[i] = Path.GetFileName(m_nonMatchingFiles[i]);
			}
			Array.Sort(m_nonMatchingFiles);
		}

		for (int i = 0; i < m_files.Length; ++i) {
			m_files[i] = Path.GetFileName(m_files[i]);
		}

		Array.Sort(m_files);

		BuildContent();

		m_newDirectory = null;
	}

	static string[] GetFiles (string path, string searchPattern)
	{
		try {
			return Directory.GetFiles( path, searchPattern );
		} catch {
			return new string[0];
		}
	}

	static string[] GetFiles (string path)
	{
		try {
			return Directory.GetFiles( path );
		} catch {
			return new string[0];
		}
	}

	static string[] GetDirectories (string path, string searchPattern)
	{
		try {
			return Directory.GetDirectories( path, searchPattern );
		} catch {
			return new string[0];
		}
	}

	static string[] GetDirectories (string path)
	{
		try {
			return Directory.GetDirectories( path );
		} catch {
			return new string[0];
		}
	}

	static List<(string, string)> GetDirectoriesForTopPanel()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			return new string[]{"/", "/sdcard/", "/storage/", "/storage/sdcard0/", "/storage/sdcard1/", "/storage/emulated/0/"}
				.Select(s => (s, s))
				.ToList();
		}
		else if (Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.LinuxEditor)
		{
			return new string[]{"/", "/home/", "/mnt/", "/media/"}
				.Select(s => (s, s))
				.ToList();
		}
		else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
		{
			var result = new List<(string, string)>();

			result.AddRange(Directory.GetLogicalDrives().Select(s => (s, s)));

			var specialFolders = new[]
			{
				Environment.SpecialFolder.UserProfile,
				Environment.SpecialFolder.DesktopDirectory,
				Environment.SpecialFolder.MyDocuments,
				Environment.SpecialFolder.ProgramFiles,
				Environment.SpecialFolder.ProgramFilesX86,
			};

			string[] specialFolderNames = new[]
			{
				"User",
				"Desktop",
				"Documents",
				"Program Files",
				"Program Files (x86)",
			};

			for (int i = 0; i < specialFolders.Length; i++)
			{
				string path = string.Empty;
				try
				{
					path = Environment.GetFolderPath(specialFolders[i]);
				}
				catch
				{
				}

				if (!string.IsNullOrWhiteSpace(path))
					result.Add((specialFolderNames[i], path));
			}

			return result;
		}
		else
		{
			return Directory.GetLogicalDrives()
				.Select(s => (s, s))
				.ToList();
		}
	}


	protected void BuildContent() {
		m_directoriesWithImages = new GUIContent[m_directories.Length];
		for (int i = 0; i < m_directoriesWithImages.Length; ++i) {
			m_directoriesWithImages[i] = new GUIContent(m_directories[i], DirectoryImage);
		}
		m_nonMatchingDirectoriesWithImages = new GUIContent[m_nonMatchingDirectories.Length];
		for (int i = 0; i < m_nonMatchingDirectoriesWithImages.Length; ++i) {
			m_nonMatchingDirectoriesWithImages[i] = new GUIContent(m_nonMatchingDirectories[i], DirectoryImage);
		}
		m_filesWithImages = new GUIContent[m_files.Length];
		for (int i = 0; i < m_filesWithImages.Length; ++i) {
			m_filesWithImages[i] = new GUIContent(m_files[i], FileImage);
		}
		m_nonMatchingFilesWithImages = new GUIContent[m_nonMatchingFiles.Length];
		for (int i = 0; i < m_nonMatchingFilesWithImages.Length; ++i) {
			m_nonMatchingFilesWithImages[i] = new GUIContent(m_nonMatchingFiles[i], FileImage);
		}
	}

	public void OnGUI() {

		if (m_areaStyle != null)
			GUILayout.BeginArea(m_screenRect, m_name, m_areaStyle);
		else
			GUILayout.BeginArea(m_screenRect, m_name);

		// display top panel
		if (m_topPanelEntries.Count > 0) {
			GUILayout.BeginHorizontal();

			foreach (var entry in m_topPanelEntries) {
				if (GUILayout.Button (entry.Item1, this.ButtonStyle)) {
					SetNewDirectory (entry.Item2);
				}
			}

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal ();
		}

		// display directory parts
		GUILayout.BeginHorizontal();

		for (int parentIndex = 0; parentIndex < m_currentDirectoryParts.Length; ++parentIndex) {
			if (parentIndex == m_currentDirectoryParts.Length - 1) {
				GUILayout.Label(m_currentDirectoryParts[parentIndex], CentredText);
			} else if (GUILayout.Button(m_currentDirectoryParts[parentIndex], this.ButtonStyle)) {
				string parentDirectoryName = m_currentDirectory;
				for (int i = m_currentDirectoryParts.Length - 1; i > parentIndex; --i) {
					parentDirectoryName = Path.GetDirectoryName(parentDirectoryName);
				}
				SetNewDirectory(parentDirectoryName);
			}
		}

		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		m_scrollPosition = GUILayout.BeginScrollView(
			m_scrollPosition,
			false,
			true,
			GUI.skin.horizontalScrollbar,
			GUI.skin.verticalScrollbar,
			GUI.skin.box
		);
		m_selectedDirectory = xGUILayout.SelectionList(
			m_selectedDirectory,
			m_directoriesWithImages,
			this.ButtonStyle,
			DirectoryDoubleClickCallback
		);
		if (m_selectedDirectory > -1) {
			m_selectedFile = m_selectedNonMatchingDirectory = -1;
		}
		m_selectedNonMatchingDirectory = xGUILayout.SelectionList(
			m_selectedNonMatchingDirectory,
			m_nonMatchingDirectoriesWithImages,
			this.ButtonStyle,
			NonMatchingDirectoryDoubleClickCallback
		);
		if (m_selectedNonMatchingDirectory > -1) {
			m_selectedDirectory = m_selectedFile = -1;
		}
		GUI.enabled = BrowserType == FileBrowserType.File;
		m_selectedFile = xGUILayout.SelectionList(
			m_selectedFile,
			m_filesWithImages,
			this.ButtonStyle,
			FileDoubleClickCallback
		);
		GUI.enabled = true;
		if (m_selectedFile > -1) {
			m_selectedDirectory = m_selectedNonMatchingDirectory = -1;
		}
		GUI.enabled = false;
		xGUILayout.SelectionList(
			-1,
			m_nonMatchingFilesWithImages,
			this.ButtonStyle
		);
		GUI.enabled = true;
		GUILayout.EndScrollView();

		GUILayout.Space(5);

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		if (SanAndreasUnity.Utilities.GUIUtils.ButtonWithCalculatedSize("Cancel")) {
			m_callback(null);
		}

		if (BrowserType == FileBrowserType.File) {
			GUI.enabled = m_selectedFile > -1;
		} else {
			if (SelectionPattern == null) {
				GUI.enabled = true;//m_selectedDirectory > -1;
			} else {
				GUI.enabled =	m_selectedDirectory > -1 ||
					(
						m_currentDirectoryMatches &&
						m_selectedNonMatchingDirectory == -1 &&
						m_selectedFile == -1
					);
			}
		}

		string selectButtonText = BrowserType == FileBrowserType.File ? "Select" : "Select current folder";

		if (SanAndreasUnity.Utilities.GUIUtils.ButtonWithCalculatedSize (selectButtonText)) {
			if (BrowserType == FileBrowserType.File) {
				m_callback(Path.Combine(m_currentDirectory, m_files[m_selectedFile]));
			} else {
//				if (m_selectedDirectory > -1) {
//					m_callback(Path.Combine(m_currentDirectory, m_directories[m_selectedDirectory]));
//				} else {
//					m_callback(m_currentDirectory);
//				}
				m_callback(m_currentDirectory);
			}
		}

		GUI.enabled = true;
		GUILayout.EndHorizontal();
		GUILayout.EndArea();

		if (Event.current.type == EventType.Repaint) {
			SwitchDirectoryNow();
		}
	}

	protected void FileDoubleClickCallback(int i) {
		if (BrowserType == FileBrowserType.File) {
			m_callback(Path.Combine(m_currentDirectory, m_files[i]));
		}
	}

	protected void DirectoryDoubleClickCallback(int i) {
		SetNewDirectory(Path.Combine(m_currentDirectory, m_directories[i]));
	}

	protected void NonMatchingDirectoryDoubleClickCallback(int i) {
		SetNewDirectory(Path.Combine(m_currentDirectory, m_nonMatchingDirectories[i]));
	}

	public static Vector2 GetRecommendedSize()
	{
		float width = Mathf.Max (Screen.width * 0.75f, 600);
		float height = width * 9f / 16f;
		return new Vector2(width, height);
	}

}