using UnityEngine;

namespace SanAndreasUnity.Utilities
{
	
	public class DontDestroyOnLoad : MonoBehaviour
	{
		
		void Awake ()
		{
			if (null == this.transform.parent)
			{
				DontDestroyOnLoad (this.gameObject);
			}
		}

	}

}
