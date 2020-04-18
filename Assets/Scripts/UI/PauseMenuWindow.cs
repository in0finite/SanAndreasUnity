using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.UI {
	
	public class PauseMenuWindow : MonoBehaviour {

		public	string	windowName = "";

		[SerializeField]	private	bool	m_isOpenedByDefaultInMainMenu = false;
		[SerializeField]	private	bool	m_isOpenedByDefaultInPauseMenu = false;

		private	bool	m_isOpened = false;
		public bool IsOpened {
			get { return this.m_isOpened; } 
			set { 
				if (m_isOpened == value)
					return;
				
				m_isOpened = value;

				if (this.IsRegisteredInMainMenu)
					MainMenu.SetEntryColor(m_mainMenuEntry, m_isOpened ? MainMenu.Instance.openedWindowTextColor : MainMenu.Instance.ClosedWindowTextColor);

				if (m_isOpened)
				{
					this.OnWindowOpened ();
				}
				else
				{
					if (this.DestroyOnClose)
						Destroy(this);
					this.OnWindowClosed ();
				}
			}
		}

		private bool ShouldBeDrawn {
			get {
				if (Behaviours.Loader.IsLoading)
					return false;

				if (!this.IsOpened)
					return false;

				if (!PauseMenu.IsOpened && !Behaviours.GameManager.IsInStartupScene)
					return false;

				return true;
			}
		}

		[SerializeField] private bool m_destroyOnClose = false;
		public bool DestroyOnClose { get { return m_destroyOnClose; } set { m_destroyOnClose = value; } }

		private	static	int	lastWindowId = 1352345;
		private	int	windowId = lastWindowId++;
		public int WindowId { get { return this.windowId; } }

		public	Rect	windowRect = Utilities.GUIUtils.GetCenteredRectPerc(new Vector2(0.5f, 0.5f));
		public	Vector2	WindowSize { get { return this.windowRect.size; } }

		public	bool	useScrollView = false;
		protected	Vector2	scrollPos = Vector2.zero;
		protected GUIStyle m_scrollViewStyle = null;

		protected	bool	isDraggable = true;
		public bool IsDraggable { get { return this.isDraggable; } }

		protected	bool	isModal = false;
		public bool IsModal { get { return this.isModal; } }

		protected	bool	m_hasExitButton = true;
		protected	bool	m_hasMinimizeButton = true;

		private	bool	m_isMinimized = false;
		public bool IsMinimized { get { return this.m_isMinimized; } set { m_isMinimized = value; } }

		public	const	float	kMinimizedWindowHeight = 45;

		private	bool	m_hasStarted = false;

		[SerializeField]	private	float	m_spaceBeforeContent = 0f;
		public float SpaceBeforeContent { get { return m_spaceBeforeContent; } set { m_spaceBeforeContent = value; } }
		[SerializeField]	private	float	m_spaceAfterContent = 0f;
		public float SpaceAfterContent { get { return m_spaceAfterContent; } set { m_spaceAfterContent = value; } }

		[SerializeField]	private	bool	m_registerInMainMenuOnStart = false;
		public	bool	IsRegisteredInMainMenu { get; private set; }
		private MenuEntry m_mainMenuEntry;
		[SerializeField] int m_sortPriorityForMainMenu = 0;

		private static GameObject s_windowsContainer;



		public static T Create<T>() where T : PauseMenuWindow
		{
			if (null == s_windowsContainer)
			{
				s_windowsContainer = new GameObject("Windows");
				DontDestroyOnLoad( s_windowsContainer );
			}

			T window = s_windowsContainer.AddComponent<T>();
			
			return window;
		}

		public void DestroyWindow()
		{
			Destroy(this);
		}

		protected virtual void Awake()
		{

		}

		protected virtual void OnEnable()
		{
			Behaviours.UIManager.onGUI += this.OnGUICustom;
		}

		protected virtual void OnDisable()
		{
			Behaviours.UIManager.onGUI -= this.OnGUICustom;
		}

		void WindowStart() {

			if (m_registerInMainMenuOnStart)
				this.RegisterInMainMenu ();

			if (m_isOpenedByDefaultInMainMenu)
				this.IsOpened = true;

			this.OnWindowStart ();
		}

		/// <summary>
		/// Called on first OnGUI().
		/// </summary>
		protected virtual void OnWindowStart() {
			
		}

		protected virtual void OnWindowOpened() {

		}

		protected virtual void OnWindowClosed() {

		}


		protected virtual void OnLoaderFinished ()
		{
			if (m_isOpenedByDefaultInPauseMenu)
			{
				this.IsOpened = true;
			}
		}


		void OnGUICustom() {

			if (null == m_scrollViewStyle)
				m_scrollViewStyle = GUI.skin.scrollView;

			if (!m_hasStarted) {
				m_hasStarted = true;
				this.WindowStart ();
			}

			
			if (!this.ShouldBeDrawn)
			{
				return;
			}


			Rect newRect;
			Rect inputRect = this.windowRect;
			if(this.IsMinimized)
				inputRect.height = kMinimizedWindowHeight;
			
			if (this.isModal)
				newRect = GUI.ModalWindow (this.windowId, inputRect, WindowFunction, this.windowName);
			else
				newRect = GUI.Window( this.windowId, inputRect, WindowFunction, this.windowName );
			
			if (this.IsMinimized)
				this.windowRect.position = newRect.position;	// only copy position
			else
				this.windowRect = newRect;

		}

		void WindowFunction( int id ) {


			float buttonWidth = 16;
			float buttonHeight = 16;
			float buttonYOffset = 2;

			// exit button
			if (m_hasExitButton) {
				Color exitButtonColor = Color.Lerp (Color.red, Color.white, 0.0f);
			//	exitButtonColor.a = 0.7f;
				if (Utilities.GUIUtils.ButtonWithColor (new Rect (this.windowRect.width - buttonWidth - 2, buttonYOffset, buttonWidth, buttonHeight), 
					   "x", exitButtonColor)) {
					this.IsOpened = false;
				}
			}

			// minimize button
			if (m_hasMinimizeButton) {
				if (GUI.Button (new Rect (this.windowRect.width - buttonWidth - 2 - buttonWidth - 2, buttonYOffset, buttonWidth, buttonHeight), "-")) {
					this.IsMinimized = !this.IsMinimized;
				}
			}


			if (!this.IsMinimized) {
				// draw contents inside window

				if (this.SpaceBeforeContent > 0)
					GUILayout.Space (this.SpaceBeforeContent);

				this.OnWindowGUIBeforeContent ();

				if (this.useScrollView)
					this.scrollPos = GUILayout.BeginScrollView (this.scrollPos, m_scrollViewStyle);

				this.OnWindowGUI ();

				if (this.useScrollView)
					GUILayout.EndScrollView ();

				if (this.SpaceAfterContent > 0)
					GUILayout.Space (this.SpaceAfterContent);

				this.OnWindowGUIAfterContent ();
			}


			if (this.isDraggable)
				GUI.DragWindow ();
			
		}

		protected virtual void OnWindowGUIBeforeContent() {

		}

		protected virtual void OnWindowGUI() {
			
		}

		protected virtual void OnWindowGUIAfterContent() {

		}


		public	void	RegisterButtonInPauseMenu() {

			PauseMenu.onDrawItems += this.OnPauseMenuGUI;

		}

		public	void	UnRegisterButtonInPauseMenu() {

			PauseMenu.onDrawItems -= this.OnPauseMenuGUI;

		}

		private	void	OnPauseMenuGUI() {

			// display button for opening/closing window

			var originalColor = GUI.contentColor;
			if (this.IsOpened)
				GUI.contentColor = PauseMenu.Instance.openedWindowTextColor;

		//	string text = this.IsOpened ? "Hide " + this.windowName : "Show " + this.windowName;
			string text = this.windowName;

			if (GUILayout.Button (text)) {
				this.IsOpened = ! this.IsOpened;
			}

			GUI.contentColor = originalColor;
		}

		public void RegisterInMainMenu ()
		{
			if (this.IsRegisteredInMainMenu)
				return;

			this.IsRegisteredInMainMenu = true;

			m_mainMenuEntry = new MenuEntry(){ name = this.windowName, sortPriority = m_sortPriorityForMainMenu, 
				drawAction = () => this.OnMainMenuGUI(), clickAction = this.OnButtonClickedInMainMenu };
			MainMenu.RegisterMenuEntry (m_mainMenuEntry);
		}

		private void OnMainMenuGUI ()
		{
			// draw a button in main menu

			var originalColor = GUI.contentColor;
			if (this.IsOpened)
				GUI.contentColor = MainMenu.Instance.openedWindowTextColor;

			if (MainMenu.DrawMenuEntry (this.windowName))
			{
				this.OnButtonClickedInMainMenu();
			}

			GUI.contentColor = originalColor;
		}

		private void OnButtonClickedInMainMenu()
		{
			this.IsOpened = !this.IsOpened;
		}

	}

}
