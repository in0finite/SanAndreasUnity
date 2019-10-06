using System;
using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Importing.GXT;
using SanAndreasUnity.Utilities;
using UnityEngine;

namespace SanAndreasUnity.UI
{
	class GXTWindow : PauseMenuWindow
	{
		private Vector2 _tableNameScrollPos;
		private Vector2 _crcScrollPos;
		private const float WindowWidth = 600;
		private const float WindowHeight = 400;
		private Int32? curCrc = null;

		private List<Int32> crcList = new List<int>();
		private GUIStyle _gxtTextAreaStyle;

		GXTWindow()
		{
			//this will use as menu item name as well as window title
			this.windowName = "GXT Viewer";

		}

		void SetUpGxtTextAreaStyle()
		{
			_gxtTextAreaStyle = new GUIStyle();
			_gxtTextAreaStyle.wordWrap = true;
			_gxtTextAreaStyle.normal.textColor = Color.white;
			_gxtTextAreaStyle.padding = new RectOffset(20, 20, 10, 5);
		}

		void Start()
		{
			this.RegisterButtonInPauseMenu();
			this.windowRect = Utilities.GUIUtils.GetCenteredRect(new Vector2(WindowWidth, WindowHeight));
			SetUpGxtTextAreaStyle();

		}


		private int m_currentPageNumber = 1;
		private int numCrcPerPage = 40;
		protected override void OnWindowGUI()
		{
			base.OnWindowGUI();
			GUILayout.BeginHorizontal();

			#region TableName
			GUILayout.BeginVertical(GUILayout.Width(WindowWidth / 3));
			GUILayout.Label("TableName");
			_tableNameScrollPos = GUILayout.BeginScrollView(_tableNameScrollPos);
			foreach (var gxtSubTableName in GXT.Current.SubTableNames)
			{
				if (GUILayout.Button(gxtSubTableName))
				{
					crcList = GXT.Current.TableEntryNameDict[gxtSubTableName];
				}
			}
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			#endregion

			#region CRC
			GUILayout.BeginVertical(GUILayout.Width(WindowWidth / 3));
			GUILayout.Label("CRC");
			if (crcList.Count > numCrcPerPage) //only show pages when we really need
			{
				m_currentPageNumber = GUIUtils.DrawPagedViewNumbers(GUILayoutUtility.GetRect(WindowWidth/3,20),
				m_currentPageNumber, crcList.Count, numCrcPerPage);
			}

			_crcScrollPos = GUILayout.BeginScrollView(_crcScrollPos);

			foreach (var crc in crcList.Skip((m_currentPageNumber-1)*numCrcPerPage).Take(numCrcPerPage))
			{
				if (GUILayout.Button(crc.ToString()))
				{
					curCrc = crc;
				}
			}
			GUILayout.EndScrollView();
			GUILayout.EndVertical();

			#endregion
			GUILayout.BeginVertical(GUILayout.Width(WindowWidth / 3));
			GUILayout.Label("text");
			if (curCrc.HasValue)
			{

				GUILayout.TextArea(GXT.Current.EntryNameWordDict[curCrc.Value], _gxtTextAreaStyle);
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

		}
	}
}
