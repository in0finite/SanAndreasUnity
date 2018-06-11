using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

/*
	File browser for selecting files or folders at runtime.
 */

public enum FileBrowserType
{
    File,
    Directory
}

public class FileBrowser
{
    // Called when the user clicks cancel or select
    public delegate void FinishedCallback(string path);

    // Defaults to working directory
    public string CurrentDirectory
    {
        get
        {
            return m_currentDirectory;
        }
        set
        {
            SetNewDirectory(value);
            SwitchDirectoryNow();
        }
    }

    protected string m_currentDirectory;

    // Optional pattern for filtering selectable files/folders. See:
    // http://msdn.microsoft.com/en-us/library/wz42302f(v=VS.90).aspx
    // and
    // http://msdn.microsoft.com/en-us/library/6ff71z1w(v=VS.90).aspx
    public string SelectionPattern
    {
        get
        {
            return m_filePattern;
        }
        set
        {
            m_filePattern = value;
            ReadDirectoryContents();
        }
    }

    protected string m_filePattern;

    // Optional image for directories
    public Texture2D DirectoryImage
    {
        get
        {
            return m_directoryImage;
        }
        set
        {
            m_directoryImage = value;
            BuildContent();
        }
    }

    protected Texture2D m_directoryImage;

    // Optional image for files
    public Texture2D FileImage
    {
        get
        {
            return m_fileImage;
        }
        set
        {
            m_fileImage = value;
            BuildContent();
        }
    }

    protected Texture2D m_fileImage;

