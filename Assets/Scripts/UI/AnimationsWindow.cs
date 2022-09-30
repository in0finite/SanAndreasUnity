using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Importing.Animation;
using SanAndreasUnity.Behaviours;
using UGameCore.Utilities;
using System.Linq;

namespace SanAndreasUnity.UI {

	public class AnimationsWindow : PauseMenuWindow {

		private	Vector2	m_scrollViewPos = Vector2.zero;
		private Vector2 m_packagesScrollViewPos = Vector2.zero;
		private bool m_displayPackages = true;
		private bool m_displayWalkcycleAnims = false;
		private bool m_displayAnimStats = false;
		private float m_minScrollViewHeight = 200;
		private float m_maxScrollViewHeight = 600;
		private int m_selectedPackageIndex = -1;

		private string[] m_ifpFileNames = new string[] {};
		private string[] m_currentlyDisplayedIfpFileNames = new string[] {};
		private string m_ifpSearchText = "";


		AnimationsWindow() {

			// set default parameters

			this.windowName = "Animations";
			this.useScrollView = false;

		}

		void Start () {
			
			this.RegisterButtonInPauseMenu ();

			// adjust rect
			this.windowRect = GUIUtils.GetCenteredRect( new Vector2( 500, Screen.height * 0.85f ) );
		}

		protected override void OnLoaderFinished()
		{
			base.OnLoaderFinished();

			// cache all IFPs
			m_ifpFileNames = Importing.Archive.ArchiveManager.GetFileNamesWithExtension(".ifp").Select(fileName => fileName.Substring(0, fileName.Length - 4)).ToArray();

			m_currentlyDisplayedIfpFileNames = m_ifpFileNames;
		}


		protected override void OnWindowGUI ()
		{
			
			bool playerExists = Ped.Instance != null;


		//	float headerHeight = m_displayAnimStats ? 300 : 100;

		//	m_headerScrollViewPos = GUILayout.BeginScrollView (m_headerScrollViewPos, GUILayout.Height(headerHeight));

			if (NetUtils.IsServer)
			{
				if (playerExists)
					Ped.Instance.shouldPlayAnims = !GUILayout.Toggle( !Ped.Instance.shouldPlayAnims, "Override player anims" );
			}

			m_displayPackages = GUILayout.Toggle(m_displayPackages, "Display packages");
			
			m_displayWalkcycleAnims = GUILayout.Toggle( m_displayWalkcycleAnims, "Display walkcycle anims");

			m_displayAnimStats = GUILayout.Toggle( m_displayAnimStats, "Display anim stats");

			// display anim stats
			if (m_displayAnimStats && playerExists) {
				DisplayAnimStats ();
			}

		//	GUILayout.EndScrollView ();

			GUILayout.Space (10);


			if (m_displayPackages)
			{
				// display loaded ifp packages and their anims
				this.DisplayPackages (playerExists);
			}
			else
			{
				// display anim groups and their anims
				this.DisplayAnimGroups (playerExists);
			}


		}

		private void DisplayAnimStats ()
		{

			GUILayout.Space (5);

			var model = Ped.Instance.PlayerModel;

			int numActiveClips = model.AnimComponent.OfType<AnimationState>().Where(a => a.enabled).Count();
			GUILayout.Label("Currently played anims [" + numActiveClips + "] :");

			// display all currently played clips

			foreach (AnimationState animState in model.AnimComponent) {

				if (!animState.enabled)
					continue;

				DisplayStatsForAnim (animState);
			}


			if (model.LastAnimState != null) {
				GUILayout.Space (3);
				GUILayout.Label ("Last played anim:");
				DisplayStatsForAnim (model.LastAnimState);
			}

			if (model.LastSecondaryAnimState != null) {
				GUILayout.Space (3);
				GUILayout.Label ("Last secondary played anim:");
				DisplayStatsForAnim (model.LastSecondaryAnimState);
			}

			GUILayout.Space (7);

			GUILayout.Label ("Root frame velocity: " + model.RootFrame.LocalVelocity);

		}

		private void DisplayStatsForAnim (AnimationState animState)
		{
		//	GUILayout.BeginHorizontal ();

			var clip = animState.clip;

			GUILayout.Label (string.Format ("name: {0}, length: {1}, frame rate: {2}, wrap mode: {3}, speed: {4}, " +
				"normalized speed: {5}, time: {6}, normalized time: {7}, time perc: {8}", 
				clip.name, animState.length, clip.frameRate, animState.wrapMode, animState.speed, animState.normalizedSpeed, 
				animState.time, animState.normalizedTime, animState.GetTimePerc ()
			));

		//	GUILayout.EndHorizontal ();
		}

