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



	    void Awake()
	    {
	        
	        Instance = this;

	        // enable touch input by default on mobile platforms
	    	if (Application.isMobilePlatform)
	    	{
	    		this.UseTouchInput = true;
	    	}

	    }

	}

}