    // Browser type. Defaults to File, but can be set to Folder
    public FileBrowserType BrowserType
    {
        get
        {
            return m_browserType;
        }
        set
        {
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

    protected GUIStyle CentredText
    {
        get
        {
            if (m_centredText == null)
            {
                m_centredText = new GUIStyle(GUI.skin.label);
                m_centredText.alignment = TextAnchor.MiddleLeft;
                m_centredText.fixedHeight = GUI.skin.button.fixedHeight;
            }
            return m_centredText;
        }
    }

    protected GUIStyle m_centredText;

    protected string m_name;
    protected Rect m_screenRect;

    protected Vector2 m_scrollPosition;

    protected FinishedCallback m_callback;
    protected string m_path;
    protected bool m_toggle;

    protected GUISkin m_skin;

    private FileBrowser()
    {
    }

    // Browsers need at least a rect, name and callback
    public FileBrowser(Rect screenRect, string name, FinishedCallback callback)
    {
        m_name = name;
        m_screenRect = screenRect;
        m_browserType = FileBrowserType.File;
        m_callback = (s) =>
        {
            if (callback != null) callback(s);
            m_path = s;
        };

        DirectoryImage = m_directoryImage ?? Resources.Load<Texture2D>("Icons/folder"); //No estoy seguro de si esto funcionará
        FileImage = m_fileImage ?? Resources.Load<Texture2D>("Icons/file");

        m_skin = Resources.Load<GUISkin>("Skins/File Browser");

        SetNewDirectory(Directory.GetCurrentDirectory());
        SwitchDirectoryNow();
    }

    public string GetPath()
    {
        while (string.IsNullOrEmpty(m_path))
        { }

        return m_path;
    }

    public void Toggle()
    {
        m_toggle = !m_toggle;
    }

    protected void SetNewDirectory(string directory)
    {
        m_newDirectory = directory;
    }

    protected void SwitchDirectoryNow()
    {
        if (m_newDirectory == null || m_currentDirectory == m_newDirectory)
        {
            return;
        }
        m_currentDirectory = m_newDirectory;
        m_scrollPosition = Vector2.zero;
        m_selectedDirectory = m_selectedNonMatchingDirectory = m_selectedFile = -1;
        ReadDirectoryContents();
    }

    protected void ReadDirectoryContents()
    {
        if (m_currentDirectory == "/")
        {
            //... some code, not importent here
            if (SelectionPattern != null)
            {
                string directoryName = Path.GetDirectoryName(m_currentDirectory);
                string[] generation = new string[0];
                if (directoryName != null)
                {   //This is new: generation should be an empty array for the root directory.
                    //directoryName will be null if it's a root directory
                    generation = Directory.GetDirectories(
                    directoryName,
                    SelectionPattern);
                }
                m_currentDirectoryMatches = Array.IndexOf(generation, m_currentDirectory) >= 0;
            }
            else
            {
                m_currentDirectoryParts = m_currentDirectory.Split(Path.DirectorySeparatorChar);
                if (SelectionPattern != null)
                {
                    string[] generation = Directory.GetDirectories(
                        Path.GetDirectoryName(m_currentDirectory),
                        SelectionPattern
                    );
                    m_currentDirectoryMatches = Array.IndexOf(generation, m_currentDirectory) >= 0;
                }
                else
                {
                    m_currentDirectoryMatches = false;
                }
            }

            if (BrowserType == FileBrowserType.File || SelectionPattern == null)
            {
                m_directories = Directory.GetDirectories(m_currentDirectory);
                m_nonMatchingDirectories = new string[0];
            }
            else
            {
                m_directories = Directory.GetDirectories(m_currentDirectory, SelectionPattern);
                var nonMatchingDirectories = new List<string>();
                foreach (string directoryPath in Directory.GetDirectories(m_currentDirectory))
                {
                    if (Array.IndexOf(m_directories, directoryPath) < 0)
                    {
                        nonMatchingDirectories.Add(directoryPath);
                    }
                }
                m_nonMatchingDirectories = nonMatchingDirectories.ToArray();
                for (int i = 0; i < m_nonMatchingDirectories.Length; ++i)
                {
                    int lastSeparator = m_nonMatchingDirectories[i].LastIndexOf(Path.DirectorySeparatorChar);
                    m_nonMatchingDirectories[i] = m_nonMatchingDirectories[i].Substring(lastSeparator + 1);
                }
                Array.Sort(m_nonMatchingDirectories);
            }

            for (int i = 0; i < m_directories.Length; ++i)
            {
                m_directories[i] = m_directories[i].Substring(m_directories[i].LastIndexOf(Path.DirectorySeparatorChar) + 1);
            }

            if (BrowserType == FileBrowserType.Directory || SelectionPattern == null)
            {
                m_files = Directory.GetFiles(m_currentDirectory);
                m_nonMatchingFiles = new string[0];
            }
            else
            {
                m_files = Directory.GetFiles(m_currentDirectory, SelectionPattern);
                var nonMatchingFiles = new List<string>();
                foreach (string filePath in Directory.GetFiles(m_currentDirectory))
                {
                    if (Array.IndexOf(m_files, filePath) < 0)
                    {
                        nonMatchingFiles.Add(filePath);
                    }
                }
                m_nonMatchingFiles = nonMatchingFiles.ToArray();
                for (int i = 0; i < m_nonMatchingFiles.Length; ++i)
                {
                    m_nonMatchingFiles[i] = Path.GetFileName(m_nonMatchingFiles[i]);
                }
                Array.Sort(m_nonMatchingFiles);
            }
            for (int i = 0; i < m_files.Length; ++i)
            {
                m_files[i] = Path.GetFileName(m_files[i]);
            }
            Array.Sort(m_files);
            BuildContent();
            m_newDirectory = null;
        }
    }

    protected void BuildContent()
    {
        m_directoriesWithImages = new GUIContent[m_directories.Length];
        for (int i = 0; i < m_directoriesWithImages.Length; ++i)
        {
            m_directoriesWithImages[i] = new GUIContent(m_directories[i], DirectoryImage);
        }
        m_nonMatchingDirectoriesWithImages = new GUIContent[m_nonMatchingDirectories.Length];
        for (int i = 0; i < m_nonMatchingDirectoriesWithImages.Length; ++i)
        {
            m_nonMatchingDirectoriesWithImages[i] = new GUIContent(m_nonMatchingDirectories[i], DirectoryImage);
        }
        m_filesWithImages = new GUIContent[m_files.Length];
        for (int i = 0; i < m_filesWithImages.Length; ++i)
        {
            m_filesWithImages[i] = new GUIContent(m_files[i], FileImage);
        }
        m_nonMatchingFilesWithImages = new GUIContent[m_nonMatchingFiles.Length];
        for (int i = 0; i < m_nonMatchingFilesWithImages.Length; ++i)
        {
            m_nonMatchingFilesWithImages[i] = new GUIContent(m_nonMatchingFiles[i], FileImage);
        }
    }

    public void OnGUI()
    {
        if (!m_toggle) return;

        GUI.skin = m_skin;

        GUILayout.BeginArea(
            m_screenRect,
            m_name,
            GUI.skin.window
        );
        GUILayout.BeginHorizontal();
        for (int parentIndex = 0; parentIndex < m_currentDirectoryParts.Length; ++parentIndex)
        {
            if (parentIndex == m_currentDirectoryParts.Length - 1)
            {
                GUILayout.Label(m_currentDirectoryParts[parentIndex], CentredText);
            }
            else if (GUILayout.Button(m_currentDirectoryParts[parentIndex]))
            {
                string parentDirectoryName = m_currentDirectory;
                for (int i = m_currentDirectoryParts.Length - 1; i > parentIndex; --i)
                {
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
            DirectoryDoubleClickCallback
        );
        if (m_selectedDirectory > -1)
        {
            m_selectedFile = m_selectedNonMatchingDirectory = -1;
        }
        m_selectedNonMatchingDirectory = xGUILayout.SelectionList(
            m_selectedNonMatchingDirectory,
            m_nonMatchingDirectoriesWithImages,
            NonMatchingDirectoryDoubleClickCallback
        );
        if (m_selectedNonMatchingDirectory > -1)
        {
            m_selectedDirectory = m_selectedFile = -1;
        }
        GUI.enabled = BrowserType == FileBrowserType.File;
        m_selectedFile = xGUILayout.SelectionList(
            m_selectedFile,
            m_filesWithImages,
            FileDoubleClickCallback
        );
        GUI.enabled = true;
        if (m_selectedFile > -1)
        {
            m_selectedDirectory = m_selectedNonMatchingDirectory = -1;
        }
        GUI.enabled = false;
        xGUILayout.SelectionList(
            -1,
            m_nonMatchingFilesWithImages
        );
        GUI.enabled = true;
        GUILayout.EndScrollView();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Cancel", GUILayout.Width(50)))
        {
            if (m_callback != null) m_callback(null);
        }
        if (BrowserType == FileBrowserType.File)
        {
            GUI.enabled = m_selectedFile > -1;
        }
        else
        {
            if (SelectionPattern == null)
            {
                GUI.enabled = m_selectedDirectory > -1;
            }
            else
            {
                GUI.enabled = m_selectedDirectory > -1 ||
                                (
                                    m_currentDirectoryMatches &&
                                    m_selectedNonMatchingDirectory == -1 &&
                                    m_selectedFile == -1
                                );
            }
        }
        if (GUILayout.Button("Select", GUILayout.Width(50)))
        {
            if (BrowserType == FileBrowserType.File)
            {
                if (m_callback != null) m_callback(Path.Combine(m_currentDirectory, m_files[m_selectedFile]));
            }
            else
            {
                if (m_selectedDirectory > -1)
                {
                    if (m_callback != null) m_callback(Path.Combine(m_currentDirectory, m_directories[m_selectedDirectory]));
                }
                else
                {
                    if (m_callback != null) m_callback(m_currentDirectory);
                }
            }
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        if (Event.current.type == EventType.Repaint)
        {
            SwitchDirectoryNow();
        }
    }

    protected void FileDoubleClickCallback(int i)
    {
        if (BrowserType == FileBrowserType.File)
        {
            if (m_callback != null) m_callback(Path.Combine(m_currentDirectory, m_files[i]));
        }
    }

    protected void DirectoryDoubleClickCallback(int i)
    {
        SetNewDirectory(Path.Combine(m_currentDirectory, m_directories[i]));
    }

    protected void NonMatchingDirectoryDoubleClickCallback(int i)
    {
        SetNewDirectory(Path.Combine(m_currentDirectory, m_nonMatchingDirectories[i]));
    }
}