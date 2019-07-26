using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours
{

	public class UIManager : MonoBehaviour
	{

		public static UIManager Instance { get; private set; }

		bool m_useTouchInput = false;
		public bool UseTouchInput
		{
			get => m_useTouchInput;
			set
			{
				m_useTouchInput = value;
				var ci = CustomInput.Instance;
				if (null == ci)	// this can happen when calling from Awake()
					ci = FindObjectOfType<CustomInput>();
				ci.IsActive = m_useTouchInput;
			}
		}

		bool m_isFirstOnGUI = true;
		
		bool m_shouldChangeFontSize = false;
		int m_imguiFontSize = 0;
		public int ImguiFontSize { get => m_imguiFontSize; set { m_imguiFontSize = value; m_shouldChangeFontSize = true; } }

		[SerializeField] int m_defaultFontSizeOnMobile = 16;

		[SerializeField] float m_scrollbarSizeMultiplierOnMobile = 2f;

		// note: UIManager's OnGUI() should execute before other OnGUI()s, because other scripts may try to create their own
		// style from existing styles



	    void Awake()
	    {
	        
	        Instance = this;

	        // enable touch input by default on mobile platforms
	    	if (Application.isMobilePlatform)
	    	{
	    		this.UseTouchInput = true;
	    	}

	    	// set default font size on mobile platforms
	    	if (Application.isMobilePlatform)
	    	{
	    		this.ImguiFontSize = m_defaultFontSizeOnMobile;
	    	}

	    }

	    void OnGUI()
	    {
	    	
	    	if (m_isFirstOnGUI)
	    	{
	    		m_isFirstOnGUI = false;
	    		this.SetupGui();
	    	}

	    	if (m_shouldChangeFontSize)
	    	{
	    		m_shouldChangeFontSize = false;
	    		SetStylesFontSize(m_imguiFontSize);
	    	}

	    }

	    void SetupGui()
	    {

	    	if (Application.isMobilePlatform)
	    	{

	    		var skin = GUI.skin;

	    		// make scrollbars wider

	    		var styles = new GUIStyle[]{skin.horizontalScrollbar, skin.horizontalScrollbarLeftButton, skin.horizontalScrollbarRightButton, skin.horizontalScrollbarThumb};
	    		foreach (var style in styles)
	    		{
	    			//Debug.LogFormat("style: {0}, height: {1}", style.name, style.fixedHeight);
	    			style.fixedHeight *= m_scrollbarSizeMultiplierOnMobile;
	    		}

	    		styles = new GUIStyle[]{skin.verticalScrollbar, skin.verticalScrollbarDownButton, skin.verticalScrollbarThumb, skin.verticalScrollbarUpButton};
	    		foreach (var style in styles)
	    		{
	    			//Debug.LogFormat("style: {0}, width: {1}", style.name, style.fixedWidth);
	    			style.fixedWidth *= m_scrollbarSizeMultiplierOnMobile;
	    		}

	    	}

	    }

	    static void SetStylesFontSize(int newFontSize)
	    {
	    	// change font size for styles
	    	var skin = GUI.skin;
    		var styles = new GUIStyle[]{skin.button, skin.label, skin.textArea, skin.textField, skin.toggle, skin.window, skin.box};
    		foreach (var style in styles)
    		{
    			//Debug.LogFormat("style: {0}, font size: {1}", style.name, style.fontSize);
    			style.fontSize = newFontSize;
    		}
	    }

	}

}
