using System;
using System.Collections.Generic;
using SanAndreasUnity.Importing.GXT;
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
			this.windowName = "GXTWindow";
			SetUpGxtTextAreaStyle();

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

		}

		protected override void OnWindowGUI()
		{
			base.OnWindowGUI();
			GUILayout.BeginHorizontal();

			#region TableName
			GUILayout.BeginVertical(GUILayout.Width(WindowWidth / 3));
			GUILayout.Label("TableName");
			_tableNameScrollPos = GUILayout.BeginScrollView(_tableNameScrollPos);
			foreach (var gxtSubTableName in GXT.Gxt.SubTableNames)
			{
				if (GUILayout.Button(gxtSubTableName))
				{
					crcList = GXT.Gxt.TableEntryNameDict[gxtSubTableName];
				}
			}
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			#endregion

			#region CRC
			GUILayout.BeginVertical(GUILayout.Width(WindowWidth / 3));
			GUILayout.Label("CRC");
			_crcScrollPos = GUILayout.BeginScrollView(_crcScrollPos);
			foreach (var crc in crcList)
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

				GUILayout.TextArea(GXT.Gxt.EntryNameWordDict[curCrc.Value], _gxtTextAreaStyle);
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

		}

		protected override void OnWindowStart()
		{
			base.OnWindowStart();
		}

		protected override void OnWindowOpened()
		{
			base.OnWindowOpened();
		}

		protected override void OnWindowClosed()
		{
			base.OnWindowClosed();
		}

		protected override void OnLoaderFinished()
		{
			base.OnLoaderFinished();
		}

		protected override void OnWindowGUIBeforeContent()
		{
			base.OnWindowGUIBeforeContent();
		}

		protected override void OnWindowGUIAfterContent()
		{
			base.OnWindowGUIAfterContent();
		}
	}
}