		private void DisplayAnimGroups(bool playerExists)
		{

			m_scrollViewPos = GUILayout.BeginScrollView (m_scrollViewPos, GUILayout.MinHeight(m_minScrollViewHeight), GUILayout.MaxHeight(m_maxScrollViewHeight));

			float elementHeight = 25;

			foreach (var pair in Importing.Animation.AnimationGroup.AllLoadedGroups) {
				
				GUILayout.Space (5);
				GUILayout.Label ("Name: " + pair.Key);

				foreach (var pair2 in pair.Value) {

					if (!m_displayWalkcycleAnims && pair2.Key == AnimGroup.WalkCycle)
						continue;
					
					GUILayout.Label ("Type: " + pair2.Key);

					var animGroup = pair2.Value;

					for (int i=0; i < animGroup.Animations.Length; i++) {
						string animName = animGroup.Animations[i];

						if (playerExists) {
							// display button which will play the anim
							if (GUILayout.Button (animName, GUILayout.Height(elementHeight))) {
								this.PlayAnim(new AnimId(animGroup.Type, AnimIndexUtil.Get(i)));
							}
						} else {
							GUILayout.Label (animName, GUILayout.Height(elementHeight));
						}
					}
				}
			}

			GUILayout.EndScrollView();

		}

		void DisplayPackages(bool playerExists)
		{
			string[] packageNames = m_currentlyDisplayedIfpFileNames;
			
			float animHeight = 25;

			// search box
			GUILayout.BeginHorizontal();
			GUILayout.Label ("Ifp name:");
			GUILayout.Space(5);
			m_ifpSearchText = GUILayout.TextField(m_ifpSearchText, GUILayout.Width(120));
			GUILayout.Space(5);
			if (GUILayout.Button("Search"))
			{
				this.SearchIfps();
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			m_packagesScrollViewPos = GUILayout.BeginScrollView(m_packagesScrollViewPos, GUILayout.MinHeight(50));
			int newPackageIndex = GUILayout.Toolbar(m_selectedPackageIndex, packageNames, GUILayout.MinHeight(animHeight));
			GUILayout.EndScrollView();

			if (newPackageIndex != m_selectedPackageIndex)
			{
				// changed selected package

				m_selectedPackageIndex = newPackageIndex;

				// load the package if it was not loaded so far
				if (newPackageIndex >= 0)
				{
					bool packageLoaded = Importing.Conversion.Animation.Loaded.ContainsKey(packageNames[newPackageIndex]);
					if (!packageLoaded)
						Importing.Conversion.Animation.LoadPackageOnly(packageNames[newPackageIndex]);
				}
				
			}

			if (newPackageIndex < 0)
				return;

			string selectedIfpName = packageNames[m_selectedPackageIndex];

			if (! Importing.Conversion.Animation.Loaded.ContainsKey(selectedIfpName))
				return;

			var package = Importing.Conversion.Animation.Loaded[selectedIfpName].AnimPackage;
			var clips = package.Clips;

			GUILayout.Space (10);

			// display all clips from this IFP package

			m_scrollViewPos = GUILayout.BeginScrollView (m_scrollViewPos, GUILayout.MinHeight(m_minScrollViewHeight), GUILayout.MaxHeight(m_maxScrollViewHeight));

			for (int i = 0; i < clips.Length; i++)
			{
				var clip = clips [i];
				if (playerExists)
				{
					if (GUILayout.Button (clip.Name, GUILayout.Height(animHeight)))
					{
						// play this anim
						this.PlayAnim(new AnimId (package.FileName, clip.Name));
					}
				}
				else
				{
					GUILayout.Label (clip.Name, GUILayout.Height(animHeight));
				}
			}

			GUILayout.EndScrollView ();

		}

		void SearchIfps()
		{
			string text = m_ifpSearchText.Trim();

			if (string.IsNullOrWhiteSpace(text))
			{
				m_currentlyDisplayedIfpFileNames = m_ifpFileNames;
				return;
			}

			m_currentlyDisplayedIfpFileNames = m_ifpFileNames.Where(fileName => fileName.Contains(text)).ToArray();
			m_selectedPackageIndex = -1;
		}

		void PlayAnim(AnimId animId)
		{
			Ped.Instance.PlayerModel.ResetModelState ();
			var state = Ped.Instance.PlayerModel.PlayAnim (animId);
			state.wrapMode = WrapMode.Loop;
		}

	}

}
